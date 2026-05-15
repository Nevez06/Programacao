using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Notifications;
using ProjetoEventX.Models;

namespace ProjetoEventX.Services
{
    public class NotificationQueryService
    {
        private readonly EventXContext _context;
        private readonly IHubContext<NotificationsHub> _notificationsHubContext;

        public NotificationQueryService(EventXContext context, IHubContext<NotificationsHub> notificationsHubContext)
        {
            _context = context;
            _notificationsHubContext = notificationsHubContext;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(int userId, int take = 100)
        {
            var normalizedTake = Math.Clamp(take, 1, 500);
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(normalizedTake)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    Link = n.Link,
                    TimeAgo = ToTimeAgo(n.CreatedAt)
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return false;
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                await PublishUnreadCountChangedAsync(userId);
            }

            return true;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Count == 0)
            {
                return 0;
            }

            foreach (var item in unread)
            {
                item.IsRead = true;
            }

            await _context.SaveChangesAsync();
            await PublishUnreadCountChangedAsync(userId);
            return unread.Count;
        }

        private async Task PublishUnreadCountChangedAsync(int userId)
        {
            var groupName = NotificationsHub.GetUserNotificationsGroupName(userId);
            var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

            await _notificationsHubContext.Clients.Group(groupName).SendAsync("UnreadCountChanged", unreadCount);
            await _notificationsHubContext.Clients.Group(groupName).SendAsync("NotificationsUpdated", new
            {
                userId,
                unreadCount
            });
        }

        private static string ToTimeAgo(DateTime dateTime)
        {
            var diff = DateTime.UtcNow - dateTime;
            if (diff.TotalMinutes < 1) return "agora";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}min";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d";
            return dateTime.ToString("dd/MM/yyyy");
        }
    }
}
