using System.ComponentModel.DataAnnotations;
namespace LMS_API.Models.DTO.AssignmentSet
{
    public class AssignmentSetCreateDTO
    {
        [Required(ErrorMessage = "Assignment set name is required")]
        public required string Name { get; set; }

    }
}
