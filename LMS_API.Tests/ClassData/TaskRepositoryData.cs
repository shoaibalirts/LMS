using System.Collections;
using LMS_API.Models.Enums;

public class TaskRepositoryData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new LMS_API.Models.Task
            {
                Id = 1,
                Points = 5,
                TaskType = TaskType.Delprøve,
                ClassLevel = ClassLevel.A,
                TaskSubject = "Linear Algebra",
                PictureUrl = "https://example.com/linear-algebra.png",
                PictureArray = null 
            }
        };

        yield return new object[]
        {
            new LMS_API.Models.Task
            {
                Id = 2,
                Points = 10,
                TaskType = TaskType.Delprøve,
                ClassLevel = ClassLevel.B,
                TaskSubject = "Calculus",
                PictureUrl = "https://example.com/calculus.png",
                PictureArray = null 
            }
        };
        yield return new object[]
        {
            new LMS_API.Models.Task
            {
                Id = 3,
                Points = 10,
                TaskType = TaskType.Delprøve,
                ClassLevel = ClassLevel.B,
                TaskSubject = "Calculus",
                PictureUrl = "https://example.com/calculus.jpg",
                PictureArray = null 
            }
        };
        yield return new object[]
        {
            new LMS_API.Models.Task
            {
                Id = 4,
                Points = 10,
                TaskType = TaskType.Delprøve,
                ClassLevel = ClassLevel.B,
                TaskSubject = "Calculus",
                PictureUrl = null,
                PictureArray = new byte[] { 0xFF, 0xD8, 0xFF } // Simulated Pictured
            }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}