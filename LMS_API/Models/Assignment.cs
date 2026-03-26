using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_API.Models
{
    public class Assignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Range(0,1000)]
        public decimal Points { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // 'Quiz', 'Homework'

        [Required]
        [MaxLength(20)]
        public string ClassLevel { get; set; } // 'Grade 10'

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }
        
        public DateTime? CreatedDate { get; set; } // optional. it can be nullable
        public DateTime? UpdatedDate { get; set; } // optional. it can be nullable

        public int? AssignmentSetId { get; set; } // Foreign Key
        public AssignmentSet AssignmentSet { get; set; } // Navigation back to parent
    }
}
