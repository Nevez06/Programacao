using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Models;
using ProjetoEventX.Hubs;

namespace ProjetoEventX.Controllers
{
    public class ChatController : Controller
    {
        private readonly EventXContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(EventXContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Chat(int eventoId)
        {
            var evento = await _context.Eventos.FindAsync(eventoId);
            if (evento == null)
            {
                return NotFound();
            }

            var mensagens = await _context.MensagensChat
                .Include(m => m.Convidado)
                .ThenInclude(c => c.Pessoa)
                .Where(m => m.EventoId == eventoId)
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();

            ViewBag.EventoId = eventoId;
            ViewBag.NomeEvento = evento.NomeEvento;
            ViewBag.CurrentUserId = 0; // Será atualizado quando houver autenticação de convidado
            return View(mensagens);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensagem(int eventoId, int convidadoId, string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
            {
                return BadRequest();
            }

            var mensagemChat = new MensagemChat
            {
                EventoId = eventoId,
                ConvidadoId = convidadoId,
                Mensagem = mensagem,
                DataEnvio = DateTime.Now
            };

            _context.MensagensChat.Add(mensagemChat);
            await _context.SaveChangesAsync();

            var convidado = await _context.Convidados
                .Include(c => c.Pessoa)
                .FirstOrDefaultAsync(c => c.Id == convidadoId);

            var nomeUsuario = convidado?.Pessoa?.Nome ?? "Anônimo";

            await _hubContext.Clients.Group($"Evento_{eventoId}").SendAsync("ReceiveMessage", 
                convidadoId, mensagem, nomeUsuario, DateTime.Now);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ListarConversas()
        {
            var eventos = await _context.Eventos
                .Select(e => new
                {
                    e.Id,
                    e.NomeEvento,
                    MensagensNaoLidas = _context.MensagensChat.Count(m => m.EventoId == e.Id)
                })
                .ToListAsync();

            return View(eventos);
        }
    }
}

