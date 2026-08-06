using LMS_API.Models.DTO.AssignedAssignment;

namespace LMS_API.Models.DTO.AssignedAssignmentSet
{
	public class AssignedAssignmentSetReadDTO
	{
		public int Id { get; set; }
		public int TeacherId { get; set; }
		public int StudentId { get; set; }
		public int? AssignmentSetId { get; set; }
		public string? AssignmentSetName { get; set; }
		public string? TaskDocumentFileName { get; set; }
		public DateOnly DateOfAssigned { get; set; }
		public DateOnly Deadline { get; set; }
		public List<AssignedAssignmentReadDTO> AssignedAssignments { get; set; } = new();
	}
}
