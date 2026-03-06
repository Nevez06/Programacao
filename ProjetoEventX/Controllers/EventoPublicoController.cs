using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;

namespace ProjetoEventX.Controllers
{
    public class EventoPublicoController : Controller
    {
        private readonly EventXContext _context;

        public EventoPublicoController(EventXContext context)
        {
            _context = context;
        }

        // GET: /evento/{slug}
        [HttpGet("/evento/{slug}")]
        public async Task<IActionResult> Detalhes(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var evento = await _context.Eventos
                .Include(e => e.Local)
                .Include(e => e.Organizador)
                    .ThenInclude(o => o!.Pessoa)
                .Include(e => e.ListasConvidados)
                .FirstOrDefaultAsync(e => e.Slug == slug);

            if (evento == null)
                return NotFound();

            return View("Detalhes", evento);
        }

        // POST: /evento/{slug}/confirmar
        [HttpPost("/evento/{slug}/confirmar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPresenca(string slug, string nome, string email)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var evento = await _context.Eventos
                .Include(e => e.ListasConvidados)
                .FirstOrDefaultAsync(e => e.Slug == slug);

            if (evento == null)
                return NotFound();

            // Verificar se já existe um convidado com este email para este evento
            var convidadoExistente = await _context.Convidados
                .FirstOrDefaultAsync(c => c.Email == email);

            if (convidadoExistente != null)
            {
                var jaInscrito = await _context.ListasConvidados
                    .AnyAsync(lc => lc.ConvidadoId == convidadoExistente.Id && lc.EventoId == evento.Id);

                if (jaInscrito)
                {
                    TempData["InfoMessage"] = "Você já confirmou presença neste evento!";
                    return Redirect($"/evento/{slug}");
                }
            }

            TempData["SuccessMessage"] = $"Presença confirmada com sucesso para \"{evento.NomeEvento}\"! Entraremos em contato pelo email informado.";
            return Redirect($"/evento/{slug}");
        }
    }
}
