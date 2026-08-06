using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO.Assignment
{
    public class AssignmentCreateDTO
    {
        [Required(ErrorMessage = "Points is required")]
        [Range(0, 10)]
        public int Points { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [MaxLength(50)]
        public required string Type { get; set; }

        [Required(ErrorMessage = "Class level is required")]
        [MaxLength(20)]
        public required string ClassLevel { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(100)]
        public required string Subject { get; set; }

        public IFormFile? PictureFile { get; set; }

        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        [MaxLength(2000)]
        public string? Result { get; set; }
    }
}
