using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.Student;
using LMS_API.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class StudentService:IStudentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentService> _logger;

        public StudentService(ApplicationDbContext db, IMapper mapper, ILogger<StudentService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Student> RegisterStudentAsync(StudentCreateDTO studentDTO)
        {
            try
            {
                if (studentDTO == null)
                {
                    _logger.LogWarning("StudentCreateDTO is null");
                    return null;
                }
                var duplicateEmail = await _db.Students.AsNoTracking().AnyAsync(u => u.Email.ToLower() == studentDTO.Email.ToLower());
                if (duplicateEmail)
                {
                    _logger.LogWarning($"Email {studentDTO.Email} already exists");
                    return null;
                }
                Student student = _mapper.Map<Student>(studentDTO);
                student.CreatedDate = DateTime.Now;
                await _db.Students.AddAsync(student);
                await _db.SaveChangesAsync();
                _logger.LogInformation($"Student {student.Email} registered successfully");
                return student;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering student: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> LoginAsync(StudentLoginDTO loginDTO)
        {
            var student = await _db.Students.FirstOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower() && u.Password == loginDTO.Password);
            return student != null;
        }
    }
}
