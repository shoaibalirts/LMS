using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_API.Models
{
    public class AssignmentSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        // Foreign key
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        public DateTime? CreatedDate { get; set; } // optional. it can be nullable
        public DateTime? UpdatedDate { get; set; } // optional. it can be nullable

        public ICollection<Assignment> Assignments { get; set; }

    }
}
