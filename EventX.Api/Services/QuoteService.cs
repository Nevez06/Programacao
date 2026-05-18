using EventX.Api.Data;
using EventX.Api.DTOs.Quotes;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class QuoteService : IQuoteService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public QuoteService(AppDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<QuoteDto>> GetQuotesAsync(
        Guid organizerId,
        QuoteQueryDto query,
        CancellationToken cancellationToken)
    {
        var quotesQuery = _dbContext.Quotes
            .AsNoTracking()
            .Include(x => x.Event)
            .Include(x => x.Supplier)
            .Where(x => x.OrganizerId == organizerId);

        if (query.EventId is > 0)
        {
            quotesQuery = quotesQuery.Where(x => x.EventId == query.EventId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            TryParseStatus(query.Status, out var status))
        {
            quotesQuery = quotesQuery.Where(x => x.Status == status);
        }

        var quotes = await quotesQuery
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return quotes.Select(Map).ToList();
    }

    public async Task<QuoteDto?> GetQuoteByIdAsync(Guid organizerId, int quoteId, CancellationToken cancellationToken)
    {
        if (quoteId <= 0)
        {
            return null;
        }

        var quote = await _dbContext.Quotes
            .AsNoTracking()
            .Include(x => x.Event)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(
                x => x.Id == quoteId && x.OrganizerId == organizerId,
                cancellationToken);

        return quote is null ? null : Map(quote);
    }

    public async Task<QuoteDto?> CreateQuoteAsync(
        Guid organizerId,
        CreateQuoteRequestDto request,
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

        var supplier = await _dbContext.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.SupplierId && x.IsActive, cancellationToken);

        if (supplier is null)
        {
            return null;
        }

        var quote = new Quote
        {
            EventId = eventEntity.Id,
            SupplierId = supplier.Id,
            OrganizerId = organizerId,
            ServiceName = request.ServiceName.Trim(),
            Description = request.Description.Trim(),
            EstimatedValue = request.EstimatedValue,
            Status = QuoteStatus.Pendente,
            CurrentRound = 0,
            ExpireAt = request.ExpireAt,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Quotes.Add(quote);
        await _dbContext.SaveChangesAsync(cancellationToken);

        quote.Event = eventEntity;
        quote.Supplier = supplier;
        return Map(quote);
    }

    public async Task<QuoteDto?> UpdateStatusAsync(
        Guid organizerId,
        int quoteId,
        UpdateQuoteStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!TryParseStatus(request.Status, out var status))
        {
            return null;
        }

        var quote = await _dbContext.Quotes
            .Include(x => x.Event)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(
                x => x.Id == quoteId && x.OrganizerId == organizerId,
                cancellationToken);

        if (quote is null)
        {
            return null;
        }

        quote.Status = status;
        quote.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Message))
        {
            quote.ResponseMessage = request.Message.Trim();
            quote.ResponseDate = DateTime.UtcNow;
        }

        if (request.Value is > 0)
        {
            quote.ResponseValue = request.Value.Value;
            quote.ResponseDate = DateTime.UtcNow;
        }

        if (status == QuoteStatus.Aceito && string.IsNullOrWhiteSpace(quote.GeneratedOrderId))
        {
            quote.GeneratedOrderId = BuildOrderId(quote.Id);
        }

        await SyncOrderFromQuoteAsync(quote, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateForUserAsync(
            organizerId,
            "orcamento",
            "Orçamento atualizado",
            $"O orçamento #{quote.Id} foi atualizado para status {quote.Status}.",
            $"/budget/quotes",
            cancellationToken);

        return Map(quote);
    }

    public async Task<QuoteDto?> NegotiateAsync(
        Guid organizerId,
        int quoteId,
        NegotiateQuoteRequestDto request,
        CancellationToken cancellationToken)
    {
        var action = request.Action?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(action))
        {
            return null;
        }

        var quote = await _dbContext.Quotes
            .Include(x => x.Event)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(
                x => x.Id == quoteId && x.OrganizerId == organizerId,
                cancellationToken);

        if (quote is null)
        {
            return null;
        }

        switch (action)
        {
            case "counter":
                if (request.Value is not > 0)
                {
                    return null;
                }

                quote.CounterProposalValue = request.Value.Value;
                quote.CounterProposalMessage = Normalize(request.Message);
                quote.CounterProposalDate = DateTime.UtcNow;
                quote.CurrentRound += 1;
                quote.Status = QuoteStatus.EmNegociacao;
                break;

            case "accept":
                quote.Status = QuoteStatus.Aceito;
                quote.ResponseValue = request.Value > 0 ? request.Value.Value : quote.ResponseValue ?? quote.EstimatedValue;
                quote.ResponseMessage = Normalize(request.Message) ?? "Proposta aceita.";
                quote.ResponseDate = DateTime.UtcNow;
                quote.GeneratedOrderId ??= BuildOrderId(quote.Id);
                break;

            case "reject":
                quote.Status = QuoteStatus.Recusado;
                quote.ResponseMessage = Normalize(request.Message) ?? "Proposta recusada.";
                quote.ResponseDate = DateTime.UtcNow;
                break;

            case "cancel":
                quote.Status = QuoteStatus.Cancelado;
                quote.ResponseMessage = Normalize(request.Message) ?? "Negociação cancelada.";
                quote.ResponseDate = DateTime.UtcNow;
                break;

            default:
                return null;
        }

        quote.UpdatedAt = DateTime.UtcNow;
        await SyncOrderFromQuoteAsync(quote, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateForUserAsync(
            organizerId,
            "orcamento",
            "Negociação de orçamento",
            BuildNegotiationMessage(quote.Id, action, quote.Status),
            $"/budget/quotes",
            cancellationToken);

        return Map(quote);
    }

    private static bool IsCreatePayloadValid(CreateQuoteRequestDto request)
    {
        if (request.EventId <= 0 || request.SupplierId <= 0 || request.EstimatedValue <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.ServiceName) || string.IsNullOrWhiteSpace(request.Description))
        {
            return false;
        }

        return true;
    }

    private static bool TryParseStatus(string? rawStatus, out QuoteStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return false;
        }

        return Enum.TryParse(rawStatus.Trim(), ignoreCase: true, out status);
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string BuildOrderId(int quoteId)
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{quoteId}";
    }

    private static string BuildNegotiationMessage(int quoteId, string action, QuoteStatus status)
    {
        return action switch
        {
            "counter" => $"A contraproposta do orçamento #{quoteId} foi registrada.",
            "accept" => $"O orçamento #{quoteId} foi aceito e segue para pedido.",
            "reject" => $"O orçamento #{quoteId} foi recusado.",
            "cancel" => $"A negociação do orçamento #{quoteId} foi cancelada.",
            _ => $"O orçamento #{quoteId} foi atualizado para {status}."
        };
    }

    private async Task SyncOrderFromQuoteAsync(Quote quote, CancellationToken cancellationToken)
    {
        if (quote.Status != QuoteStatus.Aceito)
        {
            return;
        }

        quote.GeneratedOrderId ??= BuildOrderId(quote.Id);

        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.QuoteId == quote.Id, cancellationToken);

        if (order is null)
        {
            order = new Order
            {
                Id = quote.GeneratedOrderId,
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
            };

            _dbContext.Orders.Add(order);
            return;
        }

        order.EventId = quote.EventId;
        order.OrganizerId = quote.OrganizerId;
        order.SupplierId = quote.SupplierId;
        order.ProductId = quote.SupplierId.ToString();
        order.ProductName = quote.ServiceName;
        order.TotalPrice = quote.ResponseValue ?? quote.EstimatedValue;
        order.UpdatedAt = DateTime.UtcNow;
    }

    private static QuoteDto Map(Quote quote)
    {
        return new QuoteDto
        {
            Id = quote.Id,
            EventId = quote.EventId,
            EventName = quote.Event?.Name ?? string.Empty,
            SupplierId = quote.SupplierId,
            SupplierName = quote.Supplier?.Name ?? string.Empty,
            OrganizerId = quote.OrganizerId.ToString(),
            ServiceName = quote.ServiceName,
            Description = quote.Description,
            EstimatedValue = quote.EstimatedValue,
            Status = quote.Status.ToString(),
            CreatedAt = quote.CreatedAt,
            ResponseMessage = quote.ResponseMessage,
            ResponseValue = quote.ResponseValue,
            ResponseDate = quote.ResponseDate,
            CounterProposalValue = quote.CounterProposalValue,
            CounterProposalMessage = quote.CounterProposalMessage,
            CounterProposalDate = quote.CounterProposalDate,
            CurrentRound = quote.CurrentRound,
            ExpireAt = quote.ExpireAt,
            GeneratedOrderId = quote.GeneratedOrderId,
            Title = quote.ServiceName,
            Amount = quote.ResponseValue ?? quote.EstimatedValue
        };
    }
}
