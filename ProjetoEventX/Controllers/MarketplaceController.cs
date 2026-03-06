using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    public class MarketplaceController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MarketplaceController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Marketplace
        public async Task<IActionResult> Index(string? categoria, string? localizacao, string? uf, decimal? precoMin, decimal? precoMax, int? avaliacaoMin, string? busca, string? ordenar)
        {
            var query = _context.Fornecedores
                .Include(f => f.Pessoa)
                .Include(f => f.Produtos)
                .Include(f => f.Avaliacoes)
                .Where(f => f.Produtos.Any()) // Só mostra fornecedores com produtos
                .AsQueryable();

            // Filtro por categoria (TipoServico)
            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(f => f.TipoServico == categoria);

            // Filtro por localização (cidade)
            if (!string.IsNullOrWhiteSpace(localizacao))
                query = query.Where(f => f.Cidade.ToLower().Contains(localizacao.ToLower()));

            // Filtro por UF
            if (!string.IsNullOrWhiteSpace(uf))
                query = query.Where(f => f.UF == uf);

            // Filtro por avaliação mínima
            if (avaliacaoMin.HasValue && avaliacaoMin.Value > 0)
                query = query.Where(f => f.AvaliacaoMedia >= avaliacaoMin.Value);

            // Filtro por faixa de preço (baseado nos produtos)
            if (precoMin.HasValue)
                query = query.Where(f => f.Produtos.Any(p => p.Preco >= precoMin.Value));

            if (precoMax.HasValue)
                query = query.Where(f => f.Produtos.Any(p => p.Preco <= precoMax.Value));

            // Busca por nome
            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(f => f.Pessoa.Nome.ToLower().Contains(busca.ToLower())
                    || f.TipoServico.ToLower().Contains(busca.ToLower())
                    || f.Produtos.Any(p => p.Nome.ToLower().Contains(busca.ToLower())));

            // Ordenação
            query = ordenar switch
            {
                "avaliacao" => query.OrderByDescending(f => f.AvaliacaoMedia),
                "preco_asc" => query.OrderBy(f => f.Produtos.Min(p => p.Preco)),
                "preco_desc" => query.OrderByDescending(f => f.Produtos.Min(p => p.Preco)),
                "nome" => query.OrderBy(f => f.Pessoa.Nome),
                "recente" => query.OrderByDescending(f => f.DataCadastro),
                _ => query.OrderByDescending(f => f.AvaliacaoMedia)
            };

            var fornecedores = await query.ToListAsync();

            // Categorias disponíveis para o filtro
            ViewBag.Categorias = await _context.Fornecedores
                .Where(f => f.Produtos.Any())
                .Select(f => f.TipoServico)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // UFs disponíveis
            ViewBag.UFs = await _context.Fornecedores
                .Where(f => f.Produtos.Any())
                .Select(f => f.UF)
                .Distinct()
                .OrderBy(u => u)
                .ToListAsync();

            // Manter filtros na view
            ViewBag.FiltroCategoria = categoria;
            ViewBag.FiltroLocalizacao = localizacao;
            ViewBag.FiltroUF = uf;
            ViewBag.FiltroPrecoMin = precoMin;
            ViewBag.FiltroPrecoMax = precoMax;
            ViewBag.FiltroAvaliacaoMin = avaliacaoMin;
            ViewBag.FiltroBusca = busca;
            ViewBag.FiltroOrdenar = ordenar;
            ViewBag.TotalFornecedores = fornecedores.Count;

            return View(fornecedores);
        }

        // GET: Marketplace/DetalhesFornecedor/5
        public async Task<IActionResult> DetalhesFornecedor(int? id)
        {
            if (id == null)
                return NotFound();

            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .Include(f => f.Produtos)
                .Include(f => f.Avaliacoes)
                    .ThenInclude(a => a.Organizador)
                        .ThenInclude(o => o!.Pessoa)
                .Include(f => f.Avaliacoes)
                    .ThenInclude(a => a.Evento)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fornecedor == null)
                return NotFound();

            ViewBag.TotalPedidos = await _context.Pedidos
                .CountAsync(p => fornecedor.Produtos.Select(pr => pr.Id).Contains(p.ProdutoId));

            // Verificar se o usuário logado é organizador
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.TipoUsuario == "Organizador")
                {
                    ViewBag.IsOrganizador = true;
                    ViewBag.EventosOrganizador = await _context.Eventos
                        .Where(e => e.OrganizadorId == user.Id)
                        .OrderBy(e => e.NomeEvento)
                        .ToListAsync();
                }
            }

            return View(fornecedor);
        }

        // GET: Marketplace/SolicitarOrcamento/5
        [Authorize]
        public async Task<IActionResult> SolicitarOrcamento(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            if (id == null)
                return NotFound();

            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fornecedor == null)
                return NotFound();

            ViewBag.Fornecedor = fornecedor;
            ViewBag.EventosOrganizador = await _context.Eventos
                .Where(e => e.OrganizadorId == user.Id)
                .OrderBy(e => e.NomeEvento)
                .ToListAsync();

            return View();
        }

        // POST: Marketplace/SolicitarOrcamento
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarOrcamento(int fornecedorId, string titulo, string descricao, int? eventoId, string? tipoServicoDesejado, DateTime? dataEvento, string? localEvento, decimal? orcamentoEstimado, int? quantidadeConvidados)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .FirstOrDefaultAsync(f => f.Id == fornecedorId);

            if (fornecedor == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descricao))
            {
                TempData["Erro"] = "Título e descrição são obrigatórios.";
                return RedirectToAction(nameof(SolicitarOrcamento), new { id = fornecedorId });
            }

            var solicitacao = new SolicitacaoOrcamento
            {
                OrganizadorId = user.Id,
                FornecedorId = fornecedorId,
                EventoId = eventoId,
                Titulo = titulo,
                Descricao = descricao,
                TipoServicoDesejado = tipoServicoDesejado,
                DataEvento = dataEvento,
                LocalEvento = localEvento,
                OrcamentoEstimado = orcamentoEstimado,
                QuantidadeConvidados = quantidadeConvidados,
                Status = "Pendente"
            };

            _context.SolicitacoesOrcamento.Add(solicitacao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Solicitação de orçamento enviada com sucesso!";
            return RedirectToAction(nameof(MinhasSolicitacoes));
        }

        // GET: Marketplace/MinhasSolicitacoes
        [Authorize]
        public async Task<IActionResult> MinhasSolicitacoes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var solicitacoes = await _context.SolicitacoesOrcamento
                .Include(s => s.Fornecedor)
                    .ThenInclude(f => f!.Pessoa)
                .Include(s => s.Evento)
                .Where(s => s.OrganizadorId == user.Id)
                .OrderByDescending(s => s.DataSolicitacao)
                .ToListAsync();

            return View(solicitacoes);
        }

        // GET: Marketplace/SolicitacoesRecebidas
        [Authorize]
        public async Task<IActionResult> SolicitacoesRecebidas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Fornecedor")
                return RedirectToAction("LoginFornecedor", "Auth");

            var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(f => f.Email == user.Email);
            if (fornecedor == null)
                return NotFound();

            var solicitacoes = await _context.SolicitacoesOrcamento
                .Include(s => s.Organizador)
                    .ThenInclude(o => o!.Pessoa)
                .Include(s => s.Evento)
                .Where(s => s.FornecedorId == fornecedor.Id)
                .OrderByDescending(s => s.DataSolicitacao)
                .ToListAsync();

            return View(solicitacoes);
        }

        // POST: Marketplace/ResponderOrcamento
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResponderOrcamento(int solicitacaoId, string resposta, decimal? valorProposto, string acao)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Fornecedor")
                return RedirectToAction("LoginFornecedor", "Auth");

            var fornecedor = await _context.Fornecedores.FirstOrDefaultAsync(f => f.Email == user.Email);
            if (fornecedor == null)
                return NotFound();

            var solicitacao = await _context.SolicitacoesOrcamento
                .FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.FornecedorId == fornecedor.Id);

            if (solicitacao == null)
                return NotFound();

            if (acao == "responder")
            {
                solicitacao.Status = "Respondido";
                solicitacao.RespostaFornecedor = resposta;
                solicitacao.ValorProposto = valorProposto;
                solicitacao.DataResposta = DateTime.UtcNow;
                TempData["Sucesso"] = "Orçamento respondido com sucesso!";
            }
            else if (acao == "recusar")
            {
                solicitacao.Status = "Recusado";
                solicitacao.RespostaFornecedor = resposta;
                solicitacao.DataResposta = DateTime.UtcNow;
                TempData["Sucesso"] = "Solicitação recusada.";
            }

            solicitacao.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(SolicitacoesRecebidas));
        }

        // POST: Marketplace/AceitarOrcamento
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AceitarOrcamento(int solicitacaoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
                return RedirectToAction("LoginOrganizador", "Auth");

            var solicitacao = await _context.SolicitacoesOrcamento
                .FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.OrganizadorId == user.Id);

            if (solicitacao == null)
                return NotFound();

            solicitacao.Status = "Aceito";
            solicitacao.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Orçamento aceito! Entre em contato com o fornecedor para finalizar.";
            return RedirectToAction(nameof(MinhasSolicitacoes));
        }
    }
}
