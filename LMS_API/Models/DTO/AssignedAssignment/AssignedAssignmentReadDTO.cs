namespace LMS_API.Models.DTO.AssignedAssignment
{
	public class AssignedAssignmentReadDTO
	{
		public int Id { get; set; }
		public int AssignmentId { get; set; }
		public string AssignmentSubject { get; set; } = string.Empty;
		public string AssignmentType { get; set; } = string.Empty;
		public string ClassLevel { get; set; } = string.Empty;
		public decimal AssignmentPoints { get; set; }

		public string? StudentResultPath { get; set; }
		public string? StudentResultFileName { get; set; }
		public DateTime? SubmittedAtUtc { get; set; }
		public string? Feedback { get; set; }

		public bool IsSubmitted => !string.IsNullOrWhiteSpace(StudentResultPath);
	}
}
