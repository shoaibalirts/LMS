using LMS_API.Data;
using LMS_API.Models;
using LMS_API.Models.DTO.Notification;
using LMS_API.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace LMS_API.Services
{
	public class NotificationService : INotificationService
	{
		private readonly ApplicationDbContext _db;

		public NotificationService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task CreateAsync(int recipientUserId, string recipientRole, string message, int? assignedAssignmentSetId = null, int? assignedAssignmentId = null)
		{
			if (recipientUserId <= 0) return;
			if (string.IsNullOrWhiteSpace(recipientRole)) return;
			if (string.IsNullOrWhiteSpace(message)) return;

			var notification = new Notification
			{
				RecipientUserId = recipientUserId,
				RecipientRole = recipientRole.Trim(),
				Message = message.Trim(),
				CreatedAtUtc = DateTime.UtcNow,
				AssignedAssignmentSetId = assignedAssignmentSetId,
				AssignedAssignmentId = assignedAssignmentId
			};

			await _db.Notifications.AddAsync(notification);
			await _db.SaveChangesAsync();
		}

		public async Task<IEnumerable<NotificationReadDTO>> GetForUserAsync(int recipientUserId, string recipientRole)
		{
			return await _db.Notifications
				.AsNoTracking()
				.Where(x => x.RecipientUserId == recipientUserId && x.RecipientRole == recipientRole)
				.OrderByDescending(x => x.CreatedAtUtc)
				.Select(x => new NotificationReadDTO
				{
					Id = x.Id,
					Message = x.Message,
					CreatedAtUtc = x.CreatedAtUtc,
					AssignedAssignmentSetId = x.AssignedAssignmentSetId,
					AssignedAssignmentId = x.AssignedAssignmentId
				})
				.ToListAsync();
		}
	}
}
