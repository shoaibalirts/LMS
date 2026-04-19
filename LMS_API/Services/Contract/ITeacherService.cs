using LMS_API.Models;
using LMS_API.Models.DTO.Teacher;

namespace LMS_API.Services.Contract
{
    public interface ITeacherService
    {
        Task<Teacher?> RegisterTeacherAsync(TeacherCreateDTO teacherDTO);
        Task<Teacher?> AuthenticateAsync(TeacherLoginDTO loginDTO);
    }
}
