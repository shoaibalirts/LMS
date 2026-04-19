using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO.StudyClass
{
    public class StudyClassCreateDTO
    {
        [Required]
        public required string Name { get; set; }
    }
}
