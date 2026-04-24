using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace LMS_API.Models
{
    public class Assignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Range(0,10)]
        public decimal Points { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        [Required]
        [MaxLength(20)]
        public string ClassLevel { get; set; }

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }

        // Picture of the problem, client said he gets all the concrete assignments as pictures.
        [MaxLength(500)]
        public string? PictureUrl { get; set; } // set as string now needs to be updated in the future, once we know how to store the images or if they are online.

        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public int TeacherId { get; set; }
        [JsonIgnore]
        public Teacher Teacher { get; set; }

        [JsonIgnore]
        public ICollection<AssignedAssignment> AssignedAssignments { get; set; } = new List<AssignedAssignment>();

        // MANY-TO-MANY
        [JsonIgnore]
        public ICollection<AssignmentAssignmentSet> AssignmentAssignmentSets { get; set; } = new List<AssignmentAssignmentSet>();
    }
}