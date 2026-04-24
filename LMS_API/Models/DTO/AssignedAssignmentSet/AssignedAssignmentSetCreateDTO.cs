using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO.AssignedAssignmentSet
{
	public class AssignedAssignmentSetCreateDTO
	{
		[Required]
		public int StudentId { get; set; }

		[Required]
		public DateOnly DateOfAssigned { get; set; }

		[Required]
		public DateOnly Deadline { get; set; }

		[Required]
		[MinLength(1)]
		public List<int> AssignmentIds { get; set; } = new();
	}
}
