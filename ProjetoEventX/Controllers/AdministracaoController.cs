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
                .Include(e => e.ListasConvidados)
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
            // Pedidos que ainda NÃO geraram despesa automática (evitar dupla contagem)
            var totalPedidos = evento.Pedidos
                .Where(p => !p.DespesaGerada)
                .Sum(p => p.PrecoTotal);
            var totalGeral = totalGasto + totalPedidos;

            var administracao = await _context.Administracoes
                .FirstOrDefaultAsync(a => a.IdEvento == eventoId);

            // === DADOS ANALÍTICOS ===

            // Distribuição de gastos por categoria (baseado na descrição)
            var categoriasGastos = CategorizarDespesas(despesas);

            // Evolução de despesas ao longo do tempo (agrupado por dia)
            var evolucaoDespesas = despesas
                .GroupBy(d => d.DataDespesa.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Data = g.Key.ToString("dd/MM"), Valor = g.Sum(d => d.Valor) })
                .ToList();

            // Convidados
            var convidados = evento.ListasConvidados ?? new List<ListaConvidado>();
            var totalConvidados = convidados.Count;
            var convidadosConfirmados = convidados.Count(c => c.ConfirmaPresenca == "Confirmado");
            var convidadosPendentes = convidados.Count(c => c.ConfirmaPresenca == "Pendente");
            var convidadosRecusados = totalConvidados - convidadosConfirmados - convidadosPendentes;

            // Pedidos por status
            var pedidos = evento.Pedidos ?? new List<Pedido>();
            var pedidosAprovados = pedidos.Count(p => p.StatusPedido == "Pago" || p.StatusPedido == "Entregue");
            var pedidosPendentes = pedidos.Count(p => p.StatusPedido == "Pendente");
            var pedidosCancelados = pedidos.Count(p => p.StatusPedido != "Pago" && p.StatusPedido != "Entregue" && p.StatusPedido != "Pendente");

            ViewBag.Evento = evento;
            ViewBag.Despesas = despesas;
            ViewBag.TotalGasto = totalGasto;
            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.TotalGeral = totalGeral;
            ViewBag.Administracao = administracao;

            // Analíticos
            ViewBag.CategoriasLabels = categoriasGastos.Select(c => c.Categoria).ToList();
            ViewBag.CategoriasValores = categoriasGastos.Select(c => c.Valor).ToList();
            ViewBag.EvolucaoLabels = evolucaoDespesas.Select(e => e.Data).ToList();
            ViewBag.EvolucaoValores = evolucaoDespesas.Select(e => e.Valor).ToList();
            ViewBag.TotalConvidados = totalConvidados;
            ViewBag.ConvidadosConfirmados = convidadosConfirmados;
            ViewBag.ConvidadosPendentes = convidadosPendentes;
            ViewBag.ConvidadosRecusados = convidadosRecusados;
            ViewBag.PedidosAprovados = pedidosAprovados;
            ViewBag.PedidosPendentes = pedidosPendentes;
            ViewBag.PedidosCancelados = pedidosCancelados;
            ViewBag.TotalPedidosCount = pedidos.Count;

            return View();
        }

        private List<(string Categoria, decimal Valor)> CategorizarDespesas(List<Despesa> despesas)
        {
            var categorias = new Dictionary<string, decimal>
            {
                { "Buffet", 0m },
                { "Decoração", 0m },
                { "Som", 0m },
                { "Fotografia", 0m },
                { "Outros", 0m }
            };

            foreach (var d in despesas)
            {
                var desc = d.Descricao?.ToLower() ?? "";
                if (desc.Contains("buffet") || desc.Contains("comida") || desc.Contains("alimenta") || desc.Contains("refei") || desc.Contains("catering"))
                    categorias["Buffet"] += d.Valor;
                else if (desc.Contains("decora") || desc.Contains("flores") || desc.Contains("arranjo") || desc.Contains("ornament"))
                    categorias["Decoração"] += d.Valor;
                else if (desc.Contains("som") || desc.Contains("dj") || desc.Contains("música") || desc.Contains("musica") || desc.Contains("audio") || desc.Contains("áudio"))
                    categorias["Som"] += d.Valor;
                else if (desc.Contains("foto") || desc.Contains("vídeo") || desc.Contains("video") || desc.Contains("filmag") || desc.Contains("câmera") || desc.Contains("camera"))
                    categorias["Fotografia"] += d.Valor;
                else
                    categorias["Outros"] += d.Valor;
            }

            return categorias
                .Where(c => c.Value > 0)
                .Select(c => (c.Key, c.Value))
                .ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarDespesa(int eventoId, string Descricao, string Valor)
        {
            // Converter valor no formato brasileiro (ponto = milhares, vírgula = decimal)
            var valorLimpo = Valor.Replace(".", "").Replace(",", ".");
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
            // Converter valor no formato brasileiro (ponto = milhares, vírgula = decimal)
            var valorLimpo = Valor.Replace(".", "").Replace(",", ".");
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

