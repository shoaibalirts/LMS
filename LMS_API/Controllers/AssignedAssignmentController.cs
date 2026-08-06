using LMS_API.Models.DTO.AssignedAssignment;
using LMS_API.Models.DTO.AssignedAssignmentSet;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{

	//Test
	[Route("api/assignedassignment")]
	[ApiController]
	[Authorize]
	public class AssignedAssignmentController : ControllerBase
	{
		private readonly IAssignedAssignmentService _assignedAssignmentService;
		private readonly ITokenService _tokenService;
		private readonly ILogger<AssignedAssignmentController> _logger;

		public AssignedAssignmentController(IAssignedAssignmentService assignedAssignmentService, ITokenService tokenService, ILogger<AssignedAssignmentController> logger)
		{
			_assignedAssignmentService = assignedAssignmentService;
			_tokenService = tokenService;
			_logger = logger;
		}

		[Authorize(Roles = "Teacher")]
		[HttpPost("sets")]
		public async Task<ActionResult<AssignedAssignmentSetReadDTO>> CreateAssignedAssignmentSet([FromBody] AssignedAssignmentSetCreateDTO dto)
		{
			if (dto == null)
			{
				return BadRequest("Assigned set payload is required.");
			}

			if (!_tokenService.TryGetUserId(User, out var teacherId))
			{
				return Unauthorized("Missing or invalid teacher identity.");
			}

			try
			{
				var created = await _assignedAssignmentService.CreateAssignedAssignmentSetAsync(dto, teacherId);
				if (created == null)
				{
					return BadRequest("Could not create assigned assignment set.");
				}

				return CreatedAtAction(nameof(GetTeacherAssignedSets), new { }, created);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Authorize(Roles = "Teacher")]
		[HttpPost("sets/class")]
		public async Task<ActionResult<IEnumerable<AssignedAssignmentSetReadDTO>>> CreateAssignedAssignmentSetForClass([FromBody] AssignedAssignmentSetCreateForClassDTO dto)
		{
			if (dto == null)
			{
				return BadRequest("Assigned class payload is required.");
			}

			if (!_tokenService.TryGetUserId(User, out var teacherId))
			{
				return Unauthorized("Missing or invalid teacher identity.");
			}

			try
			{
				var created = await _assignedAssignmentService.CreateAssignedAssignmentSetForClassAsync(dto, teacherId);
				return Ok(created);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Authorize(Roles = "Student")]
		[HttpGet("student")]
		public async Task<ActionResult<IEnumerable<AssignedAssignmentSetReadDTO>>> GetStudentAssignedSets()
		{
			if (!_tokenService.TryGetUserId(User, out var studentId))
			{
				return Unauthorized("Missing or invalid student identity.");
			}

			var sets = await _assignedAssignmentService.GetAssignedSetsForStudentAsync(studentId);
			return Ok(sets);
		}

		[Authorize(Roles = "Teacher")]
		[HttpGet("teacher")]
		public async Task<ActionResult<IEnumerable<AssignedAssignmentSetReadDTO>>> GetTeacherAssignedSets()
		{
			if (!_tokenService.TryGetUserId(User, out var teacherId))
			{
				return Unauthorized("Missing or invalid teacher identity.");
			}

			var sets = await _assignedAssignmentService.GetAssignedSetsForTeacherAsync(teacherId);
			return Ok(sets);
		}

		[Authorize(Roles = "Teacher")]
		[HttpDelete("sets/{assignedAssignmentSetId:int}")]
		public async Task<IActionResult> RevokeAssignedSet(int assignedAssignmentSetId)
		{
			if (!_tokenService.TryGetUserId(User, out var teacherId))
			{
				return Unauthorized("Missing or invalid teacher identity.");
			}

			var revoked = await _assignedAssignmentService.RevokeAssignedAssignmentSetAsync(assignedAssignmentSetId, teacherId);
			if (!revoked)
			{
				return NotFound("Assigned assignment set was not found.");
			}

			_logger.LogInformation("Assigned set revoked teacher_id={TeacherId} assigned_set_id={AssignedSetId}", teacherId, assignedAssignmentSetId);
			return Ok(new { message = "Assigned set revoked." });
		}

		[Authorize(Roles = "Teacher")]
		[HttpDelete("students/{studentId:int}/sets")]
		public async Task<IActionResult> RevokeAllAssignedSetsForStudent(int studentId)
		{
			if (!_tokenService.TryGetUserId(User, out var teacherId))
			{
				return Unauthorized("Missing or invalid teacher identity.");
			}

			try
			{
				var count = await _assignedAssignmentService.RevokeAllAssignedAssignmentSetsForStudentAsync(studentId, teacherId);
				return Ok(new { message = "Assigned sets revoked.", revokedCount = count });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Authorize(Roles = "Teacher")]
		[HttpDelete("assignments/{assignedAssignmentId:int}")]
		public async Task<IActionResult> DeleteAssignedAssignment(int assignedAssignmentId)
		{
			if (!_tokenService.TryGetUserId(User, out var teacherId))
			{
				return Unauthorized("Missing or invalid teacher identity.");
			}

			var deleted = await _assignedAssignmentService.DeleteAssignedAssignmentAsync(assignedAssignmentId, teacherId);
			if (!deleted)
			{
				return NotFound("Assigned assignment was not found.");
			}

			_logger.LogInformation("Assigned assignment deleted teacher_id={TeacherId} assigned_assignment_id={AssignedAssignmentId}", teacherId, assignedAssignmentId);
			return Ok(new { message = "Assigned assignment deleted." });
		}

		[Authorize(Roles = "Student")]
		[HttpPost("{assignedAssignmentId:int}/submit")]
		public async Task<ActionResult<AssignedAssignmentReadDTO>> SubmitStudentResult(int assignedAssignmentId, [FromForm] IFormFile file)
		{
			if (!_tokenService.TryGetUserId(User, out var studentId))
			{
				return Unauthorized("Missing or invalid student identity.");
			}

			if (file == null || file.Length == 0)
			{
				return BadRequest("A non-empty PDF file is required.");
			}

			var extension = Path.GetExtension(file.FileName);
			if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
			{
				return BadRequest("Only PDF files are allowed.");
			}

			try
			{
				var updated = await _assignedAssignmentService.SubmitStudentResultAsync(assignedAssignmentId, studentId, file);
				if (updated == null)
				{
					return NotFound("Assigned assignment was not found.");
				}

				_logger.LogInformation("Assignment submitted student_id={StudentId} assignment_id={AssignmentId}", studentId, assignedAssignmentId);

				return Ok(updated);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Authorize(Roles = "Teacher")]
		[HttpPut("{assignedAssignmentId:int}/feedback")]
		public async Task<ActionResult<AssignedAssignmentReadDTO>> UpdateFeedback(int assignedAssignmentId, [FromBody] AssignedAssignmentFeedbackUpdateDTO dto)
		{
			if (!_tokenService.TryGetUserId(User, out var teacherId))
			{
				return Unauthorized("Missing or invalid teacher identity.");
			}

			try
			{
				var updated = await _assignedAssignmentService.UpdateFeedbackAsync(assignedAssignmentId, teacherId, dto?.Feedback);
				if (updated == null)
				{
					return NotFound("Assigned assignment was not found.");
				}

				_logger.LogInformation("Feedback given teacher_id={TeacherId} assignment_id={AssignmentId}", teacherId, assignedAssignmentId);

				return Ok(updated);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
		}

		[HttpGet("{assignedAssignmentId:int}/result")]
		public async Task<IActionResult> GetStudentResultFile(int assignedAssignmentId)
		{
			if (!_tokenService.TryGetUserId(User, out var userId))
			{
				return Unauthorized("Missing or invalid identity.");
			}

			var isTeacher = User.IsInRole("Teacher");
			var isStudent = User.IsInRole("Student");

			if (!isTeacher && !isStudent)
			{
				return Forbid();
			}

			try
			{
				var fileData = await _assignedAssignmentService.GetStudentResultFileAsync(assignedAssignmentId, userId, isTeacher);
				if (fileData == null)
				{
					return NotFound("Submission file was not found.");
				}

				return File(fileData.Value.Content, fileData.Value.ContentType, fileData.Value.FileName);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
		}

		[HttpGet("sets/{assignedAssignmentSetId:int}/task-document")]
		public async Task<IActionResult> GetAssignmentSetTaskDocument(int assignedAssignmentSetId)
		{
			if (!_tokenService.TryGetUserId(User, out var userId))
			{
				return Unauthorized("Missing or invalid identity.");
			}

			var isTeacher = User.IsInRole("Teacher");
			var isStudent = User.IsInRole("Student");

			if (!isTeacher && !isStudent)
			{
				return Forbid();
			}

			try
			{
				var fileData = await _assignedAssignmentService.GetAssignmentSetDocumentFileAsync(assignedAssignmentSetId, userId, isTeacher);
				if (fileData == null)
				{
					return NotFound("Task document was not found.");
				}

				return File(fileData.Value.Content, fileData.Value.ContentType, fileData.Value.FileName);
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
		}
	}
}
