namespace LMS_API.Models.DTO.Notification
{
	public class NotificationReadDTO
	{
		public int Id { get; set; }
		public string Message { get; set; } = string.Empty;
		public DateTime CreatedAtUtc { get; set; }
		public int? AssignedAssignmentSetId { get; set; }
		public int? AssignedAssignmentId { get; set; }
	}
}
