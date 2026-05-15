using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Chat;
using ProjetoEventX.Models;
using System.Security.Claims;

namespace ProjetoEventX.Services
{
    public class ChatService
    {
        public const string ConversationGroupPrefix = "conversation-";
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatService(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public static int? GetUserIdFromClaims(ClaimsPrincipal principal)
        {
            var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }

        public static string GetConversationGroupName(int conversationId)
        {
            return $"{ConversationGroupPrefix}{conversationId}";
        }

        public static string GetUserGroupName(int userId)
        {
            return $"user-{userId}";
        }

        public static string GetLegacyUserGroupName(int userId)
        {
            return $"User_{userId}";
        }

        public async Task<IReadOnlyList<ChatConversationDto>> GetConversationsAsync(int userId, int take = 50)
        {
            var normalizedTake = Math.Clamp(take, 1, 200);

            var conversationIds = await _context.ConversationParticipants
                .AsNoTracking()
                .Where(cp => cp.UserId == userId && cp.IsActive)
                .Select(cp => cp.ConversationId)
                .Distinct()
                .ToListAsync();

            if (conversationIds.Count == 0)
            {
                return Array.Empty<ChatConversationDto>();
            }

            var conversations = await _context.Conversations
                .AsNoTracking()
                .Where(c => conversationIds.Contains(c.Id) && !c.IsArchived)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(normalizedTake)
                .Select(c => new ChatConversationDto
                {
                    Id = c.Id,
                    Title = c.Title ?? string.Empty,
                    ParticipantCount = c.Participants.Count(p => p.IsActive),
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    LastMessage = c.Messages
                        .Where(m => !m.IsDeleted)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => new ChatMessageDto
                        {
                            Id = m.Id,
                            ConversationId = m.ConversationId,
                            SenderUserId = m.SenderUserId,
                            SenderName = m.SenderUser != null
                                ? (m.SenderUser.UserName ?? m.SenderUser.Email ?? $"Usuario {m.SenderUserId}")
                                : $"Usuario {m.SenderUserId}",
                            Content = m.Content,
                            CreatedAt = m.CreatedAt,
                            IsMine = m.SenderUserId == userId
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            foreach (var item in conversations)
            {
                if (string.IsNullOrWhiteSpace(item.Title))
                {
                    item.Title = $"Conversa #{item.Id}";
                }
            }

            return conversations;
        }

        public async Task<IReadOnlyList<ChatMessageDto>> GetConversationMessagesAsync(int userId, int conversationId, int take = 100)
        {
            var normalizedTake = Math.Clamp(take, 1, 500);

            if (!await IsConversationParticipantAsync(conversationId, userId))
            {
                throw new UnauthorizedAccessException("Usuario nao participa desta conversa.");
            }

            var messages = await _context.Messages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Take(normalizedTake)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderUserId = m.SenderUserId,
                    SenderName = m.SenderUser != null
                        ? (m.SenderUser.UserName ?? m.SenderUser.Email ?? $"Usuario {m.SenderUserId}")
                        : $"Usuario {m.SenderUserId}",
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                    IsMine = m.SenderUserId == userId
                })
                .ToListAsync();

            return messages;
        }

        public async Task<ChatMessageDto> SendMessageAsync(int userId, int conversationId, string? rawContent)
        {
            if (conversationId <= 0)
            {
                throw new InvalidOperationException("Conversa invalida.");
            }

            var content = (rawContent ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Mensagem vazia.");
            }

            if (!await IsConversationParticipantAsync(conversationId, userId))
            {
                throw new UnauthorizedAccessException("Usuario nao participa desta conversa.");
            }

            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsArchived);
            if (conversation == null)
            {
                throw new InvalidOperationException("Conversa nao encontrada.");
            }

            var message = new Message
            {
                ConversationId = conversationId,
                SenderUserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Messages.Add(message);
            conversation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var sender = await _userManager.FindByIdAsync(userId.ToString());
            return new ChatMessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderUserId = message.SenderUserId,
                SenderName = sender?.UserName ?? sender?.Email ?? $"Usuario {userId}",
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsMine = true
            };
        }

        public async Task<bool> IsConversationParticipantAsync(int conversationId, int userId)
        {
            if (conversationId <= 0 || userId <= 0)
            {
                return false;
            }

            return await _context.ConversationParticipants
                .AsNoTracking()
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId && cp.IsActive);
        }

        public async Task<UserPresence> MarkUserOnlineAsync(int userId, string? connectionId = null)
        {
            var now = DateTime.UtcNow;
            var normalizedConnectionId = NormalizeConnectionId(connectionId);

            var presence = await _context.UserPresences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (presence == null)
            {
                presence = new UserPresence
                {
                    UserId = userId,
                    IsOnline = true,
                    ActiveConnections = 1,
                    LastHeartbeatAt = now,
                    LastSeenAt = now,
                    LastConnectionId = normalizedConnectionId,
                    UpdatedAt = now
                };

                _context.UserPresences.Add(presence);
            }
            else
            {
                presence.ActiveConnections += 1;
                presence.IsOnline = true;
                presence.LastHeartbeatAt = now;
                presence.LastSeenAt = now;
                presence.LastConnectionId = normalizedConnectionId ?? presence.LastConnectionId;
                presence.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return presence;
        }

        public async Task<UserPresence> MarkUserOfflineAsync(int userId, string? connectionId = null)
        {
            var now = DateTime.UtcNow;
            var normalizedConnectionId = NormalizeConnectionId(connectionId);

            var presence = await _context.UserPresences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (presence == null)
            {
                presence = new UserPresence
                {
                    UserId = userId,
                    IsOnline = false,
                    ActiveConnections = 0,
                    LastHeartbeatAt = now,
                    LastSeenAt = now,
                    LastConnectionId = normalizedConnectionId,
                    UpdatedAt = now
                };
                _context.UserPresences.Add(presence);
            }
            else
            {
                presence.ActiveConnections = Math.Max(0, presence.ActiveConnections - 1);
                presence.IsOnline = presence.ActiveConnections > 0;
                presence.LastSeenAt = now;
                presence.LastConnectionId = normalizedConnectionId ?? presence.LastConnectionId;
                presence.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return presence;
        }

        public async Task<UserPresence> TouchHeartbeatAsync(int userId, string? connectionId = null)
        {
            var now = DateTime.UtcNow;
            var normalizedConnectionId = NormalizeConnectionId(connectionId);

            var presence = await _context.UserPresences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (presence == null)
            {
                presence = new UserPresence
                {
                    UserId = userId,
                    IsOnline = true,
                    ActiveConnections = 1,
                    LastHeartbeatAt = now,
                    LastSeenAt = now,
                    LastConnectionId = normalizedConnectionId,
                    UpdatedAt = now
                };
                _context.UserPresences.Add(presence);
            }
            else
            {
                if (presence.ActiveConnections == 0)
                {
                    presence.ActiveConnections = 1;
                }

                presence.IsOnline = true;
                presence.LastHeartbeatAt = now;
                presence.LastSeenAt = now;
                presence.LastConnectionId = normalizedConnectionId ?? presence.LastConnectionId;
                presence.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return presence;
        }

        private static string? NormalizeConnectionId(string? connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return null;
            }

            var trimmed = connectionId.Trim();
            return trimmed.Length <= 200 ? trimmed : trimmed[..200];
        }
    }
}
