using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class AdministracaoController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdministracaoController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            var evento = await _context.Eventos
                .Include(e => e.Pedidos)
                .Include(e => e.Despesas)
                .FirstOrDefaultAsync(e => e.Id == eventoId);

            if (evento == null)
            {
                return NotFound();
            }

            var despesas = await _context.Despesas
                .Where(d => d.EventoId == eventoId)
                .OrderByDescending(d => d.DataDespesa)
                .ToListAsync();

            var totalGasto = despesas.Sum(d => d.Valor);
            var totalPedidos = evento.Pedidos.Sum(p => p.PrecoTotal);
            var totalGeral = totalGasto + totalPedidos;

            var administracao = await _context.Administracoes
                .FirstOrDefaultAsync(a => a.IdEvento == eventoId);

            ViewBag.Evento = evento;
            ViewBag.Despesas = despesas;
            ViewBag.TotalGasto = totalGasto;
            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.TotalGeral = totalGeral;
            ViewBag.Administracao = administracao;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarDespesa(int eventoId, string Descricao, string Valor)
        {
            // Converter valor (aceitar vírgula ou ponto)
            var valorLimpo = Valor.Replace(",", ".");
            if (!decimal.TryParse(valorLimpo, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out var valorDecimal) || valorDecimal <= 0)
            {
                TempData["Erro"] = "Valor inválido. Informe um valor maior que zero.";
                return RedirectToAction("Index", new { eventoId });
            }

            if (string.IsNullOrWhiteSpace(Descricao))
            {
                TempData["Erro"] = "Descrição é obrigatória.";
                return RedirectToAction("Index", new { eventoId });
            }

            var evento = await _context.Eventos.FindAsync(eventoId);
            if (evento == null)
                return NotFound();

            var despesa = new Despesa
            {
                EventoId = eventoId,
                Evento = evento,
                Descricao = Descricao,
                Valor = valorDecimal,
                DataDespesa = DateTime.Now
            };

            _context.Despesas.Add(despesa);
            await _context.SaveChangesAsync();

            // Atualizar ou criar administração
            var administracao = await _context.Administracoes
                .FirstOrDefaultAsync(a => a.IdEvento == eventoId);

            if (administracao == null)
            {
                administracao = new Administracao
                {
                    IdEvento = eventoId,
                    Orcamento = 0,
                    ValorTotal = 0
                };
                _context.Administracoes.Add(administracao);
            }

            administracao.ValorTotal = await _context.Despesas
                .Where(d => d.EventoId == eventoId)
                .SumAsync(d => d.Valor);

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Despesa \"{Descricao}\" de R$ {valorDecimal:N2} adicionada com sucesso!";
            return RedirectToAction("Index", new { eventoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarDespesa(int despesaId, int eventoId, string Descricao, string Valor)
        {
            var valorLimpo = Valor.Replace(",", ".");
            if (!decimal.TryParse(valorLimpo, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var valorDecimal) || valorDecimal <= 0)
            {
                TempData["Erro"] = "Valor inválido. Informe um valor maior que zero.";
                return RedirectToAction("Index", new { eventoId });
            }

            var despesa = await _context.Despesas.FindAsync(despesaId);
            if (despesa == null)
                return NotFound();

            despesa.Descricao = Descricao;
            despesa.Valor = valorDecimal;
            await _context.SaveChangesAsync();

            // Atualizar administração
            var administracao = await _context.Administracoes
                .FirstOrDefaultAsync(a => a.IdEvento == eventoId);
            if (administracao != null)
            {
                administracao.ValorTotal = await _context.Despesas
                    .Where(d => d.EventoId == eventoId)
                    .SumAsync(d => d.Valor);
                await _context.SaveChangesAsync();
            }

            TempData["Sucesso"] = $"Despesa atualizada com sucesso!";
            return RedirectToAction("Index", new { eventoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirDespesa(int despesaId, int eventoId)
        {
            var despesa = await _context.Despesas.FindAsync(despesaId);
            if (despesa == null)
                return NotFound();

            _context.Despesas.Remove(despesa);
            await _context.SaveChangesAsync();

            // Atualizar administração
            var administracao = await _context.Administracoes
                .FirstOrDefaultAsync(a => a.IdEvento == eventoId);
            if (administracao != null)
            {
                administracao.ValorTotal = await _context.Despesas
                    .Where(d => d.EventoId == eventoId)
                    .SumAsync(d => d.Valor);
                await _context.SaveChangesAsync();
            }

            TempData["Sucesso"] = "Despesa excluída com sucesso!";
            return RedirectToAction("Index", new { eventoId });
        }

        [HttpGet]
        public async Task<IActionResult> RelatorioGastos(int eventoId)
        {
            var despesas = await _context.Despesas
                .Where(d => d.EventoId == eventoId)
                .OrderByDescending(d => d.DataDespesa)
                .ToListAsync();

            var evento = await _context.Eventos.FindAsync(eventoId);
            ViewBag.Evento = evento;
            ViewBag.Despesas = despesas;
            ViewBag.Total = despesas.Sum(d => d.Valor);

            return View();
        }
    }
}

