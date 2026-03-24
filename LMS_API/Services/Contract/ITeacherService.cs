using LMS_API.Models;
using LMS_API.Models.DTO;

namespace LMS_API.Services.Contract
{
    public interface ITeacherService
    {
        Task<Teacher> RegisterTeacherAsync(TeacherCreateDTO teacherDTO);
        Task<bool> LoginAsync(TeacherLoginDTO loginDTO);
    }
}
