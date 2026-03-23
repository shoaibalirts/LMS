namespace LMS_API.Models
{
    public class Task
    {
        public int Id { get; set; }
        public float Points { get; set; }
        public Enums.TaskType TaskType { get; set; }
        public Enums.ClassLevel ClassLevel { get; set; }
        public string TaskSubject { get; set; }
        public string PictureUrl { get; set; }
        public byte[] PictureArray { get; set; }
    }
}
