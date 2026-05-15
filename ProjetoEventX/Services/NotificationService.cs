using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Services
{
    public class NotificationService
    {
        private readonly EventXContext _context;
        private readonly IHubContext<NotificationsHub> _notificationsHubContext;

        public NotificationService(EventXContext context, IHubContext<NotificationsHub> notificationsHubContext)
        {
            _context = context;
            _notificationsHubContext = notificationsHubContext;
        }

        public async Task CreateAsync(int userId, string title, string message, string type, string? link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Link = link
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            await PublishRealtimeUpdateAsync(userId, notification);
        }

        public async Task CreateForManyAsync(IEnumerable<int> userIds, string title, string message, string type, string? link = null)
        {
            var distinctUserIds = userIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            foreach (var userId in distinctUserIds)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Link = link
                });
            }

            await _context.SaveChangesAsync();
            await PublishRealtimeUpdatesAsync(distinctUserIds);
        }

        private async Task PublishRealtimeUpdatesAsync(IEnumerable<int> userIds)
        {
            foreach (var userId in userIds)
            {
                await PublishRealtimeUpdateAsync(userId);
            }
        }

        private async Task PublishRealtimeUpdateAsync(int userId, Notification? notification = null)
        {
            var groupName = NotificationsHub.GetUserNotificationsGroupName(userId);

            if (notification != null)
            {
                await _notificationsHubContext.Clients.Group(groupName).SendAsync("NotificationReceived", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    isRead = notification.IsRead,
                    createdAt = notification.CreatedAt,
                    link = notification.Link
                });
            }

            var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            await _notificationsHubContext.Clients.Group(groupName).SendAsync("UnreadCountChanged", unreadCount);
            await _notificationsHubContext.Clients.Group(groupName).SendAsync("NotificationsUpdated", new
            {
                userId,
                unreadCount
            });
        }
    }
}
