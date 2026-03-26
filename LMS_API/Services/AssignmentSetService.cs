using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Models.DTO.Assignmentset;
using LMS_API.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class AssignmentSetService : IAssignmentSetService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public AssignmentSetService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<AssignmentSet> CreateAssignmentSetAsync(AssignmentSetCreateDTO assignmentSetDTO)
        {
            try
            {
                if (assignmentSetDTO == null)
                {
                    return null;
                }

                AssignmentSet assignmentSet = _mapper.Map<AssignmentSet>(assignmentSetDTO);
                assignmentSet.CreatedDate = DateTime.Now;
                await _db.AssignmentSets.AddAsync(assignmentSet);
                await _db.SaveChangesAsync();
                return assignmentSet;
            }
            catch (Exception ex)
            {
                // Optionally log the exception or handle as needed
                return null;
            }
        }

        /////////////////////////////
        public async Task<IEnumerable<AssignmentSet>> GetAllAssignmentSetsByTeacherAsync(int teacherId)
        {
            try
            {
                return await _db.AssignmentSets
                    .Include(x=>x.Assignments)
                    .Where(x=>x.TeacherId==teacherId)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return Enumerable.Empty<AssignmentSet>();
            }
        }


    }
}
