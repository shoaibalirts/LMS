using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.AssignmentSet;
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

        public async Task<AssignmentSet> CreateAssignmentSetAsync(AssignmentSetCreateDTO assignmentSetDTO, int teacherId)
        {
            try
            {
                if (assignmentSetDTO == null) return null;

                AssignmentSet assignmentSet = _mapper.Map<AssignmentSet>(assignmentSetDTO);
                assignmentSet.TeacherId = teacherId;
                assignmentSet.CreatedDate = DateTime.Now;

                await _db.AssignmentSets.AddAsync(assignmentSet);
                await _db.SaveChangesAsync();
                return assignmentSet;
            }
            catch (Exception)
            {
                return null;
            }
        }

       public async Task<IEnumerable<AssignmentSetReadDTO>> GetAllAssignmentSetsByTeacherAsync(int teacherId)
        {
            try
            {
                var sets = await _db.AssignmentSets
                    .Include(x => x.AssignmentAssignmentSets)
                        .ThenInclude(link => link.Assignment)
                    .Where(x => x.TeacherId == teacherId)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<AssignmentSetReadDTO>>(sets);
            }
            catch (Exception)
            {
                return null;
            }
        }       
        public async Task<bool> AddAssignmentToSetAsync(int assignmentSetId, int assignmentId, int teacherId)
        {
            try
            {
                var assignmentSet = await _db.AssignmentSets
                    .FirstOrDefaultAsync(x => x.Id == assignmentSetId && x.TeacherId == teacherId);
                if (assignmentSet == null) return false;

                var assignment = await _db.Assignments
                    .FirstOrDefaultAsync(x => x.Id == assignmentId && x.TeacherId == teacherId);
                if (assignment == null) return false;

                var alreadyLinked = await _db.AssignmentAssignmentSets
                    .AnyAsync(x => x.AssignmentSetId == assignmentSetId && x.AssignmentId == assignmentId);
                if (alreadyLinked) return true;

                await _db.AssignmentAssignmentSets.AddAsync(new AssignmentAssignmentSet
                {
                    AssignmentSetId = assignmentSetId,
                    AssignmentId = assignmentId
                });

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}