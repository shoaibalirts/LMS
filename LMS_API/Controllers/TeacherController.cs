using LMS_API.Data;
using LMS_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    public class TeacherController:ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public TeacherController(ApplicationDbContext db)
        {
            _db = db;            
        }
        

        [HttpPost]
        public async Task<ActionResult<Teacher>> CreateTeacher(Teacher teacher)
        {
            try
            {
                if (teacher == null)
                {
                    return BadRequest("Teacher data is required");
                }
                await _db.Teacher.AddAsync(teacher); // Teacher is a table name in SQL, and teacher is an object which has properties that will be stored in Teacher table .
                await _db.SaveChangesAsync();
                return Ok(teacher);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while creating the teacher: {ex.Message} ");
            }
        }
        
    }
}
