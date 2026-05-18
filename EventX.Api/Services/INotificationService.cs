using EventX.Api.DTOs.Notifications;

namespace EventX.Api.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> MarkAsReadAsync(Guid userId, int notificationId, CancellationToken cancellationToken);

    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);

    Task CreateForUserAsync(
        Guid userId,
        string type,
        string title,
        string message,
        string? link,
        CancellationToken cancellationToken);
}
