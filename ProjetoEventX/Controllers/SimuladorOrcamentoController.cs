using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class SimuladorOrcamentoController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SimuladorOrcamentoController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: SimuladorOrcamento/Index?eventoId=1
        public async Task<IActionResult> Index(int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);
            if (evento == null)
                return NotFound();

            var simulacoes = await _context.OrcamentosSimulados
                .Where(o => o.EventoId == eventoId)
                .OrderBy(o => o.Categoria)
                .ToListAsync();

            var totalEstimado = simulacoes.Sum(s => s.ValorEstimado);

            ViewBag.Evento = evento;
            ViewBag.Simulacoes = simulacoes;
            ViewBag.TotalEstimado = totalEstimado;

            return View();
        }

        // POST: SimuladorOrcamento/Simular
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Simular(int eventoId, int quantidadeConvidados, string tipoEvento, string regiao)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);
            if (evento == null)
                return NotFound();

            if (quantidadeConvidados < 1)
            {
                TempData["Erro"] = "Informe a quantidade de convidados.";
                return RedirectToAction("Index", new { eventoId });
            }

            // Remover simulações anteriores do evento
            var antigas = await _context.OrcamentosSimulados
                .Where(o => o.EventoId == eventoId)
                .ToListAsync();
            _context.OrcamentosSimulados.RemoveRange(antigas);

            // Calcular estimativas com base nos parâmetros
            var estimativas = CalcularEstimativas(quantidadeConvidados, tipoEvento, regiao);

            var simulacoes = estimativas.Select(e => new OrcamentoSimulado
            {
                EventoId = eventoId,
                Categoria = e.Categoria,
                ValorEstimado = e.Valor,
                DataSimulacao = DateTime.UtcNow
            }).ToList();

            _context.OrcamentosSimulados.AddRange(simulacoes);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Simulação gerada para {quantidadeConvidados} convidados!";
            return RedirectToAction("Index", new { eventoId });
        }

        // POST: SimuladorOrcamento/Limpar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Limpar(int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var simulacoes = await _context.OrcamentosSimulados
                .Where(o => o.EventoId == eventoId)
                .ToListAsync();
            _context.OrcamentosSimulados.RemoveRange(simulacoes);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Simulação limpa com sucesso!";
            return RedirectToAction("Index", new { eventoId });
        }

        private List<(string Categoria, decimal Valor)> CalcularEstimativas(int convidados, string tipoEvento, string regiao)
        {
            // Multiplicadores por tipo de evento
            decimal multiplicadorTipo = tipoEvento?.ToLower() switch
            {
                "casamento" => 1.5m,
                "corporativo" => 1.3m,
                "formatura" => 1.2m,
                "aniversário" or "aniversario" => 1.0m,
                "festa" => 0.9m,
                "conferência" or "conferencia" => 1.4m,
                _ => 1.0m,
            };

            // Multiplicadores por região
            decimal multiplicadorRegiao = regiao?.ToLower() switch
            {
                "sul" => 1.0m,
                "sudeste" => 1.2m,
                "centro-oeste" => 1.0m,
                "nordeste" => 0.85m,
                "norte" => 0.9m,
                _ => 1.0m,
            };

            decimal mult = multiplicadorTipo * multiplicadorRegiao;

            // Preços base por convidado
            var estimativas = new List<(string Categoria, decimal Valor)>
            {
                ("Buffet", Math.Round(convidados * 40m * mult, 2)),
                ("Decoração", Math.Round(convidados * 12m * mult, 2)),
                ("Som / DJ", Math.Round(900m * mult, 2)),
                ("Fotografia", Math.Round(1500m * mult, 2)),
                ("Local do evento", Math.Round(convidados * 15m * mult, 2)),
                ("Bebidas", Math.Round(convidados * 25m * mult, 2)),
            };

            return estimativas;
        }
    }
}
