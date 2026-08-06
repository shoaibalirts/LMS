using AutoMapper;
using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.StudyClass;
using LMS_API.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services;

public class StudyClassService : IStudyClassService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public StudyClassService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<StudyClassReadDTO> CreateStudyClassAsync(StudyClassCreateDTO dto, int teacherId)
    {
        var studyClass = new StudyClass
        {
            Name = dto.Name,
            TeacherId = teacherId,
            CreatedDate = DateTime.UtcNow
        };

        _context.StudyClasses.Add(studyClass);
        await _context.SaveChangesAsync();

        // reload with relations (important for DTO mapping)
        var fullEntity = await _context.StudyClasses
            .Include(sc => sc.StudentStudyClasses)
                .ThenInclude(x => x.Student)
            .FirstOrDefaultAsync(sc => sc.Id == studyClass.Id);

        return _mapper.Map<StudyClassReadDTO>(fullEntity);
    }

    public async Task<IEnumerable<StudyClassReadDTO>> GetStudyClassesByTeacherAsync(int teacherId)
    {
        var classes = await _context.StudyClasses
            .AsNoTracking()
            .Include(sc => sc.StudentStudyClasses)
                .ThenInclude(x => x.Student)
            .Where(sc => sc.TeacherId == teacherId)
            .OrderByDescending(sc => sc.CreatedDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<StudyClassReadDTO>>(classes);
    }

    public async Task<StudyClassReadDTO?> GetStudyClassByIdAsync(int id, int teacherId)
    {
        var studyClass = await _context.StudyClasses
            .AsNoTracking()
            .Include(sc => sc.StudentStudyClasses)
                .ThenInclude(x => x.Student)
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.TeacherId == teacherId);

        if (studyClass == null)
            return null;

        return _mapper.Map<StudyClassReadDTO>(studyClass);
    }

    public async Task<bool> DeleteStudyClassAsync(int id, int teacherId)
    {
        var studyClass = await _context.StudyClasses
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.TeacherId == teacherId);

        if (studyClass == null)
            return false;

        _context.StudyClasses.Remove(studyClass);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<StudyClassReadDTO?> AddStudentsToStudyClassAsync(StudyClassSyncDTO dto, int teacherId)
    {
        if (dto.Id == null)
            return null;

        var studyClassId = dto.Id.Value;

        var studyClass = await _context.StudyClasses
            .Include(sc => sc.StudentStudyClasses)
                .ThenInclude(x => x.Student)
            .FirstOrDefaultAsync(sc => sc.Id == studyClassId && sc.TeacherId == teacherId);

        if (studyClass == null)
            return null;

        var requestedStudentIds = dto.StudentIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (requestedStudentIds.Count == 0)
        {
            return _mapper.Map<StudyClassReadDTO>(studyClass);
        }

        var validStudentIds = await _context.Students
            .AsNoTracking()
            .Where(s => requestedStudentIds.Contains(s.Id) && s.CreatedByTeacherId == teacherId)
            .Select(s => s.Id)
            .ToListAsync();

        var invalidStudentIds = requestedStudentIds.Except(validStudentIds).ToList();
        if (invalidStudentIds.Count > 0)
        {
            throw new InvalidOperationException(
                $"One or more students were not found for this teacher: {string.Join(", ", invalidStudentIds)}");
        }

        var existingStudentIds = studyClass.StudentStudyClasses
            .Select(x => x.StudentId)
            .ToHashSet();

        var newLinks = validStudentIds
            .Where(id => !existingStudentIds.Contains(id))
            .Select(id => new StudentStudyClass
            {
                StudentId = id,
                StudyClassId = studyClassId
            });

        _context.StudentStudyClasses.AddRange(newLinks);
        await _context.SaveChangesAsync();

        // reload updated relations (prevents stale mapping + cycle issues)
        await _context.Entry(studyClass)
            .Collection(x => x.StudentStudyClasses)
            .Query()
            .Include(x => x.Student)
            .LoadAsync();

        return _mapper.Map<StudyClassReadDTO>(studyClass);
    }
}