using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjetoEventX.DTOs.Notifications;
using ProjetoEventX.Models;
using ProjetoEventX.Services;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsApiController : ControllerBase
    {
        private readonly NotificationQueryService _notificationQueryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsApiController(
            NotificationQueryService notificationQueryService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationQueryService = notificationQueryService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications([FromQuery] int take = 100)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var notifications = await _notificationQueryService.GetNotificationsAsync(user.Id, take);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadCountDto>> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var count = await _notificationQueryService.GetUnreadCountAsync(user.Id);
            return Ok(new UnreadCountDto { Count = count });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var updated = await _notificationQueryService.MarkAsReadAsync(user.Id, id);
            if (!updated)
            {
                return NotFound(new { message = "Notificacao nao encontrada." });
            }

            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<ActionResult<UnreadCountDto>> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var updatedCount = await _notificationQueryService.MarkAllAsReadAsync(user.Id);
            return Ok(new UnreadCountDto { Count = updatedCount });
        }
    }
}
