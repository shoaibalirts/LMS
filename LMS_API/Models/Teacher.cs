using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string FirstName { get; set; }
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
