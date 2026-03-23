using Moq;
using LMS_API.Models;
using LMS_API.Repositories;
using Xunit;

public class TaskRepositoryTest
{
    // Repository not implemented, therefor following TDD red/green/refactor
    [Theory]
    [ClassData(typeof(TaskRepositoryData))] 
    public async System.Threading.Tasks.Task AddTaskAsync_ReturnsOk(LMS_API.Models.Task taskToCreate)
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.AddTaskAsync(It.IsAny<LMS_API.Models.Task>()))
                .ReturnsAsync((LMS_API.Models.Task t) => t);

        // Act
        var result = await mockRepo.Object.AddTaskAsync(taskToCreate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskToCreate.Points, result.Points);
        Assert.Equal(taskToCreate.TaskSubject, result.TaskSubject);
        Assert.Equal(taskToCreate.PictureUrl, result.PictureUrl);

        mockRepo.Verify(r => r.AddTaskAsync(It.Is<LMS_API.Models.Task>(t =>
            t.Id == taskToCreate.Id &&
            t.Points == taskToCreate.Points &&
            t.TaskType == taskToCreate.TaskType &&
            t.ClassLevel == taskToCreate.ClassLevel &&
            t.TaskSubject == taskToCreate.TaskSubject &&
            t.PictureUrl == taskToCreate.PictureUrl &&
            t.PictureArray == taskToCreate.PictureArray
        )), Times.Once);
    }
}