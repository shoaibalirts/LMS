using System.Text.Json.Serialization;

namespace LMS_API.Models
{
    public class StudentStudyClass
    {
        public int StudentId { get; set; }
        [JsonIgnore]
        public Student? Student { get; set; }

        public int StudyClassId { get; set; }
        [JsonIgnore]
        public StudyClass? StudyClass { get; set; }
    }
}