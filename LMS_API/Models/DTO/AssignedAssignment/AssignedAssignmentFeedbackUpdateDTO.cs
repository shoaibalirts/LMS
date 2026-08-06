using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO.AssignedAssignment
{
	public class AssignedAssignmentFeedbackUpdateDTO
	{
		[MaxLength(2000)]
		public string? Feedback { get; set; }
	}
}
