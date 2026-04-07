using LMS_API.Models;
using LMS_API.Models.DTO.Student;


namespace LMS_API.Services.Contract
{
    public interface IStudentService
    {
        Task<Student> RegisterStudentAsync(StudentCreateDTO studentDTO);
        Task<bool> LoginAsync(StudentLoginDTO loginDTO);
    }
}
