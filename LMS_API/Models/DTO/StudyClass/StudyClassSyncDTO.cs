

using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models.DTO.StudyClass
{
    public class StudyClassSyncDTO
    {
        [Required]
        public int? Id { get; set; } 
        public List<int> StudentIds { get; set; } = new();
    }
}
