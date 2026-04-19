using LMS_API.Models.DTO.Auth;
using LMS_API.Models.DTO.Teacher;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ITokenService _tokenService;

        public TeacherController(ITeacherService teacherService, ITokenService tokenService)
        {
            _teacherService = teacherService;
            _tokenService = tokenService;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<TeacherReadDTO>> CreateTeacher(TeacherCreateDTO teacherDTO)
        {
            try
            {
                if (teacherDTO == null)
                {
                    return BadRequest("Teacher data is required");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var teacher = await _teacherService.RegisterTeacherAsync(teacherDTO);
                if (teacher == null)
                {
                    return Conflict($"'{teacherDTO.Email}' already exists.");
                }

                var teacherReadDTO = new TeacherReadDTO
                {
                    Id = teacher.Id,
                    FirstName = teacher.FirstName,
                    LastName = teacher.LastName,
                    Email = teacher.Email
                };

                return CreatedAtAction(nameof(CreateTeacher), new { id = teacher.Id }, teacherReadDTO);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                $"An error occurred while creating the teacher: {ex.Message} ");
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDTO>> LoginTeacher(TeacherLoginDTO teacherLoginDTO)
        {
            try
            {
                var teacher = await _teacherService.AuthenticateAsync(teacherLoginDTO);
                if (teacher == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                var token = _tokenService.GenerateToken(teacher.Id, teacher.Email, "Teacher");
                return Ok(new AuthResponseDTO
                {
                    Token = token,
                    Role = "Teacher",
                    Email = teacher.Email,
                    ExpiresAtUtc = _tokenService.GetTokenExpiryUtc()
                });
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while login: {ex.Message} ");
            }
            
        }
    }
}
