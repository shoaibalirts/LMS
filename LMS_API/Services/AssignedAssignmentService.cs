using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.AssignedAssignment;
using LMS_API.Models.DTO.AssignedAssignmentSet;
using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
	public class AssignedAssignmentService : IAssignedAssignmentService
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _environment;

		public AssignedAssignmentService(ApplicationDbContext db, IWebHostEnvironment environment)
		{
			_db = db;
			_environment = environment;
		}

		public async Task<AssignedAssignmentSetReadDTO?> CreateAssignedAssignmentSetAsync(AssignedAssignmentSetCreateDTO dto, int teacherId)
		{
			if (dto.AssignmentIds.Count == 0)
			{
				throw new InvalidOperationException("At least one assignment must be selected.");
			}

			if (dto.Deadline < dto.DateOfAssigned)
			{
				throw new InvalidOperationException("Deadline cannot be earlier than date of assignment.");
			}

			var studentExists = await _db.Students.AnyAsync(s => s.Id == dto.StudentId);
			if (!studentExists)
			{
				throw new InvalidOperationException("Student was not found.");
			}

			var distinctAssignmentIds = dto.AssignmentIds.Distinct().ToList();
			var validAssignmentIds = await _db.Assignments
				.Where(a => a.TeacherId == teacherId && distinctAssignmentIds.Contains(a.Id))
				.Select(a => a.Id)
				.ToListAsync();

			if (validAssignmentIds.Count != distinctAssignmentIds.Count)
			{
				throw new InvalidOperationException("One or more assignments do not belong to this teacher.");
			}

			var assignedSet = new AssignedAssignmentSet
			{
				TeacherId = teacherId,
				StudentId = dto.StudentId,
				DateOfAssigned = dto.DateOfAssigned,
				Deadline = dto.Deadline,
				AssignedAssignments = distinctAssignmentIds.Select(assignmentId => new AssignedAssignment
				{
					AssignmentId = assignmentId,
					StudentResultPath = null,
					StudentResultFileName = null,
					StudentResultContentType = null,
					SubmittedAtUtc = null,
					Feedback = null
				}).ToList()
			};

			await _db.AssignedAssignmentSets.AddAsync(assignedSet);
			await _db.SaveChangesAsync();

			var createdSet = await _db.AssignedAssignmentSets
				.Include(x => x.AssignedAssignments)
					.ThenInclude(x => x.Assignment)
				.FirstOrDefaultAsync(x => x.Id == assignedSet.Id);

			return createdSet == null ? null : MapSet(createdSet);
		}

		public async Task<IEnumerable<AssignedAssignmentSetReadDTO>> GetAssignedSetsForStudentAsync(int studentId)
		{
			var sets = await _db.AssignedAssignmentSets
				.Include(x => x.AssignedAssignments)
					.ThenInclude(x => x.Assignment)
				.Where(x => x.StudentId == studentId)
				.OrderByDescending(x => x.DateOfAssigned)
				.ToListAsync();

			return sets.Select(MapSet);
		}

		public async Task<IEnumerable<AssignedAssignmentSetReadDTO>> GetAssignedSetsForTeacherAsync(int teacherId)
		{
			var sets = await _db.AssignedAssignmentSets
				.Include(x => x.AssignedAssignments)
					.ThenInclude(x => x.Assignment)
				.Where(x => x.TeacherId == teacherId)
				.OrderByDescending(x => x.DateOfAssigned)
				.ToListAsync();

			return sets.Select(MapSet);
		}

		public async Task<AssignedAssignmentReadDTO?> SubmitStudentResultAsync(int assignedAssignmentId, int studentId, IFormFile resultFile)
		{
			var assignedAssignment = await _db.AssignedAssignments
				.Include(x => x.AssignedAssignmentSet)
				.Include(x => x.Assignment)
				.FirstOrDefaultAsync(x => x.Id == assignedAssignmentId);

			if (assignedAssignment == null)
			{
				throw new InvalidOperationException("Assigned assignment was not found.");
			}

			if (assignedAssignment.AssignedAssignmentSet.StudentId != studentId)
			{
				throw new UnauthorizedAccessException("You are not allowed to submit this assignment.");
			}

			var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
			if (todayUtc > assignedAssignment.AssignedAssignmentSet.Deadline)
			{
				throw new InvalidOperationException("Deadline has passed. Submission is no longer allowed.");
			}

			var uploadsRoot = GetUploadsRoot();
			Directory.CreateDirectory(uploadsRoot);

			var extension = Path.GetExtension(resultFile.FileName);
			var uniqueFileName = $"assignment-{assignedAssignmentId}-student-{studentId}-{Guid.NewGuid():N}{extension}";
			var absolutePath = Path.Combine(uploadsRoot, uniqueFileName);

			await using (var fileStream = File.Create(absolutePath))
			{
				await resultFile.CopyToAsync(fileStream);
			}

			if (!string.IsNullOrWhiteSpace(assignedAssignment.StudentResultPath))
			{
				var existingAbsolutePath = Path.Combine(GetContentRoot(), assignedAssignment.StudentResultPath);
				if (File.Exists(existingAbsolutePath))
				{
					File.Delete(existingAbsolutePath);
				}
			}

			assignedAssignment.StudentResultPath = Path.Combine("uploads", "assigned-results", uniqueFileName);
			assignedAssignment.StudentResultFileName = resultFile.FileName;
			assignedAssignment.StudentResultContentType = string.IsNullOrWhiteSpace(resultFile.ContentType)
				? "application/pdf"
				: resultFile.ContentType;
			assignedAssignment.SubmittedAtUtc = DateTime.UtcNow;

			await _db.SaveChangesAsync();

			return MapAssignment(assignedAssignment);
		}

		public async Task<AssignedAssignmentReadDTO?> UpdateFeedbackAsync(int assignedAssignmentId, int teacherId, string? feedback)
		{
			var assignedAssignment = await _db.AssignedAssignments
				.Include(x => x.AssignedAssignmentSet)
				.Include(x => x.Assignment)
				.FirstOrDefaultAsync(x => x.Id == assignedAssignmentId);

			if (assignedAssignment == null)
			{
				return null;
			}

			if (assignedAssignment.AssignedAssignmentSet.TeacherId != teacherId)
			{
				throw new UnauthorizedAccessException("You are not allowed to edit feedback for this assignment.");
			}

			assignedAssignment.Feedback = string.IsNullOrWhiteSpace(feedback) ? null : feedback.Trim();
			await _db.SaveChangesAsync();
			return MapAssignment(assignedAssignment);
		}

		public async Task<(byte[] Content, string ContentType, string FileName)?> GetStudentResultFileAsync(int assignedAssignmentId, int requesterUserId, bool isTeacher)
		{
			var assignedAssignment = await _db.AssignedAssignments
				.Include(x => x.AssignedAssignmentSet)
				.FirstOrDefaultAsync(x => x.Id == assignedAssignmentId);

			if (assignedAssignment == null || string.IsNullOrWhiteSpace(assignedAssignment.StudentResultPath))
			{
				return null;
			}

			var isOwnerTeacher = isTeacher && assignedAssignment.AssignedAssignmentSet.TeacherId == requesterUserId;
			var isOwnerStudent = !isTeacher && assignedAssignment.AssignedAssignmentSet.StudentId == requesterUserId;

			if (!isOwnerTeacher && !isOwnerStudent)
			{
				throw new UnauthorizedAccessException("You are not allowed to view this file.");
			}

			var absolutePath = Path.Combine(GetContentRoot(), assignedAssignment.StudentResultPath);
			if (!File.Exists(absolutePath))
			{
				return null;
			}

			var bytes = await File.ReadAllBytesAsync(absolutePath);
			return (
				bytes,
				assignedAssignment.StudentResultContentType ?? "application/pdf",
				assignedAssignment.StudentResultFileName ?? Path.GetFileName(absolutePath)
			);
		}

		private AssignedAssignmentSetReadDTO MapSet(AssignedAssignmentSet set)
		{
			return new AssignedAssignmentSetReadDTO
			{
				Id = set.Id,
				TeacherId = set.TeacherId,
				StudentId = set.StudentId,
				DateOfAssigned = set.DateOfAssigned,
				Deadline = set.Deadline,
				AssignedAssignments = set.AssignedAssignments.Select(MapAssignment).ToList()
			};
		}

		private AssignedAssignmentReadDTO MapAssignment(AssignedAssignment assignment)
		{
			return new AssignedAssignmentReadDTO
			{
				Id = assignment.Id,
				AssignmentId = assignment.AssignmentId,
				AssignmentSubject = assignment.Assignment?.Subject ?? string.Empty,
				AssignmentType = assignment.Assignment?.Type ?? string.Empty,
				ClassLevel = assignment.Assignment?.ClassLevel ?? string.Empty,
				AssignmentPoints = assignment.Assignment?.Points ?? 0,
				StudentResultPath = assignment.StudentResultPath,
				StudentResultFileName = assignment.StudentResultFileName,
				SubmittedAtUtc = assignment.SubmittedAtUtc,
				Feedback = assignment.Feedback
			};
		}

		private string GetContentRoot()
		{
			return string.IsNullOrWhiteSpace(_environment.ContentRootPath)
				? Directory.GetCurrentDirectory()
				: _environment.ContentRootPath;
		}

		private string GetUploadsRoot()
		{
			return Path.Combine(GetContentRoot(), "uploads", "assigned-results");
		}
	}
}
