using LMS_API.Models;
using LMS_API.Models.DTO.Student;
using LMS_API.Services;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentController:ControllerBase
    {
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(StudentCreateDTO studentDTO)
        {
            try
            {
                if (studentDTO == null)
                {
                    return BadRequest("Student data is required");
                }
                var student = await _studentService.RegisterStudentAsync(studentDTO);
                if (student == null)
                {
                    return Conflict($"'{studentDTO.Email}' already exists.");
                }
                return CreatedAtAction(nameof(CreateStudent), new { id = student.Id }, student);// instead of Ok

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"An error occurred while creating the student: {ex.Message} ");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<bool>> LoginStudent(StudentLoginDTO studentLoginDTO)
        {
            try
            {
                var isSuccess = await _studentService.LoginAsync(studentLoginDTO);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while login: {ex.Message} ");
            }

        }
    }
}
