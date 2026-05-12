using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Controllers
{
    [Route("api/assignment")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class AssignmentController:ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AssignmentController> _logger;

        public AssignmentController(IAssignmentService assignmentService, ITokenService tokenService, ILogger<AssignmentController> logger)
        {
            _assignmentService = assignmentService;
            _tokenService = tokenService;
            _logger = logger;
        }
        

        [HttpPost]
        public async Task<ActionResult<AssignmentReadDTO>> CreateAssignment([FromForm] AssignmentCreateDTO assignmentDTO)
        {
            try
            {
                if (assignmentDTO == null)
                {
                    return BadRequest("Assignment data is required");
                }

                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                {
                    return Unauthorized("Missing or invalid teacher identity.");
                }

                var assignment = await _assignmentService.CreateAssignmentAsync(assignmentDTO, teacherId);
                if (assignment == null)
                {
                    return BadRequest("Could not create assignment.");
                }

                _logger.LogInformation("Assignment created teacher_id={TeacherId} assignment_id={AssignmentId}", teacherId, assignment.Id);

                return CreatedAtAction(nameof(CreateAssignment), new { id = assignment.Id }, assignment);// instead of Ok
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                var details = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while saving the assignment: {details}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  $"An error occurred while creating the assignment: {ex.Message} ");
            }
        }

        [HttpGet("teacher")]
        public async Task<ActionResult<IEnumerable<AssignmentReadDTO>>> GetAllAssignments()
        {
            try
            {
                if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                {
                    return Unauthorized("Missing or invalid teacher identity.");
                }

                var assignments = await _assignmentService.GetAllAssignmentsAsync(teacherId);
                return Ok(assignments);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                    $"Error retrieving data: {ex.Message}");
            }
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
            {
                return Unauthorized("Missing or invalid teacher identity.");
            }

            var deleted = await _assignmentService.DeleteAssignmentAsync(id, teacherId);

            if (!deleted)
            {
                return NotFound($"Assignment with ID {id} not found.");
            }

            _logger.LogInformation("Assignment deleted teacher_id={TeacherId} assignment_id={AssignmentId}", teacherId, id);

            return Ok(new { message = $"Record deleted with ID: {id}" });
        }
    }
}
