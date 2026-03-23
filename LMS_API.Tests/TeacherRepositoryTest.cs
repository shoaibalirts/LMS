using Moq;
using LMS_API.Models;
using LMS_API.Repositories;


// Keep the unit small (Testing units (classes) in isolation) and focused (testing one behavior per test) to adhere to the Single Responsibility Principle (SRP).
public class TeacherRepositoryTest
{
    [Theory]
    [InlineData("John", "Doe", "john@ucl.dk", "pass123")]
    [InlineData("Jane", "Smith", "jane@ucl.dk", "securePass")]
    public async System.Threading.Tasks.Task AddTeacherAsync_ReturnsOk(
        string firstName,
        string lastName,
        string email,
        string password)
    {
        // Arrange
        var mockRepo = new Mock<ITeacherRepository>();

        var teacher = new Teacher
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password
        };

        mockRepo.Setup(r => r.AddTeacherAsync(It.IsAny<Teacher>()))
                .ReturnsAsync((Teacher t) => t);

        // Act
        var result = await mockRepo.Object.AddTeacherAsync(teacher);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(teacher.Email, result.Email);

        mockRepo.Verify(r => r.AddTeacherAsync(It.Is<Teacher>(t =>
            t.FirstName == firstName &&
            t.LastName == lastName &&
            t.Email == email &&
            t.Password == password
        )), Times.Once);
    }
}