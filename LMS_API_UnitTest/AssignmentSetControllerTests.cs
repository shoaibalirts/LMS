using System.Collections;
using System.Security.Claims;
using FluentAssertions;
using LMS_API.Controllers;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Models.DTO.AssignmentSet;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace LMS_API_UnitTest
{
    public class AssignmentSetControllerTests
    {
        private readonly Mock<IAssignmentSetService> _assignmentSetServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<ILogger<AssignmentSetController>> _logger = new();
        private readonly AssignmentSetController _controller;

        public AssignmentSetControllerTests()
        {
            _controller = new AssignmentSetController(_assignmentSetServiceMock.Object, _tokenServiceMock.Object, _logger.Object);
        }

        public static IEnumerable<object[]> ValidCreateAssignmentSetData =>
        [
            [new AssignmentSetCreateDTO { Name = "Math Set"    }],
            [new AssignmentSetCreateDTO { Name = "English Set" }],
            [new AssignmentSetCreateDTO { Name = "Science Set" }],
        ];

        [Theory]
        [MemberData(nameof(ValidCreateAssignmentSetData))]
        public async Task CreateAssignmentSet_ValidData_ReturnsCreated(AssignmentSetCreateDTO dto)
        {
            var teacherId = 1;
            var readDTO = new AssignmentSetReadDTO { Id = 1, Name = dto.Name };

            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentSetServiceMock.Setup(s => s.CreateAssignmentSetAsync(dto, teacherId)).ReturnsAsync(readDTO);

            var result = await _controller.CreateAssignmentSet(dto);

            var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var body = created.Value.Should().BeOfType<AssignmentSetReadDTO>().Subject;
            body.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task CreateAssignmentSet_NullDTO_ReturnsBadRequest()
        {
            var result = await _controller.CreateAssignmentSet(null!);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
            _assignmentSetServiceMock.Verify(s => s.CreateAssignmentSetAsync(It.IsAny<AssignmentSetCreateDTO>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateAssignmentSet_NoTeacherIdentity_ReturnsUnauthorized()
        {
            var dto = new AssignmentSetCreateDTO { Name = "Math Set" };
            var teacherId = 0;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(false);

            var result = await _controller.CreateAssignmentSet(dto);

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task CreateAssignmentSet_ServiceReturnsNull_ReturnsBadRequest()
        {
            var dto = new AssignmentSetCreateDTO { Name = "Math Set" };
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentSetServiceMock.Setup(s => s.CreateAssignmentSetAsync(dto, teacherId)).ReturnsAsync((AssignmentSetReadDTO?)null);

            var result = await _controller.CreateAssignmentSet(dto);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }


        [Theory]
        [ClassData(typeof(ValidGetAllAssignmentSetsData))]
        public async Task GetAllAssignmentSet_ValidTeacher_ReturnsOk(List<AssignmentSetReadDTO> sets)
        {
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentSetServiceMock.Setup(s => s.GetAllAssignmentSetsByTeacherAsync(teacherId)).ReturnsAsync(sets);

            var result = await _controller.GetAllAssignmentSet();

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(sets);
        }

        [Fact]
        public async Task GetAllAssignmentSet_NoTeacherIdentity_ReturnsUnauthorized()
        {
            var teacherId = 0;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(false);

            var result = await _controller.GetAllAssignmentSet();

            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        // --- AddAssignmentToSet ---

        [Fact]
        public async Task AddAssignmentToSet_ValidIds_ReturnsOk()
        {
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentSetServiceMock.Setup(s => s.AddAssignmentToSetAsync(1, 2, teacherId)).ReturnsAsync(true);

            var result = await _controller.AddAssignmentToSet(1, 2);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AddAssignmentToSet_ServiceReturnsFalse_ReturnsBadRequest()
        {
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentSetServiceMock.Setup(s => s.AddAssignmentToSetAsync(1, 2, teacherId)).ReturnsAsync(false);

            var result = await _controller.AddAssignmentToSet(1, 2);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddAssignmentToSet_NoTeacherIdentity_ReturnsUnauthorized()
        {
            var teacherId = 0;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(false);

            var result = await _controller.AddAssignmentToSet(1, 2);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }


        [Fact]
        public async Task DeleteAssignmentSet_ValidId_ReturnsOk()
        {
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentSetServiceMock.Setup(s => s.DeleteAssignmentSetAsync(1, teacherId)).ReturnsAsync(true);

            var result = await _controller.DeleteAssignmentSet(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Theory]
        [InlineData(99)]
        [InlineData(404)]
        [InlineData(1000)]
        public async Task DeleteAssignmentSet_NotFound_ReturnsNotFound(int id)
        {
            var teacherId = 1;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(true);
            _assignmentSetServiceMock.Setup(s => s.DeleteAssignmentSetAsync(id, teacherId)).ReturnsAsync(false);

            var result = await _controller.DeleteAssignmentSet(id);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteAssignmentSet_NoTeacherIdentity_ReturnsUnauthorized()
        {
            var teacherId = 0;
            _tokenServiceMock.Setup(s => s.TryGetTeacherId(It.IsAny<ClaimsPrincipal>(), out teacherId)).Returns(false);

            var result = await _controller.DeleteAssignmentSet(1);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }

    public class ValidGetAllAssignmentSetsData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return
            [
                new List<AssignmentSetReadDTO>
                {
                    new() { Id = 1, Name = "Math Set"    },
                    new() { Id = 2, Name = "English Set" },
                }
            ];
            yield return
            [
                new List<AssignmentSetReadDTO>
                {
                    new() { Id = 3, Name = "Science Set" },
                }
            ];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
