using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public TeacherService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Teacher> RegisterTeacherAsync(TeacherCreateDTO teacherDTO)
        {
            try
            {
                if (teacherDTO == null)
                {
                    return null;
                }
                var duplicateEmail = await _db.Teacher.AnyAsync(u => u.Email.ToLower() == teacherDTO.Email.ToLower());
                if (duplicateEmail)
                {
                    return null;
                }
                Teacher teacher = _mapper.Map<Teacher>(teacherDTO);
                teacher.CreatedDate = DateTime.Now;
                await _db.Teacher.AddAsync(teacher);
                await _db.SaveChangesAsync();
                return teacher;
            }
            catch (Exception ex)
            {
                // Optionally log the exception or handle as needed
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
