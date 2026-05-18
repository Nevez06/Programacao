using EventX.Api.Data;
using EventX.Api.DTOs.Notifications;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;

    public NotificationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken)
    {
        var safeTake = Math.Clamp(take <= 0 ? 100 : take, 1, 300);

        var items = await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(safeTake)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(Guid userId, int notificationId, CancellationToken cancellationToken)
    {
        if (notificationId <= 0)
        {
            return false;
        }

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                x => x.Id == notificationId && x.UserId == userId,
                cancellationToken);

        if (notification is null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var unread = await _dbContext.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var item in unread)
            {
                item.IsRead = true;
                item.ReadAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return await GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task CreateForUserAsync(
        Guid userId,
        string type,
        string title,
        string message,
        string? link,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty ||
            string.IsNullOrWhiteSpace(type) ||
            string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _dbContext.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type.Trim().ToLowerInvariant(),
            Title = title.Trim(),
            Message = message.Trim(),
            Link = string.IsNullOrWhiteSpace(link) ? null : link.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto Map(Notification entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Message = entity.Message,
            Type = entity.Type,
            IsRead = entity.IsRead,
            CreatedAt = entity.CreatedAt,
            Link = entity.Link,
            TimeAgo = BuildTimeAgo(entity.CreatedAt)
        };
    }

    private static string BuildTimeAgo(DateTime createdAtUtc)
    {
        var delta = DateTime.UtcNow - createdAtUtc;
        if (delta.TotalMinutes < 1)
        {
            return "agora";
        }

        if (delta.TotalHours < 1)
        {
            var minutes = Math.Max(1, (int)delta.TotalMinutes);
            return $"{minutes} min";
        }

        if (delta.TotalDays < 1)
        {
            var hours = Math.Max(1, (int)delta.TotalHours);
            return $"{hours} h";
        }

        var days = Math.Max(1, (int)delta.TotalDays);
        return $"{days} d";
    }
}
