using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Models.DTO.Assignmentset;

namespace LMS_API.Services.Contract
{
    public interface IAssignmentSetService
    {
        Task<AssignmentSet> CreateAssignmentSetAsync(AssignmentSetCreateDTO assignmentSetDTO);
        Task<IEnumerable<AssignmentSet>> GetAllAssignmentSetsByTeacherAsync(int TeacherId);
    }
}
