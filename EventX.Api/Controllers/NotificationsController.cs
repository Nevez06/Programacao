using EventX.Api.Auth;
using EventX.Api.DTOs.Notifications;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Get(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var notifications = await _notificationService.GetNotificationsAsync(userId, take, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(new UnreadCountDto { Count = count });
    }

    [HttpPut("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var updated = await _notificationService.MarkAsReadAsync(userId, id, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpPut("read-all")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UnreadCountDto>> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var count = await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(new UnreadCountDto { Count = count });
    }
}
