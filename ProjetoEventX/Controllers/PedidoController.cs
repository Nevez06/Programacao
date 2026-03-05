using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class PedidoController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PedidoController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Pedido/Index?eventoId=1
        public async Task<IActionResult> Index(int? eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("LoginOrganizador", "Auth");

            IQueryable<Pedido> pedidosQuery = _context.Pedidos
                .Include(p => p.Evento)
                .Include(p => p.Produto);

            if (user.TipoUsuario == "Organizador")
            {
                // Organizador vê pedidos dos seus eventos
                var eventosIds = await _context.Eventos
                    .Where(e => e.OrganizadorId == user.Id)
                    .Select(e => e.Id)
                    .ToListAsync();

                pedidosQuery = pedidosQuery.Where(p => eventosIds.Contains(p.EventoId));

                if (eventoId.HasValue)
                {
                    pedidosQuery = pedidosQuery.Where(p => p.EventoId == eventoId.Value);
                    var evento = await _context.Eventos.FindAsync(eventoId.Value);
                    ViewBag.EventoNome = evento?.NomeEvento;
                }
            }
            else if (user.TipoUsuario == "Fornecedor")
            {
                // Buscar o Fornecedor pelo email do usuário logado
                var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(f => f.Email == user.Email);
                if (fornecedor == null)
                    return RedirectToAction("LoginFornecedor", "Auth");

                // Fornecedor vê pedidos dos seus produtos
                var produtosIds = await _context.Produtos
                    .Where(pr => pr.FornecedorId == fornecedor.Id)
                    .Select(pr => pr.Id)
                    .ToListAsync();

                pedidosQuery = pedidosQuery.Where(p => produtosIds.Contains(p.ProdutoId));
            }

            ViewBag.EventoId = eventoId;
            ViewBag.TipoUsuario = user.TipoUsuario;

            if (user.TipoUsuario == "Fornecedor")
            {
                pedidosQuery = pedidosQuery
                    .Include(p => p.Produto!)
                        .ThenInclude(pr => pr.Fornecedor)
                            .ThenInclude(f => f.Pessoa);
            }

            var pedidos = await pedidosQuery.OrderByDescending(p => p.DataPedido).ToListAsync();
            return View(pedidos);
        }

        // GET: Pedido/Create?eventoId=1
        public async Task<IActionResult> Create(int eventoId, Guid? produtoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);
            if (evento == null)
            {
                TempData["ErrorMessage"] = "❌ Evento não encontrado ou sem permissão.";
                return RedirectToAction("Index", "Eventos");
            }

            var produtos = await _context.Produtos
                .Include(p => p.Fornecedor)
                    .ThenInclude(f => f.Pessoa)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            ViewBag.EventoId = eventoId;
            ViewBag.EventoNome = evento.NomeEvento;
            ViewBag.Produtos = new SelectList(produtos, "Id", "Nome", produtoId);
            ViewBag.ProdutosLista = produtos;
            ViewBag.ProdutoSelecionado = produtoId;

            return View();
        }

        // POST: Pedido/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventoId, Guid produtoId, int quantidade)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == user.Id);
            if (evento == null)
            {
                TempData["ErrorMessage"] = "❌ Evento não encontrado ou sem permissão.";
                return RedirectToAction("Index", "Eventos");
            }

            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null)
            {
                TempData["ErrorMessage"] = "❌ Produto não encontrado.";
                return RedirectToAction("Create", new { eventoId });
            }

            if (quantidade < 1)
            {
                TempData["ErrorMessage"] = "❌ Quantidade deve ser pelo menos 1.";
                return RedirectToAction("Create", new { eventoId });
            }

            var pedido = new Pedido
            {
                EventoId = eventoId,
                ProdutoId = produtoId,
                Quantidade = quantidade,
                PrecoTotal = produto.Preco * quantidade,
                StatusPedido = "Pendente",
                DataPedido = DateTime.UtcNow
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Pedido de '{produto.Nome}' (x{quantidade}) criado com sucesso! Total: R$ {pedido.PrecoTotal:N2}";
            return RedirectToAction("Index", new { eventoId });
        }

        // GET: Pedido/Details/guid
        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("LoginOrganizador", "Auth");

            var pedido = await _context.Pedidos
                .Include(p => p.Evento)
                .Include(p => p.Produto)
                    .ThenInclude(pr => pr!.Fornecedor)
                        .ThenInclude(f => f.Pessoa)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }

        // POST: Pedido/AtualizarStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarStatus(Guid id, string novoStatus)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("LoginOrganizador", "Auth");

            var pedido = await _context.Pedidos
                .Include(p => p.Produto)
                    .ThenInclude(pr => pr!.Fornecedor)
                        .ThenInclude(f => f.Pessoa)
                .Include(p => p.Evento)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null)
                return NotFound();

            pedido.StatusPedido = novoStatus;

            // Gerar despesa automaticamente quando status mudar para "Pago"
            if (novoStatus == "Pago" && !pedido.DespesaGerada)
            {
                var nomeFornecedor = pedido.Produto?.Fornecedor?.Pessoa?.Nome
                    ?? pedido.Produto?.Fornecedor?.TipoServico
                    ?? pedido.Produto?.Nome
                    ?? "Fornecedor";

                var despesa = new Despesa
                {
                    EventoId = pedido.EventoId,
                    Evento = pedido.Evento!,
                    Descricao = $"Pedido pago - {nomeFornecedor}",
                    Valor = pedido.PrecoTotal,
                    DataDespesa = DateTime.UtcNow,
                    Origem = "Pedido automático",
                    PedidoId = pedido.Id
                };

                _context.Despesas.Add(despesa);
                pedido.DespesaGerada = true;

                // Atualizar administração do evento
                var administracao = await _context.Administracoes
                    .FirstOrDefaultAsync(a => a.IdEvento == pedido.EventoId);
                if (administracao != null)
                {
                    administracao.ValorTotal = await _context.Despesas
                        .Where(d => d.EventoId == pedido.EventoId)
                        .SumAsync(d => d.Valor) + pedido.PrecoTotal;
                }

                TempData["SuccessMessage"] = $"✅ Pagamento confirmado. Despesa de R$ {pedido.PrecoTotal:N2} adicionada automaticamente.";
            }
            else
            {
                TempData["SuccessMessage"] = $"✅ Status atualizado para '{novoStatus}'.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { eventoId = pedido.EventoId });
        }

        // POST: Pedido/Cancelar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("LoginOrganizador", "Auth");

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound();

            pedido.StatusPedido = "Cancelado";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Pedido cancelado.";
            return RedirectToAction("Index", new { eventoId = pedido.EventoId });
        }
    }
}
