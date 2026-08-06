using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.AssignedAssignment;
using LMS_API.Models.DTO.AssignedAssignmentSet;
using LMS_API.Services.Contract;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
	public class AssignedAssignmentService : IAssignedAssignmentService
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _environment;
		private readonly INotificationService _notificationService;

		public AssignedAssignmentService(ApplicationDbContext db, IWebHostEnvironment environment, INotificationService notificationService)
		{
			_db = db;
			_environment = environment;
			_notificationService = notificationService;
		}

		public async Task<AssignedAssignmentSetReadDTO?> CreateAssignedAssignmentSetAsync(AssignedAssignmentSetCreateDTO dto, int teacherId)
		{
			if (dto.Deadline < dto.DateOfAssigned)
			{
				throw new InvalidOperationException("Deadline cannot be earlier than date of assignment.");
			}

			var studentExists = await _db.Students.AnyAsync(s => s.Id == dto.StudentId && s.CreatedByTeacherId == teacherId);
			if (!studentExists)
			{
				throw new InvalidOperationException("Student was not found for this teacher.");
			}

			var assignmentSet = await _db.AssignmentSets
				.Include(x => x.AssignmentAssignmentSets)
					.ThenInclude(link => link.Assignment)
				.FirstOrDefaultAsync(x => x.Id == dto.AssignmentSetId && x.TeacherId == teacherId && !x.IsDeleted);

			if (assignmentSet == null)
			{
				throw new InvalidOperationException("Assignment set was not found.");
			}

			var distinctAssignmentIds = assignmentSet.AssignmentAssignmentSets
				.Where(x => x.Assignment != null && !x.Assignment.IsDeleted)
				.Select(x => x.AssignmentId)
				.Distinct()
				.ToList();

			if (distinctAssignmentIds.Count == 0)
			{
				throw new InvalidOperationException("Selected assignment set contains no assignments.");
			}

			var assignmentsInSet = assignmentSet.AssignmentAssignmentSets
				.Select(x => x.Assignment)
				.Where(x => x != null && !x.IsDeleted)
				.Cast<Assignment>()
				.ToList();

			await using var transaction = await _db.Database.BeginTransactionAsync();

			var assignedSet = new AssignedAssignmentSet
			{
				TeacherId = teacherId,
				StudentId = dto.StudentId,
				AssignmentSetId = dto.AssignmentSetId,
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

			var docBytes = BuildAssignmentSetDocx(
				assignmentSet.Name,
				dto.StudentId,
				dto.DateOfAssigned,
				dto.Deadline,
				assignmentsInSet,
				GetWebRoot()
			);

			var docUploadsRoot = GetTasksetUploadsRoot();
			Directory.CreateDirectory(docUploadsRoot);
			var storedFileName = $"taskset-{assignedSet.Id}-student-{dto.StudentId}-{Guid.NewGuid():N}.docx";
			var absolutePath = Path.Combine(docUploadsRoot, storedFileName);
			await File.WriteAllBytesAsync(absolutePath, docBytes);

			assignedSet.TaskDocumentPath = Path.Combine("uploads", "assigned-tasksets", storedFileName);
			assignedSet.TaskDocumentFileName = $"{SanitizeFileName(assignmentSet.Name)}.docx";
			assignedSet.TaskDocumentContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
			await _db.SaveChangesAsync();

			await _notificationService.CreateAsync(
				dto.StudentId,
				"Student",
				$"Du har fået tildelt opgavesættet '{assignmentSet.Name}' (deadline {dto.Deadline:yyyy-MM-dd}).",
				assignedAssignmentSetId: assignedSet.Id
			);

			await transaction.CommitAsync();

			await _notificationService.CreateAsync(
				teacherId,
				"Teacher",
				$"Du har tildelt opgavesættet '{assignmentSet.Name}' til elev #{dto.StudentId} (deadline {dto.Deadline:yyyy-MM-dd}).",
				assignedAssignmentSetId: assignedSet.Id
			);

			var createdSet = await _db.AssignedAssignmentSets
				.Include(x => x.AssignmentSet)
				.Include(x => x.AssignedAssignments)
					.ThenInclude(x => x.Assignment)
				.FirstOrDefaultAsync(x => x.Id == assignedSet.Id);

			return createdSet == null ? null : MapSet(createdSet);
		}

		public async Task<IEnumerable<AssignedAssignmentSetReadDTO>> CreateAssignedAssignmentSetForClassAsync(AssignedAssignmentSetCreateForClassDTO dto, int teacherId)
		{
			if (dto.Deadline < dto.DateOfAssigned)
			{
				throw new InvalidOperationException("Deadline cannot be earlier than date of assignment.");
			}

			var studyClass = await _db.StudyClasses
				.Include(x => x.StudentStudyClasses)
				.FirstOrDefaultAsync(x => x.Id == dto.StudyClassId && x.TeacherId == teacherId);

			if (studyClass == null)
			{
				throw new InvalidOperationException("Study class was not found.");
			}

			var studentIds = studyClass.StudentStudyClasses
				.Select(x => x.StudentId)
				.Where(x => x > 0)
				.Distinct()
				.ToList();

			if (studentIds.Count == 0)
			{
				throw new InvalidOperationException("Selected class contains no students.");
			}

			var assignmentSet = await _db.AssignmentSets
				.Include(x => x.AssignmentAssignmentSets)
					.ThenInclude(link => link.Assignment)
				.FirstOrDefaultAsync(x => x.Id == dto.AssignmentSetId && x.TeacherId == teacherId && !x.IsDeleted);

			if (assignmentSet == null)
			{
				throw new InvalidOperationException("Assignment set was not found.");
			}

			var distinctAssignmentIds = assignmentSet.AssignmentAssignmentSets
				.Where(x => x.Assignment != null && !x.Assignment.IsDeleted)
				.Select(x => x.AssignmentId)
				.Distinct()
				.ToList();

			if (distinctAssignmentIds.Count == 0)
			{
				throw new InvalidOperationException("Selected assignment set contains no assignments.");
			}

			var assignmentsInSet = assignmentSet.AssignmentAssignmentSets
				.Select(x => x.Assignment)
				.Where(x => x != null && !x.IsDeleted)
				.Cast<Assignment>()
				.ToList();

			await using var transaction = await _db.Database.BeginTransactionAsync();

			var assignedSets = studentIds.Select(studentId => new AssignedAssignmentSet
			{
				TeacherId = teacherId,
				StudentId = studentId,
				AssignmentSetId = dto.AssignmentSetId,
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
			}).ToList();

			await _db.AssignedAssignmentSets.AddRangeAsync(assignedSets);
			await _db.SaveChangesAsync();

			var docUploadsRoot = GetTasksetUploadsRoot();
			Directory.CreateDirectory(docUploadsRoot);
			var webRoot = GetWebRoot();

			foreach (var assignedSet in assignedSets)
			{
				var docBytes = BuildAssignmentSetDocx(
					assignmentSet.Name,
					assignedSet.StudentId,
					dto.DateOfAssigned,
					dto.Deadline,
					assignmentsInSet,
					webRoot
				);

				var storedFileName = $"taskset-{assignedSet.Id}-student-{assignedSet.StudentId}-{Guid.NewGuid():N}.docx";
				var absolutePath = Path.Combine(docUploadsRoot, storedFileName);
				await File.WriteAllBytesAsync(absolutePath, docBytes);

				assignedSet.TaskDocumentPath = Path.Combine("uploads", "assigned-tasksets", storedFileName);
				assignedSet.TaskDocumentFileName = $"{SanitizeFileName(assignmentSet.Name)}.docx";
				assignedSet.TaskDocumentContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

				await _notificationService.CreateAsync(
					assignedSet.StudentId,
					"Student",
					$"Du har fået tildelt opgavesættet '{assignmentSet.Name}' (deadline {dto.Deadline:yyyy-MM-dd}).",
					assignedAssignmentSetId: assignedSet.Id
				);
			}

			await _db.SaveChangesAsync();
			await transaction.CommitAsync();

			await _notificationService.CreateAsync(
				teacherId,
				"Teacher",
				$"Du har tildelt opgavesættet '{assignmentSet.Name}' til klassen '{studyClass.Name}' ({studentIds.Count} elever) (deadline {dto.Deadline:yyyy-MM-dd})."
			);

			var created = await _db.AssignedAssignmentSets
				.Include(x => x.AssignmentSet)
				.Include(x => x.AssignedAssignments)
					.ThenInclude(x => x.Assignment)
				.Where(x => assignedSets.Select(s => s.Id).Contains(x.Id))
				.ToListAsync();

			return created.Select(MapSet);
		}

		public async Task<IEnumerable<AssignedAssignmentSetReadDTO>> GetAssignedSetsForStudentAsync(int studentId)
		{
			var sets = await _db.AssignedAssignmentSets
				.Include(x => x.AssignmentSet)
				.Include(x => x.AssignedAssignments)
					.ThenInclude(x => x.Assignment)
				.Where(x => x.StudentId == studentId && !x.IsRevoked)
				.OrderByDescending(x => x.DateOfAssigned)
				.ToListAsync();

			return sets.Select(MapSet);
		}

		public async Task<IEnumerable<AssignedAssignmentSetReadDTO>> GetAssignedSetsForTeacherAsync(int teacherId)
		{
			var sets = await _db.AssignedAssignmentSets
				.Include(x => x.AssignmentSet)
				.Include(x => x.AssignedAssignments)
					.ThenInclude(x => x.Assignment)
				.Where(x => x.TeacherId == teacherId && !x.IsRevoked)
				.OrderByDescending(x => x.DateOfAssigned)
				.ToListAsync();

			return sets.Select(MapSet);
		}

		public async Task<bool> DeleteAssignedAssignmentAsync(int assignedAssignmentId, int teacherId)
		{
			var assigned = await _db.AssignedAssignments
				.Include(x => x.AssignedAssignmentSet)
					.ThenInclude(s => s.AssignmentSet)
				.Include(x => x.Assignment)
				.FirstOrDefaultAsync(x => x.Id == assignedAssignmentId);

			if (assigned == null)
			{
				return false;
			}

			if (assigned.AssignedAssignmentSet == null || assigned.AssignedAssignmentSet.TeacherId != teacherId)
			{
				throw new UnauthorizedAccessException();
			}

			var studentId = assigned.AssignedAssignmentSet.StudentId;
			var assignmentSetName = assigned.AssignedAssignmentSet.AssignmentSet?.Name;
			var assignmentSubject = assigned.Assignment?.Subject;
			var assignedSetId = assigned.AssignedAssignmentSet.Id;

			_db.AssignedAssignments.Remove(assigned);
			await _db.SaveChangesAsync();

			await _notificationService.CreateAsync(
				teacherId,
				"Teacher",
				$"Du har fjernet en tildelt opgave{(string.IsNullOrWhiteSpace(assignmentSubject) ? "" : $" '{assignmentSubject}'")} fra opgavesættet{(string.IsNullOrWhiteSpace(assignmentSetName) ? "" : $" '{assignmentSetName}'")}.",
				assignedAssignmentSetId: assignedSetId
			);

			await _notificationService.CreateAsync(
				studentId,
				"Student",
				$"En tildelt opgave{(string.IsNullOrWhiteSpace(assignmentSubject) ? "" : $" '{assignmentSubject}'")} er blevet fjernet fra dit opgavesæt{(string.IsNullOrWhiteSpace(assignmentSetName) ? "" : $" '{assignmentSetName}'")}.",
				assignedAssignmentSetId: assignedSetId
			);

			return true;
		}

		public async Task<bool> RevokeAssignedAssignmentSetAsync(int assignedAssignmentSetId, int teacherId)
		{
			var assignedSet = await _db.AssignedAssignmentSets
				.Include(x => x.AssignmentSet)
				.FirstOrDefaultAsync(x => x.Id == assignedAssignmentSetId && x.TeacherId == teacherId);

			if (assignedSet == null)
			{
				return false;
			}

			if (assignedSet.IsRevoked)
			{
				return true;
			}

			assignedSet.IsRevoked = true;
			assignedSet.RevokedAtUtc = DateTime.UtcNow;
			await _db.SaveChangesAsync();

			var setName = assignedSet.AssignmentSet?.Name ?? "opgavesæt";
			await _notificationService.CreateAsync(
				assignedSet.StudentId,
				"Student",
				$"Tildelingen af '{setName}' er blevet fjernet.",
				assignedAssignmentSetId: assignedSet.Id
			);

			return true;
		}

		public async Task<int> RevokeAllAssignedAssignmentSetsForStudentAsync(int studentId, int teacherId)
		{
			if (studentId <= 0)
			{
				throw new InvalidOperationException("Student id is required.");
			}

			var studentExists = await _db.Students.AnyAsync(s => s.Id == studentId && s.CreatedByTeacherId == teacherId);
			if (!studentExists)
			{
				throw new InvalidOperationException("Student was not found for this teacher.");
			}

			var sets = await _db.AssignedAssignmentSets
				.Where(x => x.TeacherId == teacherId && x.StudentId == studentId && !x.IsRevoked)
				.ToListAsync();

			if (sets.Count == 0)
			{
				return 0;
			}

			var now = DateTime.UtcNow;
			foreach (var set in sets)
			{
				set.IsRevoked = true;
				set.RevokedAtUtc = now;
			}

			await _db.SaveChangesAsync();

			await _notificationService.CreateAsync(
				teacherId,
				"Teacher",
				$"Du har fjernet alle tildelinger for elev #{studentId} ({sets.Count} opgavesæt)."
			);

			await _notificationService.CreateAsync(
				studentId,
				"Student",
				"Alle dine tildelinger er blevet fjernet af din lærer."
			);

			return sets.Count;
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

			if (assignedAssignment.AssignedAssignmentSet.IsRevoked)
			{
				throw new InvalidOperationException("This assignment set is no longer assigned.");
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

			await _notificationService.CreateAsync(
				assignedAssignment.AssignedAssignmentSet.TeacherId,
				"Teacher",
				$"Elev #{studentId} har afleveret '{assignedAssignment.Assignment?.Subject ?? "opgave"}'.",
				assignedAssignmentSetId: assignedAssignment.AssignedAssignmentSetId,
				assignedAssignmentId: assignedAssignmentId
			);

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

			if (assignedAssignment.AssignedAssignmentSet.IsRevoked)
			{
				throw new InvalidOperationException("This assignment set is no longer assigned.");
			}

			if (assignedAssignment.AssignedAssignmentSet.TeacherId != teacherId)
			{
				throw new UnauthorizedAccessException("You are not allowed to edit feedback for this assignment.");
			}

			assignedAssignment.Feedback = string.IsNullOrWhiteSpace(feedback) ? null : feedback.Trim();
			await _db.SaveChangesAsync();

			await _notificationService.CreateAsync(
				assignedAssignment.AssignedAssignmentSet.StudentId,
				"Student",
				$"Du har fået feedback på '{assignedAssignment.Assignment?.Subject ?? "opgave"}'.",
				assignedAssignmentSetId: assignedAssignment.AssignedAssignmentSetId,
				assignedAssignmentId: assignedAssignmentId
			);
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

			if (assignedAssignment.AssignedAssignmentSet.IsRevoked)
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

		public async Task<(byte[] Content, string ContentType, string FileName)?> GetAssignmentSetDocumentFileAsync(int assignedAssignmentSetId, int requesterUserId, bool isTeacher)
		{
			var assignedSet = await _db.AssignedAssignmentSets
				.FirstOrDefaultAsync(x => x.Id == assignedAssignmentSetId);

			if (assignedSet == null || string.IsNullOrWhiteSpace(assignedSet.TaskDocumentPath))
			{
				return null;
			}

			if (assignedSet.IsRevoked)
			{
				return null;
			}

			var isOwnerTeacher = isTeacher && assignedSet.TeacherId == requesterUserId;
			var isOwnerStudent = !isTeacher && assignedSet.StudentId == requesterUserId;

			if (!isOwnerTeacher && !isOwnerStudent)
			{
				throw new UnauthorizedAccessException("You are not allowed to view this file.");
			}

			var absolutePath = Path.Combine(GetContentRoot(), assignedSet.TaskDocumentPath);
			if (!File.Exists(absolutePath))
			{
				return null;
			}

			var bytes = await File.ReadAllBytesAsync(absolutePath);
			return (
				bytes,
				assignedSet.TaskDocumentContentType ?? "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
				assignedSet.TaskDocumentFileName ?? Path.GetFileName(absolutePath)
			);
		}

		private AssignedAssignmentSetReadDTO MapSet(AssignedAssignmentSet set)
		{
			return new AssignedAssignmentSetReadDTO
			{
				Id = set.Id,
				TeacherId = set.TeacherId,
				StudentId = set.StudentId,
				AssignmentSetId = set.AssignmentSetId,
				AssignmentSetName = set.AssignmentSet?.Name,
				TaskDocumentFileName = set.TaskDocumentFileName,
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

		private string GetTasksetUploadsRoot()
		{
			return Path.Combine(GetContentRoot(), "uploads", "assigned-tasksets");
		}

		private static string SanitizeFileName(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return "opgavesaet";
			var invalid = Path.GetInvalidFileNameChars();
			var cleaned = new string(value.Where(ch => !invalid.Contains(ch)).ToArray()).Trim();
			return string.IsNullOrWhiteSpace(cleaned) ? "opgavesaet" : cleaned;
		}

		private byte[] BuildAssignmentSetDocx(
			string assignmentSetName,
			int studentId,
			DateOnly dateOfAssigned,
			DateOnly deadline,
			IReadOnlyList<Assignment> assignments,
			string webRootPath)
		{
			using var ms = new MemoryStream();
			using (var document = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
			{
				var mainPart = document.AddMainDocumentPart();
				mainPart.Document = new Document(new Body());
				var body = mainPart.Document.Body!;

				body.Append(CreateHeading($"Opgavesaet: {assignmentSetName}", 32));
				body.Append(CreateParagraph($"Elev ID: {studentId}"));
				body.Append(CreateParagraph($"Tildelt: {dateOfAssigned:yyyy-MM-dd}"));
				body.Append(CreateParagraph($"Deadline: {deadline:yyyy-MM-dd}"));
				body.Append(new Paragraph(new Run(new Break())));

				for (var index = 0; index < assignments.Count; index++)
				{
					var assignment = assignments[index];
					var number = index + 1;
					body.Append(CreateHeading($"Opgave {number}: {assignment.Subject}", 28));
					body.Append(CreateParagraph($"Type: {assignment.Type}  |  Niveau: {assignment.ClassLevel}  |  Point: {assignment.Points}"));

					if (!string.IsNullOrWhiteSpace(assignment.PictureUrl))
					{
						if (TryLoadLocalAssignmentImage(webRootPath, assignment.PictureUrl, out var imageBytes, out var imageType))
						{
							body.Append(CreateParagraph("Billede:"));
							body.Append(CreateImageParagraph(mainPart, imageBytes, imageType));
						}
						else
						{
							body.Append(CreateParagraph($"Billede: {assignment.PictureUrl}"));
						}
					}

					if (!string.IsNullOrWhiteSpace(assignment.VideoUrl))
					{
						body.Append(CreateParagraph($"Video: {assignment.VideoUrl}"));
					}

					body.Append(CreateParagraph("Svar:"));
					for (var i = 0; i < 8; i++)	// plads til besvarelse
					{
						body.Append(CreateParagraph(" "));
					}

					if (index < assignments.Count - 1)
					{
						body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
					}
				}

				mainPart.Document.Save();
			}

			return ms.ToArray();
		}

		private static bool TryLoadLocalAssignmentImage(string webRootPath, string pictureUrl, out byte[] bytes, out PartTypeInfo imagePartType)
		{
			bytes = Array.Empty<byte>();
			imagePartType = ImagePartType.Png;

			if (string.IsNullOrWhiteSpace(pictureUrl)) return false;
			if (pictureUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return false;

			var relative = pictureUrl.Trim();
			if (relative.StartsWith('/')) relative = relative.TrimStart('/');
			relative = relative.Replace('/', Path.DirectorySeparatorChar);

			var absolutePath = Path.Combine(webRootPath, relative);
			if (!File.Exists(absolutePath)) return false;

			var extension = Path.GetExtension(absolutePath);
			if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase)) imagePartType = ImagePartType.Png;
			else if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) imagePartType = ImagePartType.Jpeg;
			else if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase)) imagePartType = ImagePartType.Gif;
			else return false;

			bytes = File.ReadAllBytes(absolutePath);
			return bytes.Length > 0;
		}

		private static DocumentFormat.OpenXml.Wordprocessing.Paragraph CreateImageParagraph(MainDocumentPart mainPart, byte[] imageBytes, PartTypeInfo imagePartType)
		{
			var imagePart = mainPart.AddImagePart(imagePartType);
			using (var stream = new MemoryStream(imageBytes))
			{
				imagePart.FeedData(stream);
			}

			var relationshipId = mainPart.GetIdOfPart(imagePart);

			// Simple fixed size (keeps implementation minimal)
			const long widthEmus = 500L * 9525L;
			const long heightEmus = 320L * 9525L;

			var docPrId = (UInt32Value)(uint)(Math.Abs(Guid.NewGuid().GetHashCode()) + 1);
			var nvId = (UInt32Value)(uint)(Math.Abs(Guid.NewGuid().GetHashCode()) + 1);

			var element = new Drawing(
				new Inline(
					new Extent { Cx = widthEmus, Cy = heightEmus },
					new EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
					new DocProperties { Id = docPrId, Name = "AssignmentImage" },
					new NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
					new A.Graphic(
						new A.GraphicData(
							new PIC.Picture(
								new PIC.NonVisualPictureProperties(
									new PIC.NonVisualDrawingProperties { Id = nvId, Name = "image" },
									new PIC.NonVisualPictureDrawingProperties()),
								new PIC.BlipFill(
									new A.Blip { Embed = relationshipId },
									new A.Stretch(new A.FillRectangle())),
								new PIC.ShapeProperties(
									new A.Transform2D(
										new A.Offset { X = 0L, Y = 0L },
										new A.Extents { Cx = widthEmus, Cy = heightEmus }),
									new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
						)
						{ Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
					)
				)
				{
					DistanceFromTop = (UInt32Value)0U,
					DistanceFromBottom = (UInt32Value)0U,
					DistanceFromLeft = (UInt32Value)0U,
					DistanceFromRight = (UInt32Value)0U
				});

			return new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new Run(element));
		}

		private static Paragraph CreateHeading(string text, int fontSize)
		{
			var runProps = new RunProperties(new Bold(), new FontSize { Val = fontSize.ToString() });
			var run = new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
			return new Paragraph(run);
		}

		private static Paragraph CreateParagraph(string text)
		{
			var run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
			return new Paragraph(run);
		}

		private string GetWebRoot()
		{
			return string.IsNullOrWhiteSpace(_environment.WebRootPath)
				? Path.Combine(_environment.ContentRootPath, "wwwroot")
				: _environment.WebRootPath;
		}
	}
}
