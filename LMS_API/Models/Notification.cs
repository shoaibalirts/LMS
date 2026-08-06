using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_API.Models
{
	public class Notification
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int RecipientUserId { get; set; }

		[Required]
		[MaxLength(20)]
		public string RecipientRole { get; set; } = string.Empty; // "Teacher" or "Student"

		[Required]
		[MaxLength(2000)]
		public string Message { get; set; } = string.Empty;

		public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

		public int? AssignedAssignmentSetId { get; set; }
		public int? AssignedAssignmentId { get; set; }
	}
}
