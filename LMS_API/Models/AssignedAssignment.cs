using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LMS_API.Models
{
	public class AssignedAssignment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int AssignedAssignmentSetId { get; set; }

		[JsonIgnore]
		public AssignedAssignmentSet AssignedAssignmentSet { get; set; }

		[Required]
		public int AssignmentId { get; set; }

		public Assignment Assignment { get; set; }

		// File path for the uploaded PDF. Null until submitted by student.
		public string? StudentResultPath { get; set; }
		public string? StudentResultFileName { get; set; }
		public string? StudentResultContentType { get; set; }
		public DateTime? SubmittedAtUtc { get; set; }

		// Teacher feedback, null by default.
		public string? Feedback { get; set; }
	}
}
