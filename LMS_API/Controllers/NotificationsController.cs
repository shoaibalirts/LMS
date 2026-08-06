using LMS_API.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_API.Controllers
{
	[Route("api/notifications")]
	[ApiController]
	[Authorize]
	public class NotificationsController : ControllerBase
	{
		private readonly INotificationService _notificationService;
		private readonly ITokenService _tokenService;

		public NotificationsController(INotificationService notificationService, ITokenService tokenService)
		{
			_notificationService = notificationService;
			_tokenService = tokenService;
		}

		[HttpGet]
		public async Task<IActionResult> GetMyNotifications()
		{
			if (!_tokenService.TryGetUserId(User, out var userId))
			{
				return Unauthorized("Missing or invalid identity.");
			}

			var isTeacher = User.IsInRole("Teacher");
			var isStudent = User.IsInRole("Student");
			if (!isTeacher && !isStudent)
			{
				return Forbid();
			}

			var role = isTeacher ? "Teacher" : "Student";
			var notifications = await _notificationService.GetForUserAsync(userId, role);
			return Ok(notifications);
		}
	}
}
