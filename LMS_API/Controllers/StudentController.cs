using LMS_API.Models;
using LMS_API.Models.DTO.Auth;
using LMS_API.Models.DTO.Student;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentController:ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, ITokenService tokenService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<ActionResult<StudentReadDTO>> CreateStudent(StudentCreateDTO studentDTO)
        {
            try
            {
                if (studentDTO == null)
                {
                    return BadRequest("Student data is required");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!_tokenService.TryGetTeacherId(User, out var teacherId))
                {
                    return Unauthorized("Missing or invalid teacher identity.");
                }

                var student = await _studentService.RegisterStudentAsync(studentDTO, teacherId);
                if (student == null)
                {
                    _logger.LogWarning("Student registration conflict email={Email}", studentDTO.Email);
                    return Conflict($"'{studentDTO.Email}' already exists.");
                }

                _logger.LogInformation("Student registered email={Email}", student.Email);

                var studentReadDTO = new StudentReadDTO
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email
                };

                return CreatedAtAction(nameof(CreateStudent), new { id = student.Id }, studentReadDTO);

            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                var details = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while saving the student: {details}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   $"An error occurred while creating the student: {ex.Message} ");
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("teacher")]
        public async Task<ActionResult<IEnumerable<StudentReadDTO>>> GetStudentsCreatedByTeacher()
        {
            if (!_tokenService.TryGetTeacherId(User, out var teacherId))
            {
                return Unauthorized("Missing or invalid teacher identity.");
            }

            var students = await _studentService.GetStudentsCreatedByTeacherAsync(teacherId);
            return Ok(students);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDTO>> LoginStudent(StudentLoginDTO studentLoginDTO)
        {
            try
            {
                var student = await _studentService.AuthenticateAsync(studentLoginDTO);
                if (student == null)
                {
                    _logger.LogWarning("Failed student login attempt email={Email}", studentLoginDTO.Email);
                    return Unauthorized("Invalid email or password.");
                }

                _logger.LogInformation("Student logged in email={Email}", student.Email);

                var token = _tokenService.GenerateToken(student.Id, student.Email, "Student");
                return Ok(new AuthResponseDTO
                {
                    Token = token,
                    Role = "Student",
                    Email = student.Email,
                    ExpiresAtUtc = _tokenService.GetTokenExpiryUtc()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while login: {ex.Message} ");
            }

        }
    }
}
