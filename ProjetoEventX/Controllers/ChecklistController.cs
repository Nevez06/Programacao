using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class ChecklistController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChecklistController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Checklist/Index?eventoId=1
        public async Task<IActionResult> Index(int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);
            if (evento == null)
                return NotFound();

            var checklist = await _context.ChecklistEventos
                .Where(c => c.EventoId == eventoId)
                .OrderBy(c => c.Ordem)
                .ThenBy(c => c.DataCriacao)
                .ToListAsync();

            var total = checklist.Count;
            var concluidos = checklist.Count(c => c.Concluido);
            var progresso = total > 0 ? (int)Math.Round((double)concluidos / total * 100) : 0;

            ViewBag.Evento = evento;
            ViewBag.Checklist = checklist;
            ViewBag.Total = total;
            ViewBag.Concluidos = concluidos;
            ViewBag.Progresso = progresso;

            return View();
        }

        // POST: Checklist/Adicionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(int eventoId, string titulo, string? descricao, string? categoria)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);
            if (evento == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(titulo))
            {
                TempData["Erro"] = "O título da tarefa é obrigatório.";
                return RedirectToAction("Index", new { eventoId });
            }

            var maxOrdem = await _context.ChecklistEventos
                .Where(c => c.EventoId == eventoId)
                .MaxAsync(c => (int?)c.Ordem) ?? 0;

            var item = new ChecklistEvento
            {
                EventoId = eventoId,
                Titulo = titulo.Trim(),
                Descricao = descricao?.Trim(),
                Categoria = categoria?.Trim(),
                Concluido = false,
                DataCriacao = DateTime.UtcNow,
                Ordem = maxOrdem + 1
            };

            _context.ChecklistEventos.Add(item);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Tarefa \"{titulo}\" adicionada ao checklist!";
            return RedirectToAction("Index", new { eventoId });
        }

        // POST: Checklist/AlternarStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatus(int id, int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var item = await _context.ChecklistEventos.FindAsync(id);
            if (item == null || item.EventoId != eventoId)
                return NotFound();

            item.Concluido = !item.Concluido;
            item.DataConclusao = item.Concluido ? DateTime.UtcNow : null;
            await _context.SaveChangesAsync();

            var status = item.Concluido ? "concluída" : "reaberta";
            TempData["Sucesso"] = $"Tarefa \"{item.Titulo}\" {status}!";
            return RedirectToAction("Index", new { eventoId });
        }

        // POST: Checklist/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, int eventoId, string titulo, string? descricao, string? categoria)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var item = await _context.ChecklistEventos.FindAsync(id);
            if (item == null || item.EventoId != eventoId)
                return NotFound();

            if (string.IsNullOrWhiteSpace(titulo))
            {
                TempData["Erro"] = "O título da tarefa é obrigatório.";
                return RedirectToAction("Index", new { eventoId });
            }

            item.Titulo = titulo.Trim();
            item.Descricao = descricao?.Trim();
            item.Categoria = categoria?.Trim();
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Tarefa atualizada com sucesso!";
            return RedirectToAction("Index", new { eventoId });
        }

        // POST: Checklist/Excluir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id, int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var item = await _context.ChecklistEventos.FindAsync(id);
            if (item == null || item.EventoId != eventoId)
                return NotFound();

            _context.ChecklistEventos.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Tarefa excluída com sucesso!";
            return RedirectToAction("Index", new { eventoId });
        }
    }
}
