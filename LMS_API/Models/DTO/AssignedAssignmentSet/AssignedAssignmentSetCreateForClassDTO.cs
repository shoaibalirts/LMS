using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO.AssignedAssignmentSet
{
	public class AssignedAssignmentSetCreateForClassDTO
	{
		[Required]
		public int StudyClassId { get; set; }

		[Required]
		public int AssignmentSetId { get; set; }

		[Required]
		public DateOnly DateOfAssigned { get; set; }

		[Required]
		public DateOnly Deadline { get; set; }
	}
}
