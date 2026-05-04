using System.Security.Claims;
using FluentAssertions;
using LMS_API.Controllers;
using LMS_API.Models;
using LMS_API.Models.DTO.Student;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Reqnroll;

namespace LMS_API_BDDTest.StepDefinitions;

[Binding]
public class CreateStudentSteps
{
    private readonly Mock<IStudentService> _studentService = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly StudentController _controller;
    private ActionResult<StudentReadDTO> _result = null!;

    public CreateStudentSteps()
    {
        _controller = new StudentController(_studentService.Object, _tokenService.Object);
    }

    [Given("a teacher is authenticated")]
    public void GivenATeacherIsAuthenticated()
    {
        int teacherId = 1;
        _tokenService
            .Setup(t => t.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId))
            .Returns(true);
    }

    [When("the teacher sends creates a student with valid student data")]
    public async Task WhenTeacherCreatesStudentWithValidData()
    {
        // mock the student service to return a student when RegisterStudentAsync is called with any StudentCreateDTO and teacherId 1
        _studentService
            .Setup(s => s.RegisterStudentAsync(It.IsAny<StudentCreateDTO>(), 1))
            .ReturnsAsync(new Student
            {
                FirstName = "Jan",
                LastName = "Doe",
                Email = "jan@school.dk",
                Password = "hashed"
            });

        // call the controller action to create a student with valid data and store it in _result to validate in the next steps
        _result = await _controller.CreateStudent(new StudentCreateDTO
        {
            FirstName = "Jan",
            LastName = "Doe",
            Email = "jan@school.dk",
            Password = "Pass123!"
        });
    }

    [Then("the response should be {int}")]
    public void ThenStatusCodeShouldBe(int expectedStatus)
    {
        _result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.StatusCode.Should().Be(expectedStatus);
    }


    [Then("the response should contain the student's email")]
    public void ThenResponseContainsEmail()
    {
        var created = _result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var dto = created.Value.Should().BeOfType<StudentReadDTO>().Subject;
        dto.Email.Should().Be("jan@school.dk");
    }
}
