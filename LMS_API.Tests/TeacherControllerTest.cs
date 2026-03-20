using Moq;
using LMS_API.Controllers;
using LMS_API.Models;
using LMS_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class TeacherControllerTest
{
    // Test successful creation of a teacher
    [Fact]
    public async Task CreateTeacher_ReturnsOk1()
    {
        // Arrange
            // Use dependency inversion: controller depends on the ITeacherRepository abstraction, not a concrete DbContext.
        var mockRepo = new Mock<ITeacherRepository>();

            // Setup mock behavior for AddTeacherAsync
        mockRepo.Setup(r => r.AddTeacherAsync(It.IsAny<Teacher>()))
                .ReturnsAsync((Teacher t) => t);

            // Inject mock into controller (supports testing in isolation)
        var controller = new TeacherController(mockRepo.Object);

        var teacher = new Teacher { FirstName = "John", LastName = "Doe", Email = "john@ucl.dk", Password = "pass123" };

        // Act
        var result = await controller.CreateTeacher(teacher);

        // Assert 
            // (SRP: test only one behavior)
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTeacher = Assert.IsType<Teacher>(okResult.Value);
        Assert.Equal("John", returnedTeacher.FirstName);

            // Liskov the Mocked object can replace any real ITeacherRepository implementations without changing controller behavior
    }

    // Test model validation 
    [Theory]
    [InlineData("John", "Doe", "", "pass123")] // Missing email
    [InlineData("John", "Doe", "john@ucl.dk", "")] // Missing password
    [InlineData("", "Doe", "john@ucl.dk", "pass123")] // Missing first name
    [InlineData("John", "", "john@ucl.dk", "pass123")] // Missing last name
    [InlineData("", "", "", "")] // All fields missing
    public async Task CreateTeacherValidation_ReturnsBadRequest(string firstName, string lastName, string email, string password)
    {
        // Arrange
        var mockRepo = new Mock<ITeacherRepository>();
        var controller = new TeacherController(mockRepo.Object);

        var teacher = new Teacher { FirstName = firstName, LastName = lastName, Email = email, Password = password };

        // Manually trigger model validation
            // Nessary because our model validation doesnt trigger, if its not send by a HTTP Request.
        controller.ModelState.Clear(); // ensure it's empty
        var validationContext = new ValidationContext(teacher, null, null);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(teacher, validationContext, validationResults, true);

        foreach (var validationResult in validationResults)
        {
            controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage!);
        }
            // End of model validation trigger

        // Act
        var result = await controller.CreateTeacher(teacher);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}