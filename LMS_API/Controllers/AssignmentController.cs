using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
    [Route("api/assignment")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class AssignmentController:ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ITokenService _tokenService;

        public AssignmentController(IAssignmentService assignmentService, ITokenService tokenService) 
        {
            _assignmentService = assignmentService;
            _tokenService = tokenService;
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
                if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                {
                    return Unauthorized("Missing or invalid teacher identity.");
                }

                var assignment = await _assignmentService.CreateAssignmentAsync(assignmentDTO, teacherId);
                if (assignment == null)
                {
                    return BadRequest("Could not create assignment. Check that the image is a valid type (jpg, png, gif, webp) and under 10 MB.");
                }
                return CreatedAtAction(nameof(CreateAssignment), new { id = assignment.Id }, assignment);// instead of Ok
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

            return Ok(new { message = $"Record deleted with ID: {id}" });
        }
    }
}
