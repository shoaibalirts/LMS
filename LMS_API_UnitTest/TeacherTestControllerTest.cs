using AutoMapper;
using LMS_API.Controllers;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LMS_API_UnitTest
{
    public class TeacherTestControllerTest
    {
        [Fact]
        public async Task CreateTeacher_ValidData_ReturnsCreatedAtAction()
        {
            // --- ARRANGE ---
            // 1. Setup the "Fake" DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            // 2. Setup the Mock Mapper
            var mockMapper = new Mock<IMapper>();
            var teacherDto = new TeacherCreateDTO
            {
                FirstName = "abc",
                LastName = "def",
                Email = "abc.def@gmail.com",
                Password = "123456"
            };

            var teacherEntity = new Teacher
            {
                Id = 10,
                FirstName = "abc",
                LastName = "def",
                Email = "abc.def@gmail.com",
                Password = "123456"
            };

            // Tell Moq: "When Map is called with ANY TeacherCreateDTO, return our teacherEntity"
            mockMapper.Setup(m => m.Map<Teacher>(It.IsAny<TeacherCreateDTO>()))
                       .Returns(teacherEntity);

            var controller = new TeacherController(context, mockMapper.Object);

            // --- ACT ---
            var result = await controller.CreateTeacher(teacherDto);

            // --- ASSERT ---
            // Check if the result is a "CreatedAtAction" (201 Created)
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);

            // Check if the teacher was actually added to the database
            var teacherInDb = await context.Teacher.FirstOrDefaultAsync(t => t.Email == "abc.def@gmail.com");
            Assert.NotNull(teacherInDb);
            Assert.Equal("abc", teacherInDb.FirstName);
            Assert.Equal("def", teacherInDb.LastName);
        }
    }
}
