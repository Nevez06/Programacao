using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Quotes;
using ProjetoEventX.Models;
using ProjetoEventX.Security;
using ProjetoEventX.Services;
using System.Linq.Expressions;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/quotes")]
    public class QuotesApiController : ControllerBase
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public QuotesApiController(
            EventXContext context,
            UserManager<ApplicationUser> userManager,
            NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuoteDto>>> GetQuotes([FromQuery] string? status = null, [FromQuery] int? eventId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var scope = await BuildQuoteScopeAsync(currentUser);
            if (!scope.Success)
            {
                return scope.Forbid ? Forbid() : Unauthorized();
            }

            var query = _context.Quotes
                .AsNoTracking()
                .Include(q => q.Event)
                .Include(q => q.Supplier).ThenInclude(s => s!.Pessoa)
                .Where(scope.Predicate!);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalized = status.Trim().ToLower();
                query = query.Where(q => q.Status.ToLower() == normalized);
            }

            if (eventId.HasValue)
            {
                query = query.Where(q => q.EventId == eventId.Value);
            }

            var quotes = await query
                .OrderByDescending(q => q.CreatedAt)
                .Take(300)
                .ToListAsync();

            return Ok(quotes.Select(MapQuote));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<QuoteDto>> GetQuoteById(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var quote = await _context.Quotes
                .AsNoTracking()
                .Include(q => q.Event)
                .Include(q => q.Supplier).ThenInclude(s => s!.Pessoa)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
            {
                return NotFound(new { message = "Orçamento não encontrado." });
            }

            if (!await CanAccessQuoteAsync(currentUser, quote))
            {
                return Forbid();
            }

            return Ok(MapQuote(quote));
        }

        [HttpPost]
        public async Task<ActionResult<QuoteDto>> CreateQuote([FromBody] CreateQuoteDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!string.Equals(currentUser.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == request.EventId && e.OrganizadorId == currentUser.Id);
            if (evento == null)
            {
                return BadRequest(new { message = "Evento inválido para este organizador." });
            }

            var fornecedor = await _context.Fornecedores
                .Include(f => f.Pessoa)
                .FirstOrDefaultAsync(f => f.Id == request.SupplierId);

            if (fornecedor == null)
            {
                return BadRequest(new { message = "Fornecedor não encontrado." });
            }

            var quote = new Quote
            {
                EventId = request.EventId,
                SupplierId = fornecedor.Id,
                OrganizadorId = currentUser.Id,
                ServiceName = SecurityValidator.SanitizeInput(request.ServiceName),
                Description = SecurityValidator.SanitizeHtml(request.Description),
                EstimatedValue = request.EstimatedValue,
                Status = "Pendente",
                CreatedAt = DateTime.UtcNow,
                RodadaAtual = 1,
                PrazoValidade = request.ExpireAt
            };

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            var supplierAppUser = await _userManager.FindByEmailAsync(fornecedor.Email ?? string.Empty);
            if (supplierAppUser != null)
            {
                await _notificationService.CreateAsync(
                    supplierAppUser.Id,
                    "Nova solicitação de orçamento",
                    $"Você recebeu uma nova solicitação para '{quote.ServiceName}'.",
                    "NovoPedido",
                    $"/Orcamento/Detalhes/{quote.Id}");
            }

            var created = await _context.Quotes
                .AsNoTracking()
                .Include(q => q.Event)
                .Include(q => q.Supplier).ThenInclude(s => s!.Pessoa)
                .FirstAsync(q => q.Id == quote.Id);

            return CreatedAtAction(nameof(GetQuoteById), new { id = created.Id }, MapQuote(created));
        }

        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<QuoteDto>> UpdateQuoteStatus(int id, [FromBody] UpdateQuoteStatusDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var quote = await _context.Quotes
                .Include(q => q.Event)
                .Include(q => q.Supplier)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
            {
                return NotFound(new { message = "Orçamento não encontrado." });
            }

            if (!await CanAccessQuoteAsync(currentUser, quote))
            {
                return Forbid();
            }

            var status = NormalizeStatus(request.Status);
            quote.Status = status;
            quote.ResponseMessage = request.Message != null
                ? SecurityValidator.SanitizeHtml(request.Message)
                : quote.ResponseMessage;

            if (request.Value.HasValue)
            {
                quote.ResponseValue = request.Value.Value;
            }

            if (status is "Respondido" or "Aceito" or "Recusado")
            {
                quote.ResponseDate = DateTime.UtcNow;
            }

            if (status == "Aceito" && string.Equals(currentUser.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                await EnsureOrderFromAcceptedQuoteAsync(quote);
            }

            await _context.SaveChangesAsync();

            var refreshed = await _context.Quotes
                .AsNoTracking()
                .Include(q => q.Event)
                .Include(q => q.Supplier).ThenInclude(s => s!.Pessoa)
                .FirstAsync(q => q.Id == quote.Id);

            return Ok(MapQuote(refreshed));
        }

        [HttpPut("{id:int}/negotiate")]
        public async Task<ActionResult<QuoteDto>> NegotiateQuote(int id, [FromBody] NegotiateQuoteDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var quote = await _context.Quotes
                .Include(q => q.Event)
                .Include(q => q.Supplier)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
            {
                return NotFound(new { message = "Orçamento não encontrado." });
            }

            if (!await CanAccessQuoteAsync(currentUser, quote))
            {
                return Forbid();
            }

            var action = (request.Action ?? string.Empty).Trim().ToLowerInvariant();
            if (string.Equals(currentUser.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                if (action is not ("counter" or "counter-proposal" or "contraproposta"))
                {
                    return BadRequest(new { message = "Ação de negociação inválida para organizador." });
                }

                if (!request.Value.HasValue)
                {
                    return BadRequest(new { message = "Valor da contraproposta é obrigatório." });
                }

                quote.ContraPropostaValor = request.Value.Value;
                quote.ContraPropostaMensagem = string.IsNullOrWhiteSpace(request.Message)
                    ? "Contraproposta enviada pelo organizador."
                    : SecurityValidator.SanitizeHtml(request.Message);
                quote.DataContraProposta = DateTime.UtcNow;
                quote.Status = "EmNegociacao";
                quote.RodadaAtual += 1;
            }
            else
            {
                if (action is not ("respond" or "accept-counter" or "nova-oferta" or "responder"))
                {
                    return BadRequest(new { message = "Ação de negociação inválida para fornecedor." });
                }

                quote.ResponseMessage = string.IsNullOrWhiteSpace(request.Message)
                    ? "Resposta do fornecedor"
                    : SecurityValidator.SanitizeHtml(request.Message);
                quote.ResponseValue = request.Value ?? quote.ResponseValue ?? quote.EstimatedValue;
                quote.ResponseDate = DateTime.UtcNow;
                quote.Status = "Respondido";
            }

            await _context.SaveChangesAsync();

            var refreshed = await _context.Quotes
                .AsNoTracking()
                .Include(q => q.Event)
                .Include(q => q.Supplier).ThenInclude(s => s!.Pessoa)
                .FirstAsync(q => q.Id == quote.Id);

            return Ok(MapQuote(refreshed));
        }

        private async Task<(bool Success, bool Forbid, Expression<Func<Quote, bool>>? Predicate)> BuildQuoteScopeAsync(ApplicationUser user)
        {
            if (string.Equals(user.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                return (true, false, q => q.OrganizadorId == user.Id);
            }

            if (string.Equals(user.TipoUsuario, "Fornecedor", StringComparison.OrdinalIgnoreCase))
            {
                var fornecedor = await _context.Fornecedores.AsNoTracking().FirstOrDefaultAsync(f => f.Email == user.Email);
                if (fornecedor == null)
                {
                    return (false, true, null);
                }
                var supplierId = fornecedor.Id;
                return (true, false, q => q.SupplierId == supplierId);
            }

            return (false, true, null);
        }

        private async Task<bool> CanAccessQuoteAsync(ApplicationUser user, Quote quote)
        {
            if (string.Equals(user.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                return quote.OrganizadorId == user.Id;
            }

            if (string.Equals(user.TipoUsuario, "Fornecedor", StringComparison.OrdinalIgnoreCase))
            {
                var fornecedor = await _context.Fornecedores.AsNoTracking().FirstOrDefaultAsync(f => f.Email == user.Email);
                return fornecedor != null && quote.SupplierId == fornecedor.Id;
            }

            return false;
        }

        private async Task EnsureOrderFromAcceptedQuoteAsync(Quote quote)
        {
            if (quote.PedidoGeradoId.HasValue)
            {
                return;
            }

            var supplierProduct = await _context.Produtos
                .FirstOrDefaultAsync(p => p.FornecedorId == quote.SupplierId);

            if (supplierProduct == null)
            {
                var supplier = await _context.Fornecedores.FirstAsync(f => f.Id == quote.SupplierId);
                supplierProduct = new Produto
                {
                    Nome = quote.ServiceName,
                    Descricao = quote.Description,
                    Preco = quote.ResponseValue ?? quote.EstimatedValue,
                    Tipo = "Servico",
                    FornecedorId = quote.SupplierId,
                    Fornecedor = supplier
                };

                _context.Produtos.Add(supplierProduct);
                await _context.SaveChangesAsync();
            }

            var pedido = new Pedido
            {
                EventoId = quote.EventId,
                ProdutoId = supplierProduct.Id,
                Quantidade = 1,
                PrecoTotal = quote.ResponseValue ?? quote.EstimatedValue,
                StatusPedido = "Pendente",
                DataPedido = DateTime.UtcNow,
                DespesaGerada = false
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            quote.PedidoGeradoId = pedido.Id;
        }

        private static string NormalizeStatus(string status)
        {
            var normalized = status.Trim().ToLowerInvariant();
            return normalized switch
            {
                "pendente" => "Pendente",
                "respondido" => "Respondido",
                "emnegociacao" or "em-negociacao" or "em negociação" => "EmNegociacao",
                "aceito" => "Aceito",
                "recusado" => "Recusado",
                "cancelado" => "Cancelado",
                _ => status.Trim()
            };
        }

        private static QuoteDto MapQuote(Quote quote)
        {
            return new QuoteDto
            {
                Id = quote.Id,
                EventId = quote.EventId,
                EventName = quote.Event?.NomeEvento ?? string.Empty,
                SupplierId = quote.SupplierId,
                SupplierName = quote.Supplier?.Pessoa?.Nome ?? quote.Supplier?.UserName ?? $"Fornecedor {quote.SupplierId}",
                OrganizerId = quote.OrganizadorId,
                ServiceName = quote.ServiceName,
                Description = quote.Description,
                EstimatedValue = quote.EstimatedValue,
                Status = quote.Status,
                CreatedAt = quote.CreatedAt,
                ResponseMessage = quote.ResponseMessage,
                ResponseValue = quote.ResponseValue,
                ResponseDate = quote.ResponseDate,
                CounterProposalValue = quote.ContraPropostaValor,
                CounterProposalMessage = quote.ContraPropostaMensagem,
                CounterProposalDate = quote.DataContraProposta,
                CurrentRound = quote.RodadaAtual,
                ExpireAt = quote.PrazoValidade,
                GeneratedOrderId = quote.PedidoGeradoId
            };
        }
    }
}
