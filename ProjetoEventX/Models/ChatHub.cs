using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjetoEventX.Data;
using ProjetoEventX.Services;

namespace ProjetoEventX.Models
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly EventXContext _context;
        private readonly ChatService _chatService;

        public ChatHub(EventXContext context, ChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        // Fluxo legado para não quebrar integração existente.
        public async Task SendMessage(int remetenteId, int destinatarioId, string tipoDestinatario, string conteudo, int eventoId)
        {
            var authenticatedUserId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (!authenticatedUserId.HasValue || authenticatedUserId.Value != remetenteId)
            {
                throw new HubException("Usuario autenticado nao corresponde ao remetente.");
            }

            if (string.IsNullOrWhiteSpace(conteudo))
            {
                throw new HubException("Mensagem vazia.");
            }

            var remetente = await _context.Pessoas.FindAsync(remetenteId);
            var destinatario = await _context.Pessoas.FindAsync(destinatarioId);
            var evento = await _context.Eventos.FindAsync(eventoId);

            if (remetente == null || destinatario == null || evento == null)
            {
                throw new HubException("Remetente, destinatario ou evento invalido.");
            }

            var conteudoLimpo = conteudo.Trim();
            var mensagem = new MensagemChat
            {
                RemetenteId = remetenteId,
                DestinatarioId = destinatarioId,
                TipoDestinatario = tipoDestinatario,
                Conteudo = conteudoLimpo,
                EventoId = eventoId,
                DataEnvio = DateTime.UtcNow,
                EhRespostaAssistente = false,
                Remetente = remetente,
                Destinatario = destinatario,
                Evento = evento
            };

            _context.MensagemChats.Add(mensagem);
            await _context.SaveChangesAsync();

            await Clients.Group(ChatService.GetLegacyUserGroupName(destinatarioId))
                .SendAsync("ReceiveMessage", remetenteId, conteudoLimpo, eventoId);
        }

        // Fluxo legado para não quebrar integração existente.
        public async Task SendToAssistant(int remetenteId, string conteudo, int eventoId)
        {
            var authenticatedUserId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (!authenticatedUserId.HasValue || authenticatedUserId.Value != remetenteId)
            {
                throw new HubException("Usuario autenticado nao corresponde ao remetente.");
            }

            if (string.IsNullOrWhiteSpace(conteudo))
            {
                throw new HubException("Mensagem vazia.");
            }

            var remetente = await _context.Pessoas.FindAsync(remetenteId);
            var evento = await _context.Eventos.FindAsync(eventoId);

            if (remetente == null || evento == null)
            {
                throw new HubException("Remetente ou evento invalido.");
            }

            var respostaAssistente = CallAssistantVirtual(conteudo.Trim(), eventoId);

            var mensagem = new MensagemChat
            {
                RemetenteId = remetenteId,
                DestinatarioId = remetenteId,
                TipoDestinatario = "Assistente",
                Conteudo = respostaAssistente,
                EventoId = eventoId,
                DataEnvio = DateTime.UtcNow,
                EhRespostaAssistente = true,
                Remetente = remetente,
                Destinatario = remetente,
                Evento = evento
            };

            _context.MensagemChats.Add(mensagem);
            await _context.SaveChangesAsync();

            await Clients.Group(ChatService.GetLegacyUserGroupName(remetenteId))
                .SendAsync("ReceiveMessage", 0, respostaAssistente, eventoId);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = ChatService.GetUserIdFromClaims(Context.User ?? new System.Security.Claims.ClaimsPrincipal());
            if (userId.HasValue)
            {
                await _chatService.MarkUserOnlineAsync(userId.Value, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, ChatService.GetLegacyUserGroupName(userId.Value));
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
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatService.GetLegacyUserGroupName(userId.Value));
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

            var groupName = ChatService.GetConversationGroupName(conversationId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

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

        private string CallAssistantVirtual(string conteudo, int eventoId)
        {
            var assistente = _context.AssistentesVirtuais.FirstOrDefault();
            if (assistente == null)
            {
                return "Assistente virtual nao configurado.";
            }

            return $"Resposta do Assistente Virtual: {conteudo} (Evento: {eventoId})";
        }
    }
}
