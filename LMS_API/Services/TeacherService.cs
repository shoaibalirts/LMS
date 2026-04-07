using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.Teacher;
using LMS_API.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(ApplicationDbContext db, IMapper mapper, ILogger<TeacherService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Teacher> RegisterTeacherAsync(TeacherCreateDTO teacherDTO)
        {
            try
            {
                if (teacherDTO == null)
                {
                    _logger.LogWarning("TeacherCreateDTO is null");
                    return null;
                }
                var duplicateEmail = await _db.Teacher.AnyAsync(u => u.Email.ToLower() == teacherDTO.Email.ToLower());
                if (duplicateEmail)
                {
                    _logger.LogWarning($"Email {teacherDTO.Email} already exists");
                    return null;
                }
                Teacher teacher = _mapper.Map<Teacher>(teacherDTO);
                teacher.CreatedDate = DateTime.Now;
                await _db.Teacher.AddAsync(teacher);
                await _db.SaveChangesAsync();
                _logger.LogInformation($"Teacher {teacher.Email} registered successfully");
                return teacher;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering teacher: {ex.Message}");
                return null;
            }          
        }
        public async Task<bool> LoginAsync(TeacherLoginDTO loginDTO) 
        {
            var teacher = await _db.Teacher.FirstOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower() && u.Password == loginDTO.Password);
            return teacher != null;
        }
    }
}
