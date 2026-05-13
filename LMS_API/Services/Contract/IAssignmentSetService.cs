using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Models.DTO.AssignmentSet;

namespace LMS_API.Services.Contract
{
    public interface IAssignmentSetService
    {
        Task<AssignmentSetReadDTO?> CreateAssignmentSetAsync(AssignmentSetCreateDTO assignmentSetDTO, int teacherId);
        Task<IEnumerable<AssignmentSetReadDTO>> GetAllAssignmentSetsByTeacherAsync(int teacherId);
        Task<bool> AddAssignmentToSetAsync(int assignmentSetId, int assignmentId, int teacherId);
		Task<bool> DeleteAssignmentSetAsync(int assignmentSetId, int teacherId);
    }
}
