using LMS_API.Models;
using LMS_API.Models.DTO.AssignmentSet;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
    [Route("api/assignmentset")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class AssignmentSetController:ControllerBase
    {
        private readonly IAssignmentSetService _assignmentSetService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AssignmentSetController> _logger;

        public AssignmentSetController(IAssignmentSetService assignmentSetService, ITokenService tokenService, ILogger<AssignmentSetController> logger)
        {
            _assignmentSetService = assignmentSetService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<AssignmentSetReadDTO>> CreateAssignmentSet(AssignmentSetCreateDTO assignmentSetDTO)
        {
            try
            {
                if (assignmentSetDTO == null)
                {
                    return BadRequest("Assignment Set data is required");
                }
                if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                {
                    return Unauthorized("Missing or invalid teacher identity.");
                }

                var assignmentSet = await _assignmentSetService.CreateAssignmentSetAsync(assignmentSetDTO, teacherId);
                if (assignmentSet == null)
                {
                    return BadRequest("Could not create assignment set.");
                }

                _logger.LogInformation("Assignment set created teacher_id={TeacherId} set_id={SetId}", teacherId, assignmentSet.Id);

                return CreatedAtAction(nameof(CreateAssignmentSet), new { id = assignmentSet.Id }, assignmentSet);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  $"An error occurred while creating the assignment set: {ex.Message} ");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssignmentSetReadDTO>>> GetAllAssignmentSet()
        {
            try
            {
                if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                {
                    return Unauthorized("Missing or invalid teacher identity.");
                }

                var assignmentSet = await _assignmentSetService.GetAllAssignmentSetsByTeacherAsync(teacherId);
				assignmentSet ??= Enumerable.Empty<AssignmentSetReadDTO>();
				return Ok(assignmentSet);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                    $"Error retrieving data: {ex.Message}");
            }
        }
        [HttpPost("{assignmentSetId:int}/add-assignment/{assignmentId:int}")]
        public async Task<ActionResult> AddAssignmentToSet(int assignmentSetId, int assignmentId)
        {
            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
            {
                return Unauthorized("Missing or invalid teacher identity.");
            }

            var result = await _assignmentSetService.AddAssignmentToSetAsync(assignmentSetId, assignmentId, teacherId);
            if (!result) return BadRequest("Could not add assignment to set.");

            _logger.LogInformation("Assignment added to set teacher_id={TeacherId} set_id={SetId} assignment_id={AssignmentId}", teacherId, assignmentSetId, assignmentId);

            return Ok("Assignment added successfully.");
        }

        [HttpDelete("{assignmentSetId:int}")]
        public async Task<IActionResult> DeleteAssignmentSet(int assignmentSetId)
        {
            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
            {
                return Unauthorized("Missing or invalid teacher identity.");
            }

            bool deleted;
            try
            {
                deleted = await _assignmentSetService.DeleteAssignmentSetAsync(assignmentSetId, teacherId);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }

            if (!deleted)
            {
                return NotFound($"Assignment set with ID {assignmentSetId} not found.");
            }

            _logger.LogInformation("Assignment set deleted teacher_id={TeacherId} set_id={SetId}", teacherId, assignmentSetId);
            return Ok(new { message = $"Assignment set deleted with ID: {assignmentSetId}" });
        }
    }
}
