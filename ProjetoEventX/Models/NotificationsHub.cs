using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjetoEventX.Services;

namespace ProjetoEventX.Models
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        private readonly ChatService _chatService;

        public NotificationsHub(ChatService chatService)
        {
            _chatService = chatService;
        }

        public static string GetUserNotificationsGroupName(int userId)
        {
            return $"notifications-user-{userId}";
        }

        public override async Task OnConnectedAsync()
        {
            var userId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (userId.HasValue)
            {
                await _chatService.MarkUserOnlineAsync(userId.Value, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, GetUserNotificationsGroupName(userId.Value));
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatService.GetUserGroupName(userId.Value));
                await Clients.Others.SendAsync("UserOnline", userId.Value);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (userId.HasValue)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserNotificationsGroupName(userId.Value));
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatService.GetUserGroupName(userId.Value));

                var presence = await _chatService.MarkUserOfflineAsync(userId.Value, Context.ConnectionId);
                if (!presence.IsOnline)
                {
                    await Clients.Others.SendAsync("UserOffline", userId.Value);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinConversation(int conversationId)
        {
            var userId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (!userId.HasValue)
            {
                throw new HubException("Usuario nao autenticado.");
            }

            if (conversationId <= 0)
            {
                throw new HubException("Conversa invalida.");
            }

            var isParticipant = await _chatService.IsConversationParticipantAsync(conversationId, userId.Value);
            if (!isParticipant)
            {
                throw new HubException("Sem permissao para participar desta conversa.");
            }

            var groupName = ChatService.GetConversationGroupName(conversationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await _chatService.TouchHeartbeatAsync(userId.Value, Context.ConnectionId);

            await Clients.Group(groupName).SendAsync("UserJoinedConversation", new
            {
                conversationId,
                userId = userId.Value,
                at = DateTime.UtcNow
            });
        }

        public async Task LeaveConversation(int conversationId)
        {
            var userId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (!userId.HasValue)
            {
                throw new HubException("Usuario nao autenticado.");
            }

            if (conversationId <= 0)
            {
                throw new HubException("Conversa invalida.");
            }

            var groupName = ChatService.GetConversationGroupName(conversationId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await _chatService.TouchHeartbeatAsync(userId.Value, Context.ConnectionId);

            await Clients.Group(groupName).SendAsync("UserLeftConversation", new
            {
                conversationId,
                userId = userId.Value,
                at = DateTime.UtcNow
            });
        }

        public async Task Heartbeat()
        {
            var userId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (!userId.HasValue)
            {
                throw new HubException("Usuario nao autenticado.");
            }

            await _chatService.TouchHeartbeatAsync(userId.Value, Context.ConnectionId);
        }

        public async Task Typing(int conversationId)
        {
            var userId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (!userId.HasValue)
            {
                throw new HubException("Usuario nao autenticado.");
            }

            if (conversationId <= 0)
            {
                throw new HubException("Conversa invalida.");
            }

            var isParticipant = await _chatService.IsConversationParticipantAsync(conversationId, userId.Value);
            if (!isParticipant)
            {
                throw new HubException("Sem permissao para participar desta conversa.");
            }

            var groupName = ChatService.GetConversationGroupName(conversationId);
            await _chatService.TouchHeartbeatAsync(userId.Value, Context.ConnectionId);

            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", new
            {
                conversationId,
                userId = userId.Value,
                at = DateTime.UtcNow
            });
        }
    }
}
