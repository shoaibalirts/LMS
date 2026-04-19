using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.Assignment;
using LMS_API.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorage;

        public AssignmentService(ApplicationDbContext db, IMapper mapper, IFileStorageService fileStorage)
        {
            _db = db;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }

        public async Task<AssignmentReadDTO?> CreateAssignmentAsync(AssignmentCreateDTO assignmentDTO, int teacherId)
        {
            try
            {
                if (assignmentDTO == null) return null;

                string? pictureUrl = null;
                if (assignmentDTO.PictureFile != null)
                {
                    pictureUrl = await _fileStorage.SaveAsync(assignmentDTO.PictureFile);
                    if (pictureUrl == null)
                        return null;
                }

                Assignment assignment = _mapper.Map<Assignment>(assignmentDTO);
                assignment.PictureUrl = pictureUrl;
                assignment.CreatedDate = DateTime.Now;
                assignment.TeacherId = teacherId;

                await _db.Assignments.AddAsync(assignment);
                await _db.SaveChangesAsync();

                return _mapper.Map<AssignmentReadDTO>(assignment);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<AssignmentReadDTO>> GetAllAssignmentsAsync(int teacherId)
        {
            try
            {
                var assignments = await _db.Assignments
                    .Where(a => a.TeacherId == teacherId)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<AssignmentReadDTO>>(assignments);
            }
            catch (Exception)
            {
                return Enumerable.Empty<AssignmentReadDTO>();
            }
        }

        public async Task<bool> DeleteAssignmentAsync(int id, int teacherId)
        {
            try
            {
                var assignment = await _db.Assignments
                    .FirstOrDefaultAsync(a => a.Id == id && a.TeacherId == teacherId);
                if (assignment == null) return false;

                if (assignment.PictureUrl != null)
                    _fileStorage.Delete(assignment.PictureUrl);

                var links = _db.AssignmentAssignmentSets.Where(x => x.AssignmentId == id);
                _db.AssignmentAssignmentSets.RemoveRange(links);

                _db.Assignments.Remove(assignment);
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
