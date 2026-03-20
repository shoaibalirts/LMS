using LMS_API.Models;
namespace LMS_API.Repositories
{
    public interface ITeacherRepository
    {
        Task<Teacher> AddTeacherAsync(Teacher teacher);
        Task<Teacher?> GetTeacherByIdAsync(int id);
        Task<IEnumerable<Teacher>> GetAllTeachersAsync();
    }
}