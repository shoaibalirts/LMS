using LMS_API.Models;
using LMS_API.Models.DTO.Student;


namespace LMS_API.Services.Contract
{
    public interface IStudentService
    {
        Task<Student?> RegisterStudentAsync(StudentCreateDTO studentDTO, int teacherId);
        Task<Student?> AuthenticateAsync(StudentLoginDTO loginDTO);
        Task<IEnumerable<StudentReadDTO>> GetStudentsCreatedByTeacherAsync(int teacherId);
    }
}
