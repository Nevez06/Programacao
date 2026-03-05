using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class CheckInController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckInController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: CheckIn/Index?eventoId=1
        public async Task<IActionResult> Index(int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);
            if (evento == null)
                return NotFound();

            var convidados = await _context.ListasConvidados
                .Include(l => l.Convidado)
                .ThenInclude(c => c!.Pessoa)
                .Where(l => l.EventoId == eventoId)
                .OrderByDescending(l => l.CheckInRealizado)
                .ThenByDescending(l => l.DataCheckIn)
                .ThenBy(l => l.Convidado!.Pessoa!.Nome)
                .ToListAsync();

            // Gerar CodigoQR para convites que ainda não possuem
            foreach (var c in convidados.Where(c => string.IsNullOrEmpty(c.CodigoQR)))
            {
                c.CodigoQR = $"EVTX-{eventoId}-{c.ConvidadoId}-{Guid.NewGuid().ToString("N")[..8]}";
            }
            await _context.SaveChangesAsync();

            var totalConvidados = convidados.Count;
            var presentes = convidados.Count(c => c.CheckInRealizado);

            ViewBag.Evento = evento;
            ViewBag.Convidados = convidados;
            ViewBag.TotalConvidados = totalConvidados;
            ViewBag.Presentes = presentes;

            return View();
        }

        // POST: CheckIn/Validar (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Validar([FromBody] CheckInRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return Json(new { sucesso = false, mensagem = "Acesso negado." });

            if (string.IsNullOrWhiteSpace(request?.CodigoQR))
                return Json(new { sucesso = false, mensagem = "QR Code inválido." });

            var listaConvidado = await _context.ListasConvidados
                .Include(l => l.Convidado)
                .ThenInclude(c => c!.Pessoa)
                .Include(l => l.Evento)
                .FirstOrDefaultAsync(l => l.CodigoQR == request.CodigoQR && l.EventoId == request.EventoId);

            if (listaConvidado == null)
                return Json(new { sucesso = false, mensagem = "Convite não encontrado para este evento." });

            if (listaConvidado.CheckInRealizado)
            {
                return Json(new {
                    sucesso = false,
                    mensagem = $"Check-in já realizado por {listaConvidado.Convidado?.Pessoa?.Nome} às {listaConvidado.DataCheckIn?.ToString("HH:mm")}."
                });
            }

            listaConvidado.CheckInRealizado = true;
            listaConvidado.DataCheckIn = DateTime.UtcNow;
            listaConvidado.UpdatedAt = DateTime.UtcNow;

            _context.Update(listaConvidado);
            await _context.SaveChangesAsync();

            return Json(new {
                sucesso = true,
                mensagem = "Check-in realizado!",
                nome = listaConvidado.Convidado?.Pessoa?.Nome ?? "Convidado",
                hora = listaConvidado.DataCheckIn?.ToString("HH:mm")
            });
        }

        // POST: CheckIn/DesfazerCheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesfazerCheckIn(int id, int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var listaConvidado = await _context.ListasConvidados
                .FirstOrDefaultAsync(l => l.Id == id && l.EventoId == eventoId);

            if (listaConvidado != null)
            {
                listaConvidado.CheckInRealizado = false;
                listaConvidado.DataCheckIn = null;
                listaConvidado.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Check-in desfeito.";
            }

            return RedirectToAction("Index", new { eventoId });
        }

        // GET: CheckIn/QRCode?eventoId=1&convidadoId=2
        public async Task<IActionResult> QRCode(int eventoId, int convidadoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var listaConvidado = await _context.ListasConvidados
                .Include(l => l.Convidado)
                .ThenInclude(c => c!.Pessoa)
                .Include(l => l.Evento)
                .FirstOrDefaultAsync(l => l.EventoId == eventoId && l.ConvidadoId == convidadoId);

            if (listaConvidado == null)
                return NotFound();

            // Gerar código QR se não existir
            if (string.IsNullOrEmpty(listaConvidado.CodigoQR))
            {
                listaConvidado.CodigoQR = $"EVTX-{eventoId}-{convidadoId}-{Guid.NewGuid().ToString("N")[..8]}";
                await _context.SaveChangesAsync();
            }

            ViewBag.ListaConvidado = listaConvidado;
            ViewBag.Evento = listaConvidado.Evento;
            ViewBag.NomeConvidado = listaConvidado.Convidado?.Pessoa?.Nome ?? "Convidado";

            return View();
        }
    }

    public class CheckInRequest
    {
        public string? CodigoQR { get; set; }
        public int EventoId { get; set; }
    }
}
