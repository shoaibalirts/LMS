using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public TeacherController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        [HttpPost]
        public async Task<ActionResult<Teacher>> CreateTeacher(TeacherCreateDTO teacherDTO)
        {
            try
            {
                if (teacherDTO == null)
                {
                    return BadRequest("Teacher data is required");
                }
                /* mapping properties

                Teacher teacher = new Teacher()
                {
                    FirstName = teacherDTO.FirstName,
                    LastName = teacherDTO.LastName,
                    Email = teacherDTO.Email,
                    Password = teacherDTO.Password,
                    CreatedDate = DateTime.Now


                };
                */

                var duplicateEmail = await _db.Teacher.FirstOrDefaultAsync(u => u.Email.ToLower() == teacherDTO.Email.ToLower());
                if (duplicateEmail != null)
                {
                    return Conflict($"'{teacherDTO.Email}' already exists.");
                }
                Teacher teacher = _mapper.Map<Teacher>(teacherDTO);
                await _db.Teacher.AddAsync(teacher); // Teacher is a table name in SQL, and teacherDTO is an object which has some specific properties allowed to the frontend to stored in Teacher table .
                await _db.SaveChangesAsync();
                //return Ok(teacherDTO);
                return CreatedAtAction(nameof(CreateTeacher), new { id = teacher.Id }, teacher);// instead of Ok
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while creating the teacher: {ex.Message} ");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<bool>> LoginTeacher(TeacherLoginDTO teacherLoginDTO)
        {

            try
            {

                var teacher = await _db.Teacher.FirstOrDefaultAsync(u => u.Email.ToLower() == teacherLoginDTO.Email.ToLower() && u.Password == teacherLoginDTO.Password);

                if (teacher != null)
                {
                    return true; 
                }
                else
                {
                    return false; 
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while login: {ex.Message} ");
            }
        }
    }
}
