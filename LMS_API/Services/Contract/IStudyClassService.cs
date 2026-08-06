using LMS_API.Models;
using LMS_API.Models.DTO.StudyClass;

namespace LMS_API.Services.Contract
{
    public interface IStudyClassService
    {
        Task<StudyClassReadDTO> CreateStudyClassAsync(StudyClassCreateDTO studyClassDTO, int teacherId);
        Task<IEnumerable<StudyClassReadDTO>> GetStudyClassesByTeacherAsync(int teacherId);
        Task<StudyClassReadDTO?> GetStudyClassByIdAsync(int id, int teacherId);
        Task<bool> DeleteStudyClassAsync(int id, int teacherId);
        Task<StudyClassReadDTO?> AddStudentsToStudyClassAsync(StudyClassSyncDTO dto, int teacherId);
    }
}