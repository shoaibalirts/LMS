using LMS_API.Controllers;
using LMS_API.Models;
using LMS_API.Models.DTO.Auth;
using LMS_API.Models.DTO.Student;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections;
using System.Security.Claims;

namespace LMS_API_UnitTest
{
    // ClassData: invalid login credentials supplied to the login tests
    public class InvalidLoginData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return [new StudentLoginDTO { Email = "notfound@test.com", Password = "secret123" }];
            yield return [new StudentLoginDTO { Email = "jane@test.com", Password = "wrongpass" }];
            yield return [new StudentLoginDTO { Email = "jane@test.com", Password = "      " }];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class StudentControllerTest
    {
        private readonly Mock<IStudentService> _studentServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly StudentController _controller;

        public StudentControllerTest()
        {
            _studentServiceMock = new Mock<IStudentService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _controller = new StudentController(_studentServiceMock.Object, _tokenServiceMock.Object);
        }

        // MemberData: different valid student inputs to test CreateStudent happy path
        public static IEnumerable<object[]> ValidStudentDtos =>
        [
            [new StudentCreateDTO { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", Password = "secret123" }],
            [new StudentCreateDTO { FirstName = "John", LastName = "Smith", Email = "john@test.com", Password = "pass1234" }],
            [new StudentCreateDTO { FirstName = "Anna", LastName = "Berg", Email = "anna@test.com", Password = "hunter22" }],
        ];

        private void SetAuthenticatedTeacher(int teacherId)
        {
            _tokenServiceMock
                .Setup(t => t.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId))
                .Returns(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("teacherId", teacherId.ToString())
                    }, "mock"))
                }
            };
        }

        private void SetUnauthenticated()
        {
            var dummyId = 0;
            _tokenServiceMock
                .Setup(t => t.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out dummyId))
                .Returns(false);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };
        }

        // --- CreateStudent ---

        // MOCK: Verify confirms the controller extracted teacherId=42 from the token
        // and passed it through to the service. The HTTP result alone does not prove
        // the correct teacherId was used.
        [Theory]
        [MemberData(nameof(ValidStudentDtos))]
        public async Task CreateStudent_ValidData_ReturnsCreatedAtAction(StudentCreateDTO dto)
        {
            // Arrange
            var student = new Student
            {
                Id = 1,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = "hashed"
            };

            SetAuthenticatedTeacher(42);
            _studentServiceMock
                .Setup(s => s.RegisterStudentAsync(dto, 42))
                .ReturnsAsync(student);

            // Act
            var result = await _controller.CreateStudent(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<StudentReadDTO>(created.Value);
            Assert.Equal(dto.Email, returned.Email);
            _studentServiceMock.Verify(s => s.RegisterStudentAsync(dto, 42), Times.Once);
        }

        // STUB: the Conflict result already proves the service was reached and returned null.
        // Verifying the call was made adds no extra information.
        [Fact]
        public async Task CreateStudent_DuplicateEmail_ReturnsConflict()
        {
            // Arrange
            var dto = new StudentCreateDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com",
                Password = "secret123"
            };

            SetAuthenticatedTeacher(42);
            _studentServiceMock
                .Setup(s => s.RegisterStudentAsync(dto, 42))
                .ReturnsAsync((Student?)null);

            // Act
            var result = await _controller.CreateStudent(dto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        // MOCK: Verify(Times.Never) proves the controller short-circuits on a bad token
        // and never reaches the service. Without this, the test only proves the response
        // shape — not that the service was correctly guarded.
        [Fact]
        public async Task CreateStudent_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new StudentCreateDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com",
                Password = "secret123"
            };

            SetUnauthenticated();

            // Act
            var result = await _controller.CreateStudent(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            _studentServiceMock.Verify(s => s.RegisterStudentAsync(It.IsAny<StudentCreateDTO>(), It.IsAny<int>()), Times.Never);
        }

        // STUB: the 500 result proves the exception was caught and handled.
        // The service throwing is the cause — no need to verify the call separately.
        [Fact]
        public async Task CreateStudent_ServiceThrows_Returns500()
        {
            // Arrange
            var dto = new StudentCreateDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com",
                Password = "secret123"
            };

            SetAuthenticatedTeacher(42);
            _studentServiceMock
                .Setup(s => s.RegisterStudentAsync(dto, 42))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.CreateStudent(dto);

            // Assert
            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, status.StatusCode);
        }

        // --- GetStudentsCreatedByTeacher ---

        // MOCK: Verify confirms the correct teacherId was extracted from the token
        // and forwarded to the service. The OK result only proves data came back.
        [Fact]
        public async Task GetStudentsCreatedByTeacher_ValidToken_ReturnsOkWithStudents()
        {
            // Arrange
            var students = new List<StudentReadDTO>
            {
                new() { Id = 1, FirstName = "Jane", LastName = "Doe", Email = "jane@test.com" }
            };

            SetAuthenticatedTeacher(42);
            _studentServiceMock
                .Setup(s => s.GetStudentsCreatedByTeacherAsync(42))
                .ReturnsAsync(students);

            // Act
            var result = await _controller.GetStudentsCreatedByTeacher();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<StudentReadDTO>>(ok.Value);
            Assert.Single(returned);
            _studentServiceMock.Verify(s => s.GetStudentsCreatedByTeacherAsync(42), Times.Once);
        }

        // MOCK: Verify(Times.Never) proves the controller never calls the service
        // when the token is invalid — same guard pattern as CreateStudent_InvalidToken.
        [Fact]
        public async Task GetStudentsCreatedByTeacher_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            SetUnauthenticated();

            // Act
            var result = await _controller.GetStudentsCreatedByTeacher();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
            _studentServiceMock.Verify(s => s.GetStudentsCreatedByTeacherAsync(It.IsAny<int>()), Times.Never);
        }

        // --- LoginStudent ---

        // MOCK: Verify confirms GenerateToken was called with the authenticated student's
        // data. The token string in the response alone doesn't prove the right arguments
        // were used — the mock could return "jwt-token" regardless.
        [Fact]
        public async Task LoginStudent_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new StudentLoginDTO { Email = "jane@test.com", Password = "secret123" };
            var student = new Student
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Doe",
                Email = dto.Email,
                Password = "hashed"
            };
            var expiry = DateTime.UtcNow.AddHours(1);

            _studentServiceMock.Setup(s => s.AuthenticateAsync(dto)).ReturnsAsync(student);
            _tokenServiceMock.Setup(t => t.GenerateToken(student.Id, student.Email, "Student")).Returns("jwt-token");
            _tokenServiceMock.Setup(t => t.GetTokenExpiryUtc()).Returns(expiry);

            // Act
            var result = await _controller.LoginStudent(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDTO>(ok.Value);
            Assert.Equal("jwt-token", response.Token);
            Assert.Equal("Student", response.Role);
            Assert.Equal(student.Email, response.Email);
            _tokenServiceMock.Verify(t => t.GenerateToken(student.Id, student.Email, "Student"), Times.Once);
        }

        // STUB: the Unauthorized result proves AuthenticateAsync returned null for each
        // input. Verifying the call itself adds no extra information here.
        [Theory]
        [ClassData(typeof(InvalidLoginData))]
        public async Task LoginStudent_InvalidCredentials_ReturnsUnauthorized(StudentLoginDTO dto)
        {
            // Arrange
            _studentServiceMock.Setup(s => s.AuthenticateAsync(dto)).ReturnsAsync((Student?)null);

            // Act
            var result = await _controller.LoginStudent(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        // STUB: the 500 result proves the thrown exception was caught.
        // The service throwing is itself the trigger — no call verification needed.
        [Fact]
        public async Task LoginStudent_ServiceThrows_Returns500()
        {
            // Arrange
            var dto = new StudentLoginDTO { Email = "jane@test.com", Password = "secret123" };

            _studentServiceMock.Setup(s => s.AuthenticateAsync(dto)).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.LoginStudent(dto);

            // Assert
            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, status.StatusCode);
        }
    }
}
