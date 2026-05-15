using EventX.Api.Data;
using EventX.Api.DTOs.Orders;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public OrderService(AppDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<OrderDto?> CreateOrderAsync(
        Guid organizerId,
        CreateOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!IsCreatePayloadValid(request))
        {
            return null;
        }

        var eventEntity = await _dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.EventId && x.OrganizerId == organizerId,
                cancellationToken);

        if (eventEntity is null)
        {
            return null;
        }

        Supplier? supplier = null;
        if (request.SupplierId is > 0)
        {
            supplier = await _dbContext.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SupplierId.Value && x.IsActive, cancellationToken);

            if (supplier is null)
            {
                return null;
            }
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = BuildOrderId(),
            EventId = eventEntity.Id,
            OrganizerId = organizerId,
            SupplierId = supplier?.Id,
            ProductId = (supplier?.Id ?? eventEntity.Id).ToString(),
            ProductName = request.Title.Trim(),
            Quantity = request.Quantity <= 0 ? 1 : request.Quantity,
            TotalPrice = request.Total,
            Status = ParseOrDefaultStatus(request.Status),
            OrderedAt = now,
            Notes = Normalize(request.Description),
            UpdatedAt = now,
            ExpenseGenerated = false
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        order.Event = eventEntity;
        order.Supplier = supplier;

        await _notificationService.CreateForUserAsync(
            organizerId,
            "pedido",
            "Novo pedido criado",
            $"O pedido {order.Id} foi criado para o evento {eventEntity.Name}.",
            "/orders",
            cancellationToken);

        return Map(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(
        Guid organizerId,
        OrderQueryDto query,
        CancellationToken cancellationToken)
    {
        await EnsureOrdersFromAcceptedQuotesAsync(organizerId, cancellationToken);

        var ordersQuery = _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Event)
            .Include(x => x.Supplier)
            .Where(x => x.OrganizerId == organizerId);

        if (query.EventId is > 0)
        {
            ordersQuery = ordersQuery.Where(x => x.EventId == query.EventId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            TryParseStatus(query.Status, out var status))
        {
            ordersQuery = ordersQuery.Where(x => x.Status == status);
        }

        var orders = await ordersQuery
            .OrderByDescending(x => x.OrderedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(Map).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid organizerId, string orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return null;
        }

        var normalizedId = orderId.Trim();
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Event)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(
                x => x.OrganizerId == organizerId && x.Id == normalizedId,
                cancellationToken);

        return order is null ? null : Map(order);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(
        Guid organizerId,
        string orderId,
        UpdateOrderStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId) ||
            !TryParseStatus(request.Status, out var status))
        {
            return null;
        }

        var normalizedId = orderId.Trim();
        var order = await _dbContext.Orders
            .Include(x => x.Event)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(
                x => x.OrganizerId == organizerId && x.Id == normalizedId,
                cancellationToken);

        if (order is null)
        {
            return null;
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        if (status == OrderStatus.Entregue)
        {
            order.ExpenseGenerated = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateForUserAsync(
            organizerId,
            "pedido",
            "Pedido atualizado",
            $"O pedido {order.Id} foi atualizado para status {order.Status}.",
            $"/orders",
            cancellationToken);

        return Map(order);
    }

    private async Task EnsureOrdersFromAcceptedQuotesAsync(Guid organizerId, CancellationToken cancellationToken)
    {
        var acceptedQuotes = await _dbContext.Quotes
            .AsNoTracking()
            .Where(x =>
                x.OrganizerId == organizerId &&
                x.Status == QuoteStatus.Aceito &&
                !string.IsNullOrWhiteSpace(x.GeneratedOrderId))
            .ToListAsync(cancellationToken);

        if (acceptedQuotes.Count == 0)
        {
            return;
        }

        var orderIds = acceptedQuotes
            .Select(x => x.GeneratedOrderId!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingOrderIds = await _dbContext.Orders
            .AsNoTracking()
            .Where(x => x.OrganizerId == organizerId && orderIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var existingSet = existingOrderIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingQuotes = acceptedQuotes
            .Where(x => !existingSet.Contains(x.GeneratedOrderId!))
            .ToList();

        if (missingQuotes.Count == 0)
        {
            return;
        }

        foreach (var quote in missingQuotes)
        {
            _dbContext.Orders.Add(new Order
            {
                Id = quote.GeneratedOrderId!,
                EventId = quote.EventId,
                OrganizerId = quote.OrganizerId,
                SupplierId = quote.SupplierId,
                QuoteId = quote.Id,
                ProductId = quote.SupplierId.ToString(),
                ProductName = quote.ServiceName,
                Quantity = 1,
                TotalPrice = quote.ResponseValue ?? quote.EstimatedValue,
                Status = OrderStatus.Pendente,
                OrderedAt = quote.ResponseDate ?? quote.UpdatedAt ?? quote.CreatedAt,
                ExpenseGenerated = false
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool TryParseStatus(string? rawStatus, out OrderStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return false;
        }

        return Enum.TryParse(rawStatus.Trim(), ignoreCase: true, out status);
    }

    private static bool IsCreatePayloadValid(CreateOrderRequestDto request)
    {
        if (request.EventId <= 0 || request.Total <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return false;
        }

        return true;
    }

    private static OrderStatus ParseOrDefaultStatus(string? rawStatus)
    {
        if (TryParseStatus(rawStatus, out var parsed))
        {
            return parsed;
        }

        return OrderStatus.Pendente;
    }

    private static string BuildOrderId()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static OrderDto Map(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            EventId = order.EventId,
            EventName = order.Event?.Name ?? string.Empty,
            ProductId = order.ProductId,
            ProductName = order.ProductName,
            Quantity = order.Quantity,
            TotalPrice = order.TotalPrice,
            Status = order.Status.ToString(),
            OrderedAt = order.OrderedAt,
            ExpenseGenerated = order.ExpenseGenerated,
            SupplierId = order.SupplierId,
            SupplierName = order.Supplier?.Name,
            Title = order.ProductName,
            Total = order.TotalPrice,
            CreatedAt = order.OrderedAt
        };
    }
}
