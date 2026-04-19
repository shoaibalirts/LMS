using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.Student;
using LMS_API.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentService> _logger;
        private readonly IPasswordHasher _passwordHasher;

        public StudentService(ApplicationDbContext db, IMapper mapper, ILogger<StudentService> logger, IPasswordHasher passwordHasher)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public async Task<Student?> RegisterStudentAsync(StudentCreateDTO studentDTO, int teacherId)
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
                student.Password = _passwordHasher.Hash(studentDTO.Password);
                student.CreatedDate = DateTime.Now;
                student.CreatedByTeacherId = teacherId;

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

        public async Task<IEnumerable<StudentReadDTO>> GetStudentsCreatedByTeacherAsync(int teacherId)
        {
            var students = await _db.Students
                .AsNoTracking()
                .Where(s => s.CreatedByTeacherId == teacherId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<StudentReadDTO>>(students);
        }

        public async Task<Student?> AuthenticateAsync(StudentLoginDTO loginDTO)
        {
            var student = await _db.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower());

            if (student == null) return null;
            return _passwordHasher.Verify(loginDTO.Password, student.Password) ? student : null;
        }
    }
}
