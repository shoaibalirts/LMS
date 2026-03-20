using LMS_API.Models;
using LMS_API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherRepository _teacherRepo;

        public TeacherController(ITeacherRepository teacherRepo)
        {
            _teacherRepo = teacherRepo;
        }

        [HttpPost]
        public async Task<ActionResult<Teacher>> CreateTeacher(Teacher teacher)
        {
            if (teacher == null)
                return BadRequest("Teacher data is required");

            if (!ModelState.IsValid) // Checks if model validation is passed
                return BadRequest(ModelState);

            try
            {
                var createdTeacher = await _teacherRepo.AddTeacherAsync(teacher);
                return Ok(createdTeacher);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while creating the teacher: {ex.Message}");
            }
        }
    }
}