using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjetoEventX.Models;

namespace ProjetoEventX.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly EventXContext _context;

        public ChatHub(EventXContext context)
        {
            _context = context;
        }

        // Enviar mensagem do Organizador para Convidado ou Fornecedor
        public async Task SendMessage(int remetenteId, int destinatarioId, string tipoDestinatario, string conteudo, int eventoId)
        {
            var remetente = await _context.Pessoas.FindAsync(remetenteId);
            var destinatario = await _context.Pessoas.FindAsync(destinatarioId);
            var evento = await _context.Eventos.FindAsync(eventoId);

            if (remetente == null || destinatario == null || evento == null)
            {
                throw new HubException("Remetente, destinatário ou evento inválido.");
            }

            var mensagem = new MensagemChat
            {
                RemetenteId = remetenteId,
                DestinatarioId = destinatarioId,
                TipoDestinatario = tipoDestinatario, // "Convidado" ou "Fornecedor"
                Conteudo = conteudo,
                EventoId = eventoId,
                DataEnvio = DateTime.Now,
                EhRespostaAssistente = false,
                Remetente = remetente,
                Destinatario = destinatario,
                Evento = evento
            };

            _context.MensagemChats.Add(mensagem);
            await _context.SaveChangesAsync();

            // Enviar mensagem para o grupo do destinatário
            await Clients.Group($"User_{destinatarioId}").SendAsync("ReceiveMessage", remetenteId, conteudo, eventoId);
        }

        // Enviar mensagem para o Assistente Virtual
        public async Task SendToAssistant(int remetenteId, string conteudo, int eventoId)
        {
            var remetente = await _context.Pessoas.FindAsync(remetenteId);
            var evento = await _context.Eventos.FindAsync(eventoId);

            if (remetente == null || evento == null)
            {
                throw new HubException("Remetente ou evento inválido.");
            }

            // Chama o Assistente Virtual
            var respostaAssistente = CallAssistantVirtual(remetenteId, conteudo, eventoId);

            var mensagem = new MensagemChat
            {
                RemetenteId = remetenteId,
                DestinatarioId = remetenteId, // Mensagem do assistente é enviada de volta ao remetente
                TipoDestinatario = "Assistente",
                Conteudo = respostaAssistente,
                EventoId = eventoId,
                DataEnvio = DateTime.Now,
                EhRespostaAssistente = true,
                Remetente = remetente,
                Destinatario = remetente,
                Evento = evento
            };

            _context.MensagemChats.Add(mensagem);
            await _context.SaveChangesAsync();

            // Enviar a resposta do assistente para o organizador
            await Clients.Group($"User_{remetenteId}").SendAsync("ReceiveMessage", 0, respostaAssistente, eventoId);
        }

        // Mapear o ConnectionId ao usuário usando grupos
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Método para chamar o Assistente Virtual (síncrono até integração real)
        private string CallAssistantVirtual(int remetenteId, string conteudo, int eventoId)
        {
            // Buscar o Assistente Virtual no banco
            var assistente = _context.AssistentesVirtuais.FirstOrDefault();
            if (assistente == null)
            {
                return "Assistente virtual não configurado.";
            }

            // Exemplo: Substitua pela lógica real do seu AssistenteVirtual.cs
            return $"Resposta do Assistente Virtual: {conteudo} (Evento: {eventoId})";
        }
    }
}