using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class TimelineController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimelineController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Timeline/Index?eventoId=1
        public async Task<IActionResult> Index(int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);
            if (evento == null)
                return NotFound();

            var timeline = await _context.TimelineEventos
                .Where(t => t.EventoId == eventoId)
                .OrderByDescending(t => t.DiasAntesEvento)
                .ThenBy(t => t.Ordem)
                .ToListAsync();

            // Se não há itens, gerar timeline sugerida
            if (!timeline.Any())
            {
                timeline = GerarTimelineSugerida(eventoId);
                _context.TimelineEventos.AddRange(timeline);
                await _context.SaveChangesAsync();
                timeline = timeline.OrderByDescending(t => t.DiasAntesEvento).ThenBy(t => t.Ordem).ToList();
            }

            var diasParaEvento = (evento.DataEvento - DateTime.UtcNow).Days;

            ViewBag.Evento = evento;
            ViewBag.Timeline = timeline;
            ViewBag.DiasParaEvento = diasParaEvento;
            ViewBag.Total = timeline.Count;
            ViewBag.Concluidos = timeline.Count(t => t.Concluido);

            return View();
        }

        // POST: Timeline/AlternarStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatus(int id, int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var item = await _context.TimelineEventos.FindAsync(id);
            if (item == null || item.EventoId != eventoId)
                return NotFound();

            item.Concluido = !item.Concluido;
            await _context.SaveChangesAsync();

            var status = item.Concluido ? "concluída" : "reaberta";
            TempData["Sucesso"] = $"Etapa \"{item.Titulo}\" {status}!";
            return RedirectToAction("Index", new { eventoId });
        }

        // POST: Timeline/Adicionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(int eventoId, string titulo, int diasAntesEvento, string? categoria)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            if (string.IsNullOrWhiteSpace(titulo))
            {
                TempData["Erro"] = "O título da etapa é obrigatório.";
                return RedirectToAction("Index", new { eventoId });
            }

            var item = new TimelineEvento
            {
                EventoId = eventoId,
                Titulo = titulo.Trim(),
                DiasAntesEvento = diasAntesEvento,
                Categoria = categoria?.Trim(),
                Concluido = false,
                Ordem = 0
            };

            _context.TimelineEventos.Add(item);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Etapa \"{titulo}\" adicionada ao cronograma!";
            return RedirectToAction("Index", new { eventoId });
        }

        // POST: Timeline/Excluir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id, int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var item = await _context.TimelineEventos.FindAsync(id);
            if (item == null || item.EventoId != eventoId)
                return NotFound();

            _context.TimelineEventos.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Etapa excluída do cronograma!";
            return RedirectToAction("Index", new { eventoId });
        }

        private List<TimelineEvento> GerarTimelineSugerida(int eventoId)
        {
            return new List<TimelineEvento>
            {
                new() { EventoId = eventoId, Titulo = "Definir local do evento", DiasAntesEvento = 90, Categoria = "Planejamento", Ordem = 1 },
                new() { EventoId = eventoId, Titulo = "Definir orçamento geral", DiasAntesEvento = 90, Categoria = "Financeiro", Ordem = 2 },
                new() { EventoId = eventoId, Titulo = "Contratar buffet", DiasAntesEvento = 60, Categoria = "Fornecedores", Ordem = 1 },
                new() { EventoId = eventoId, Titulo = "Escolher decoração", DiasAntesEvento = 60, Categoria = "Fornecedores", Ordem = 2 },
                new() { EventoId = eventoId, Titulo = "Contratar som / DJ", DiasAntesEvento = 45, Categoria = "Fornecedores", Ordem = 1 },
                new() { EventoId = eventoId, Titulo = "Enviar convites", DiasAntesEvento = 30, Categoria = "Comunicação", Ordem = 1 },
                new() { EventoId = eventoId, Titulo = "Contratar fotógrafo", DiasAntesEvento = 15, Categoria = "Fornecedores", Ordem = 1 },
                new() { EventoId = eventoId, Titulo = "Confirmar fornecedores", DiasAntesEvento = 7, Categoria = "Fornecedores", Ordem = 1 },
                new() { EventoId = eventoId, Titulo = "Confirmar lista de convidados", DiasAntesEvento = 7, Categoria = "Comunicação", Ordem = 2 },
                new() { EventoId = eventoId, Titulo = "Preparar local", DiasAntesEvento = 1, Categoria = "Logística", Ordem = 1 },
                new() { EventoId = eventoId, Titulo = "Revisão final", DiasAntesEvento = 1, Categoria = "Planejamento", Ordem = 2 },
            };
        }
    }
}
