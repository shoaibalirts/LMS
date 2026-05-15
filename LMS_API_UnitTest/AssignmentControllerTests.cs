using System.Collections;
using System.Security.Claims;
using FluentAssertions;
using LMS_API.Controllers;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace LMS_API_UnitTest
{
    public class AssignmentControllerTests
    {
        private readonly Mock<IAssignmentService> _assignmentServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<ILogger<AssignmentController>> _logger = new();
        private readonly AssignmentController _controller;

        public AssignmentControllerTests()
        {
            _controller = new AssignmentController(_assignmentServiceMock.Object, _tokenServiceMock.Object, _logger.Object);
        }

        public static IEnumerable<object[]> ValidCreateAssignmentData =>
        [
            [new AssignmentCreateDTO { Points = 5,  Type = "Quiz",  ClassLevel = "A", Subject = "Math"    }],
            [new AssignmentCreateDTO { Points = 10, Type = "Essay", ClassLevel = "B", Subject = "English" }],
            [new AssignmentCreateDTO { Points = 0,  Type = "Exam",  ClassLevel = "C", Subject = "Science" }],
        ];

        [Theory]
        [MemberData(nameof(ValidCreateAssignmentData))]
        public async Task CreateAssignment_ValidData_ReturnsCreated(AssignmentCreateDTO dto)
        {
            var teacherId = 1;
            var readDTO = new AssignmentReadDTO { Id = 1, Points = dto.Points, Type = dto.Type, ClassLevel = dto.ClassLevel, Subject = dto.Subject };

            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentServiceMock.Setup(s => s.CreateAssignmentAsync(dto, teacherId)).ReturnsAsync(readDTO);

            var result = await _controller.CreateAssignment(dto);

            var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var body = created.Value.Should().BeOfType<AssignmentReadDTO>().Subject;
            body.Type.Should().Be(dto.Type);
            body.Subject.Should().Be(dto.Subject);
            body.Points.Should().Be(dto.Points);
        }

        [Fact]
        public async Task CreateAssignment_NullDTO_ReturnsBadRequest()
        {
            var result = await _controller.CreateAssignment(null!);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
            _assignmentServiceMock.Verify(s => s.CreateAssignmentAsync(It.IsAny<AssignmentCreateDTO>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateAssignment_NoTeacherIdentity_ReturnsUnauthorized()
        {
            var dto = new AssignmentCreateDTO { Points = 5, Type = "Quiz", ClassLevel = "A", Subject = "Math" };
            var teacherId = 0;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(false);

            var result = await _controller.CreateAssignment(dto);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Theory]
        [ClassData(typeof(ValidGetAllAssignmentsData))]
        public async Task GetAllAssignments_ValidTeacher_ReturnsOk(List<AssignmentReadDTO> assignments)
        {
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentServiceMock.Setup(s => s.GetAllAssignmentsAsync(teacherId)).ReturnsAsync(assignments);

            var result = await _controller.GetAllAssignments();

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(assignments);
        }

        [Fact]
        public async Task GetAllAssignments_NoTeacherIdentity_ReturnsUnauthorized()
        {
            var teacherId = 0;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(false);

            var result = await _controller.GetAllAssignments();

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task DeleteAssignment_ValidId_ReturnsOk()
        {
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentServiceMock.Setup(s => s.DeleteAssignmentAsync(42, teacherId)).ReturnsAsync(true);

            var result = await _controller.DeleteAssignment(42);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory]
        [InlineData(99)]
        [InlineData(404)]
        [InlineData(1000)]
        public async Task DeleteAssignment_NotFound_ReturnsNotFound(int id)
        {
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentServiceMock.Setup(s => s.DeleteAssignmentAsync(id, teacherId)).ReturnsAsync(false);

            var result = await _controller.DeleteAssignment(id);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteAssignment_NoTeacherIdentity_ReturnsUnauthorized()
        {
            var teacherId = 0;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(false);

            var result = await _controller.DeleteAssignment(1);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }

    public class ValidGetAllAssignmentsData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return
            [
                new List<AssignmentReadDTO>
                {
                    new() { Id = 1, Points = 5,  Type = "Quiz",  ClassLevel = "A", Subject = "Math"    },
                    new() { Id = 2, Points = 10, Type = "Essay", ClassLevel = "B", Subject = "English" },
                }
            ];
            yield return
            [
                new List<AssignmentReadDTO>
                {
                    new() { Id = 3, Points = 0, Type = "Exam", ClassLevel = "C", Subject = "Science" },
                }
            ];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
