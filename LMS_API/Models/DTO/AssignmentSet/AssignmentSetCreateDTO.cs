using System.ComponentModel.DataAnnotations;
namespace LMS_API.Models.DTO.Assignmentset
{
    public class AssignmentSetCreateDTO
    {
        [Required(ErrorMessage = "Assignment set name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Teacher Id is required")]
        public int TeacherId { get; set; }

    }
}
