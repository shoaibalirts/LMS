using LMS_API.Models.DTO.Student;

namespace LMS_API.Models.DTO.StudyClass
{
    public class StudyClassReadDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<StudentReadDTO> Students { get; set; } = new();
    }
}
