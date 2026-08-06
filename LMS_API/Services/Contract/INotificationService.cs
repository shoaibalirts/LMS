using LMS_API.Models.DTO.Notification;

namespace LMS_API.Services.Contract
{
	public interface INotificationService
	{
		Task CreateAsync(int recipientUserId, string recipientRole, string message, int? assignedAssignmentSetId = null, int? assignedAssignmentId = null);
		Task<IEnumerable<NotificationReadDTO>> GetForUserAsync(int recipientUserId, string recipientRole);
	}
}
