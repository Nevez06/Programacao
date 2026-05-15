namespace EventX.Api.DTOs.Quotes;

public sealed class QuoteDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string OrganizerId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ResponseMessage { get; set; }
    public decimal? ResponseValue { get; set; }
    public DateTime? ResponseDate { get; set; }
    public decimal? CounterProposalValue { get; set; }
    public string? CounterProposalMessage { get; set; }
    public DateTime? CounterProposalDate { get; set; }
    public int CurrentRound { get; set; }
    public DateTime? ExpireAt { get; set; }
    public string? GeneratedOrderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
