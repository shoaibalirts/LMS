using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LMS_API.Models
{
	public class AssignedAssignmentSet
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public DateOnly DateOfAssigned { get; set; }

		[Required]
		public DateOnly Deadline { get; set; }

		[Required]
		public int TeacherId { get; set; }

		[JsonIgnore]
		public Teacher Teacher { get; set; }

		[Required]
		public int StudentId { get; set; }

		public Student Student { get; set; }

		public List<AssignedAssignment> AssignedAssignments { get; set; } = new();
	}
}
