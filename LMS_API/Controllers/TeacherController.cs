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
    public class TeacherController:ControllerBase
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
                Teacher teacher = _mapper.Map<Teacher>(teacherDTO);
                await _db.Teacher.AddAsync(teacher); // Teacher is a table name in SQL, and teacherDTO is an object which has properties that will be stored in Teacher table .
                await _db.SaveChangesAsync();
                //return Ok(teacherDTO);
                return CreatedAtAction(nameof(CreateTeacher),new {id=teacher.Id},teacher);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while creating the teacher: {ex.Message} ");
            }
        }
        
    }
}
