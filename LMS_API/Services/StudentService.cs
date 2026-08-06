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
            if (studentDTO == null)
            {
                _logger.LogWarning("StudentCreateDTO is null");
                return null;
            }

            var normalizedEmail = studentDTO.Email.Trim().ToLowerInvariant();
            var duplicateEmail = await _db.Students
                .AsNoTracking()
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if (duplicateEmail)
            {
                _logger.LogWarning("Email {Email} already exists", studentDTO.Email);
                return null;
            }

            var teacherExists = await _db.Teacher
                .AsNoTracking()
                .AnyAsync(t => t.Id == teacherId);
            if (!teacherExists)
            {
                throw new InvalidOperationException("Teacher account for current session was not found. Please log in again.");
            }

            Student student = _mapper.Map<Student>(studentDTO);
            student.Password = _passwordHasher.Hash(studentDTO.Password);
            student.CreatedDate = DateTime.Now;
            student.CreatedByTeacherId = teacherId;
            student.Email = studentDTO.Email.Trim();

            await _db.Students.AddAsync(student);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Student {Email} registered successfully", student.Email);
            return student;
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
