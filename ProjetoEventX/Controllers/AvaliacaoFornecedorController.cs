using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class AvaliacaoFornecedorController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AvaliacaoFornecedorController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AvaliacaoFornecedor/Criar?fornecedorId=1&eventoId=2
        public async Task<IActionResult> Criar(int fornecedorId, int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .FirstOrDefaultAsync(f => f.Id == fornecedorId);

            if (fornecedor == null)
                return NotFound();

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);

            if (evento == null)
            {
                TempData["ErrorMessage"] = "Evento não encontrado ou sem permissão.";
                return RedirectToAction("Index", "Eventos");
            }

            // Verificar se já avaliou
            var avaliacaoExistente = await _context.AvaliacoesFornecedores
                .AnyAsync(a => a.FornecedorId == fornecedorId && a.OrganizadorId == user.Id && a.EventoId == eventoId);

            if (avaliacaoExistente)
            {
                TempData["InfoMessage"] = "Você já avaliou este fornecedor para este evento.";
                return RedirectToAction("Avaliacoes", new { fornecedorId });
            }

            ViewBag.Fornecedor = fornecedor;
            ViewBag.Evento = evento;

            return View();
        }

        // POST: AvaliacaoFornecedor/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(int fornecedorId, int eventoId, int nota, string? comentario)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            if (nota < 1 || nota > 5)
            {
                TempData["ErrorMessage"] = "A nota deve ser entre 1 e 5.";
                return RedirectToAction("Criar", new { fornecedorId, eventoId });
            }

            var fornecedor = await _context.Fornecedores
                .FirstOrDefaultAsync(f => f.Id == fornecedorId);

            if (fornecedor == null)
                return NotFound();

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);

            if (evento == null)
                return NotFound();

            // Verificar duplicata
            var avaliacaoExistente = await _context.AvaliacoesFornecedores
                .AnyAsync(a => a.FornecedorId == fornecedorId && a.OrganizadorId == user.Id && a.EventoId == eventoId);

            if (avaliacaoExistente)
            {
                TempData["InfoMessage"] = "Você já avaliou este fornecedor para este evento.";
                return RedirectToAction("Avaliacoes", new { fornecedorId });
            }

            var avaliacao = new AvaliacaoFornecedor
            {
                Nota = nota,
                Comentario = comentario?.Trim(),
                FornecedorId = fornecedorId,
                OrganizadorId = user.Id,
                EventoId = eventoId,
                DataAvaliacao = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AvaliacoesFornecedores.Add(avaliacao);
            await _context.SaveChangesAsync();

            // Recalcular média
            await RecalcularMedia(fornecedorId);

            TempData["SuccessMessage"] = "Avaliação enviada com sucesso!";
            return RedirectToAction("Avaliacoes", new { fornecedorId });
        }

        // GET: AvaliacaoFornecedor/Avaliacoes?fornecedorId=1
        public async Task<IActionResult> Avaliacoes(int fornecedorId)
        {
            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .Include(f => f.Avaliacoes)
                .FirstOrDefaultAsync(f => f.Id == fornecedorId);

            if (fornecedor == null)
                return NotFound();

            var avaliacoes = await _context.AvaliacoesFornecedores
                .Include(a => a.Organizador)
                    .ThenInclude(o => o!.Pessoa)
                .Include(a => a.Evento)
                .Where(a => a.FornecedorId == fornecedorId)
                .OrderByDescending(a => a.DataAvaliacao)
                .ToListAsync();

            ViewBag.Fornecedor = fornecedor;

            // Se o usuário é organizador, buscar eventos dele que usaram este fornecedor e ainda não avaliaram
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.TipoUsuario == "Organizador")
            {
                var eventosComPedido = await _context.Pedidos
                    .Include(p => p.Produto)
                    .Include(p => p.Evento)
                    .Where(p => p.Produto != null && p.Produto.FornecedorId == fornecedorId
                             && p.Evento != null && p.Evento.OrganizadorId == user.Id)
                    .Select(p => p.Evento!)
                    .Distinct()
                    .ToListAsync();

                var eventosJaAvaliados = await _context.AvaliacoesFornecedores
                    .Where(a => a.FornecedorId == fornecedorId && a.OrganizadorId == user.Id)
                    .Select(a => a.EventoId)
                    .ToListAsync();

                ViewBag.EventosPendentes = eventosComPedido
                    .Where(e => !eventosJaAvaliados.Contains(e.Id))
                    .ToList();
            }

            return View(avaliacoes);
        }

        // GET: AvaliacaoFornecedor/MeusEventosParaAvaliar
        public async Task<IActionResult> MeusEventosParaAvaliar()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            // Buscar fornecedores com pedidos em eventos do organizador
            var fornecedoresComPedido = await _context.Pedidos
                .Include(p => p.Produto!)
                    .ThenInclude(pr => pr.Fornecedor)
                        .ThenInclude(f => f.Pessoa)
                .Include(p => p.Evento)
                .Where(p => p.Evento != null && p.Evento.OrganizadorId == user.Id
                         && p.Produto != null && p.Produto.Fornecedor != null)
                .Select(p => new {
                    Fornecedor = p.Produto!.Fornecedor,
                    Evento = p.Evento!
                })
                .Distinct()
                .ToListAsync();

            // Buscar avaliações já feitas
            var avaliacoesFeitas = await _context.AvaliacoesFornecedores
                .Where(a => a.OrganizadorId == user.Id)
                .Select(a => new { a.FornecedorId, a.EventoId })
                .ToListAsync();

            var pendentes = fornecedoresComPedido
                .Where(x => !avaliacoesFeitas.Any(a => a.FornecedorId == x.Fornecedor.Id && a.EventoId == x.Evento.Id))
                .GroupBy(x => x.Fornecedor.Id)
                .Select(g => new {
                    Fornecedor = g.First().Fornecedor,
                    Eventos = g.Select(x => x.Evento).Distinct().ToList()
                })
                .ToList();

            ViewBag.Pendentes = pendentes;

            return View();
        }

        private async Task RecalcularMedia(int fornecedorId)
        {
            var media = await _context.AvaliacoesFornecedores
                .Where(a => a.FornecedorId == fornecedorId)
                .AverageAsync(a => (decimal?)a.Nota) ?? 0;

            var fornecedor = await _context.Fornecedores.FindAsync(fornecedorId);
            if (fornecedor != null)
            {
                fornecedor.AvaliacaoMedia = Math.Round(media, 1);
                fornecedor.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
