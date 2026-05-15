using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Orders;
using ProjetoEventX.Models;
using System.Linq.Expressions;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/orders")]
    public class OrdersApiController : ControllerBase
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersApiController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] int? eventId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var scope = await BuildOrderScopeAsync(currentUser);
            if (!scope.Success)
            {
                return scope.Forbid ? Forbid() : Unauthorized();
            }

            var query = _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Evento)
                .Include(p => p.Produto)
                    .ThenInclude(pr => pr!.Fornecedor)
                        .ThenInclude(f => f!.Pessoa)
                .Where(scope.Predicate!);

            if (eventId.HasValue)
            {
                query = query.Where(o => o.EventoId == eventId.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.DataPedido)
                .Take(300)
                .ToListAsync();

            return Ok(orders.Select(MapOrder));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var order = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Evento)
                .Include(p => p.Produto)
                    .ThenInclude(pr => pr!.Fornecedor)
                        .ThenInclude(f => f!.Pessoa)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (order == null)
            {
                return NotFound(new { message = "Pedido não encontrado." });
            }

            if (!await CanAccessOrderAsync(currentUser, order))
            {
                return Forbid();
            }

            return Ok(MapOrder(order));
        }

        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto request)
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

            var order = await _context.Pedidos
                .Include(p => p.Evento)
                .Include(p => p.Produto)
                    .ThenInclude(pr => pr!.Fornecedor)
                        .ThenInclude(f => f!.Pessoa)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (order == null)
            {
                return NotFound(new { message = "Pedido não encontrado." });
            }

            if (!await CanAccessOrderAsync(currentUser, order))
            {
                return Forbid();
            }

            order.StatusPedido = NormalizeStatus(request.Status);
            await _context.SaveChangesAsync();

            return Ok(MapOrder(order));
        }

        private async Task<(bool Success, bool Forbid, Expression<Func<Pedido, bool>>? Predicate)> BuildOrderScopeAsync(ApplicationUser user)
        {
            if (string.Equals(user.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                var organizerId = user.Id;
                return (true, false, o => o.Evento != null && o.Evento.OrganizadorId == organizerId);
            }

            if (string.Equals(user.TipoUsuario, "Fornecedor", StringComparison.OrdinalIgnoreCase))
            {
                var supplier = await _context.Fornecedores.AsNoTracking().FirstOrDefaultAsync(f => f.Email == user.Email);
                if (supplier == null)
                {
                    return (false, true, null);
                }

                var supplierId = supplier.Id;
                return (true, false, o => o.Produto != null && o.Produto.FornecedorId == supplierId);
            }

            return (false, true, null);
        }

        private async Task<bool> CanAccessOrderAsync(ApplicationUser user, Pedido order)
        {
            if (string.Equals(user.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                return order.Evento != null && order.Evento.OrganizadorId == user.Id;
            }

            if (string.Equals(user.TipoUsuario, "Fornecedor", StringComparison.OrdinalIgnoreCase))
            {
                var supplier = await _context.Fornecedores.AsNoTracking().FirstOrDefaultAsync(f => f.Email == user.Email);
                return supplier != null && order.Produto != null && order.Produto.FornecedorId == supplier.Id;
            }

            return false;
        }

        private static string NormalizeStatus(string status)
        {
            var normalized = status.Trim().ToLowerInvariant();
            return normalized switch
            {
                "pendente" => "Pendente",
                "pago" => "Pago",
                "enviado" => "Enviado",
                "entregue" => "Entregue",
                "cancelado" => "Cancelado",
                _ => status.Trim()
            };
        }

        private static OrderDto MapOrder(Pedido order)
        {
            return new OrderDto
            {
                Id = order.Id,
                EventId = order.EventoId,
                EventName = order.Evento?.NomeEvento ?? string.Empty,
                ProductId = order.ProdutoId,
                ProductName = order.Produto?.Nome ?? string.Empty,
                Quantity = order.Quantidade,
                TotalPrice = order.PrecoTotal,
                Status = order.StatusPedido,
                OrderedAt = order.DataPedido,
                ExpenseGenerated = order.DespesaGerada,
                SupplierId = order.Produto?.FornecedorId,
                SupplierName = order.Produto?.Fornecedor?.Pessoa?.Nome
            };
        }
    }
}