using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO
{
    public class TeacherCreateDTO
    {
        [MaxLength(50)]
        [Required]
        public required string FirstName { get; set; }
        [MaxLength(50)]
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }

        public DateTime? CreatedDate { get; set; } // optional. it can be nullable
        public DateTime? UpdatedDate { get; set; } // optional. it can be nullable

    }
}
