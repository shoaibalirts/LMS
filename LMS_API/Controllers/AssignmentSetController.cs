using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Models.DTO.Assignmentset;
using LMS_API.Services;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
    [Route("api/assignmentset")]
    [ApiController]
    public class AssignmentSetController:ControllerBase
    {
        private readonly IAssignmentSetService _assignmentSetService;
        public AssignmentSetController(IAssignmentSetService assignmentSetService)
        {
            _assignmentSetService = assignmentSetService;
        }

        [HttpPost]
        public async Task<ActionResult<AssignmentSet>> CreateAssignmentSet(AssignmentSetCreateDTO assignmentSetDTO)
        {
            try
            {
                if (assignmentSetDTO == null)
                {
                    return BadRequest("Assignment Set data is required");
                }
                var assignmentSet = await _assignmentSetService.CreateAssignmentSetAsync(assignmentSetDTO);
                if (assignmentSet == null)
                {
                    return BadRequest("Could not create assignment set.");
                }
                return CreatedAtAction(nameof(CreateAssignmentSet), new { id = assignmentSet.Id }, assignmentSet);// instead of Ok
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  $"An error occurred while creating the assignment set: {ex.Message} ");
            }
        }

        [HttpGet("teacher/{teacherId:int}")]
        public async Task<ActionResult<IEnumerable<AssignmentSet>>> GetAllAssignmentSet(int teacherId)
        {
            try
            {
                var assignmentSet = await _assignmentSetService.GetAllAssignmentSetsByTeacherAsync(teacherId);
                if (assignmentSet == null || !assignmentSet.Any())
                {
                    return NotFound($"No assignment set found for Teacher ID {teacherId}");
                }
                return Ok(assignmentSet);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                    $"Error retrieving data: {ex.Message}");
            }
        }
    }
}
