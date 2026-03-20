using LMS_API.Data;
using LMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly ApplicationDbContext _db;

        public TeacherRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Teacher> AddTeacherAsync(Teacher teacher)
        {
            await _db.Teacher.AddAsync(teacher);
            await _db.SaveChangesAsync();
            return teacher;
        }

        public async Task<Teacher?> GetTeacherByIdAsync(int id)
        {
            return await _db.Teacher.FindAsync(id);
        }

        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
        {
            return await _db.Teacher.ToListAsync();
        }
    }
}