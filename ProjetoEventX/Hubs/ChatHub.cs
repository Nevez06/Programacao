using Microsoft.AspNetCore.SignalR;

namespace ProjetoEventX.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int eventoId, int convidadoId, string mensagem, string nomeUsuario)
        {
            await Clients.Group($"Evento_{eventoId}").SendAsync("ReceiveMessage", convidadoId, mensagem, nomeUsuario, DateTime.Now);
        }

        public async Task JoinEventGroup(int eventoId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Evento_{eventoId}");
        }

        public async Task LeaveEventGroup(int eventoId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Evento_{eventoId}");
        }
    }
}



