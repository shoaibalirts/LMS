using System.Collections;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using LMS_API.Controllers;
using LMS_API.Models;
using LMS_API.Models.DTO.Auth;
using LMS_API.Models.DTO.Teacher;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LMS_API_UnitTest
{
    public class TeacherControllerTests
    {
        private readonly Mock<ITeacherService> _teacherServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<ILogger<TeacherController>> _logger = new();
        private readonly TeacherController _controller;

        public TeacherControllerTests()
        {
            _controller = new TeacherController(_teacherServiceMock.Object, _tokenServiceMock.Object, _logger.Object);
        }

        // DDT: different valid teacher inputs all expect 201 Created
        public static IEnumerable<object[]> ValidCreateTeacherData =>
        [
            [new TeacherCreateDTO { FirstName = "Alice", LastName = "Smith",  Email = "alice@school.dk", Password = "pass123" }],
            [new TeacherCreateDTO { FirstName = "Bob",   LastName = "Hansen", Email = "bob@school.dk",   Password = "secret99" }],
            [new TeacherCreateDTO { FirstName = "Sara",  LastName = "Lund",   Email = "sara@school.dk",  Password = "abcdef" }],
        ];

        [Theory]
        [MemberData(nameof(ValidCreateTeacherData))]
        public async Task CreateTeacher_ValidData_ReturnsCreated(TeacherCreateDTO dto)
        {
            var teacher = new Teacher { Id = 1, FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email, Password = dto.Password };

            _teacherServiceMock.Setup(s => s.RegisterTeacherAsync(dto)).ReturnsAsync(teacher);

            var result = await _controller.CreateTeacher(dto);

            var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var body = created.Value.Should().BeOfType<TeacherReadDTO>().Subject;

            body.Email.Should().Be(dto.Email);
            body.FirstName.Should().Be(dto.FirstName);
            body.LastName.Should().Be(dto.LastName);
        }

        [Fact]
        public async Task CreateTeacher_NullDTO_ReturnsBadRequest()
        {
            var result = await _controller.CreateTeacher(null!);

            result.Result.Should().BeOfType<BadRequestObjectResult>();

            // Null check must short-circuit before reaching the service - Verify that RegisterTeacherAsync is never called
            _teacherServiceMock.Verify(s => s.RegisterTeacherAsync(It.IsAny<TeacherCreateDTO>()), Times.Never);
        }

        [Theory]
        [InlineData("alice@school.dk")]
        [InlineData("existing@school.dk")]
        public async Task CreateTeacher_EmailAlreadyExists_ReturnsConflict(string email)
        {
            var dto = new TeacherCreateDTO { FirstName = "Alice", LastName = "Smith", Email = email, Password = "pass123" };

            _teacherServiceMock.Setup(s => s.RegisterTeacherAsync(dto)).ReturnsAsync((Teacher?)null);

            var result = await _controller.CreateTeacher(dto);

            result.Result.Should().BeOfType<ConflictObjectResult>();
        }

        [Theory]
        [ClassData(typeof(LoginTeacherData))]
        public async Task LoginTeacher_ValidCredentials_ReturnsOkWithToken(TeacherLoginDTO dto)
        {
            var teacher = new Teacher { Id = 1, FirstName = "Alice", LastName = "Smith", Email = dto.Email, Password = dto.Password };
            var fakeToken = "fake-jwt-token";
            var FakeExpiry = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            _teacherServiceMock.Setup(s => s.AuthenticateAsync(dto)).ReturnsAsync(teacher);
            _tokenServiceMock.Setup(s => s.GenerateToken(teacher.Id, teacher.Email, "Teacher")).Returns(fakeToken);
            _tokenServiceMock.Setup(s => s.GetTokenExpiryUtc()).Returns(FakeExpiry);

            var result = await _controller.LoginTeacher(dto);

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var body = ok.Value.Should().BeOfType<AuthResponseDTO>().Subject;

            body.Token.Should().Be(fakeToken);
            body.Role.Should().Be("Teacher");
            body.Email.Should().Be(dto.Email);
            body.ExpiresAtUtc.Should().Be(FakeExpiry);
        }

        [Theory]
        [ClassData(typeof(LoginTeacherData))]
        public async Task LoginTeacher_InvalidCredentials_ReturnsUnauthorized(TeacherLoginDTO dto)
        {
            _teacherServiceMock.Setup(s => s.AuthenticateAsync(dto)).ReturnsAsync((Teacher?)null);

            var result = await _controller.LoginTeacher(dto);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
            _tokenServiceMock.Verify(s => s.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }

    public class LoginTeacherData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return [new TeacherLoginDTO { Email = "alice@school.dk",  Password = "pass123"  }];
            yield return [new TeacherLoginDTO { Email = "bob@school.dk",    Password = "secret99" }];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}