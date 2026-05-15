using EventX.Api.DTOs.Quotes;

namespace EventX.Api.Services;

public interface IQuoteService
{
    Task<IReadOnlyList<QuoteDto>> GetQuotesAsync(
        Guid organizerId,
        QuoteQueryDto query,
        CancellationToken cancellationToken);

    Task<QuoteDto?> GetQuoteByIdAsync(Guid organizerId, int quoteId, CancellationToken cancellationToken);

    Task<QuoteDto?> CreateQuoteAsync(
        Guid organizerId,
        CreateQuoteRequestDto request,
        CancellationToken cancellationToken);

    Task<QuoteDto?> UpdateStatusAsync(
        Guid organizerId,
        int quoteId,
        UpdateQuoteStatusRequestDto request,
        CancellationToken cancellationToken);

    Task<QuoteDto?> NegotiateAsync(
        Guid organizerId,
        int quoteId,
        NegotiateQuoteRequestDto request,
        CancellationToken cancellationToken);
}
