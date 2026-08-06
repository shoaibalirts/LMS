using LMS_API.Models.DTO.AssignedAssignment;
using LMS_API.Models.DTO.AssignedAssignmentSet;
using Microsoft.AspNetCore.Http;

namespace LMS_API.Services.Contract
{
	public interface IAssignedAssignmentService
	{
		Task<AssignedAssignmentSetReadDTO?> CreateAssignedAssignmentSetAsync(AssignedAssignmentSetCreateDTO dto, int teacherId);
		Task<IEnumerable<AssignedAssignmentSetReadDTO>> CreateAssignedAssignmentSetForClassAsync(AssignedAssignmentSetCreateForClassDTO dto, int teacherId);
		Task<IEnumerable<AssignedAssignmentSetReadDTO>> GetAssignedSetsForStudentAsync(int studentId);
		Task<IEnumerable<AssignedAssignmentSetReadDTO>> GetAssignedSetsForTeacherAsync(int teacherId);
		Task<bool> RevokeAssignedAssignmentSetAsync(int assignedAssignmentSetId, int teacherId);
		Task<int> RevokeAllAssignedAssignmentSetsForStudentAsync(int studentId, int teacherId);
		Task<bool> DeleteAssignedAssignmentAsync(int assignedAssignmentId, int teacherId);
		Task<AssignedAssignmentReadDTO?> SubmitStudentResultAsync(int assignedAssignmentId, int studentId, IFormFile resultFile);
		Task<AssignedAssignmentReadDTO?> UpdateFeedbackAsync(int assignedAssignmentId, int teacherId, string? feedback);
		Task<(byte[] Content, string ContentType, string FileName)?> GetStudentResultFileAsync(int assignedAssignmentId, int requesterUserId, bool isTeacher);
		Task<(byte[] Content, string ContentType, string FileName)?> GetAssignmentSetDocumentFileAsync(int assignedAssignmentSetId, int requesterUserId, bool isTeacher);
	}
}
