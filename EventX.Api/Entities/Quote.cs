using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class Quote
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    [Required]
    [MaxLength(180)]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    [MaxLength(2500)]
    public string Description { get; set; } = string.Empty;

    public decimal EstimatedValue { get; set; }

    public QuoteStatus Status { get; set; } = QuoteStatus.Pendente;

    [MaxLength(1000)]
    public string? ResponseMessage { get; set; }

    public decimal? ResponseValue { get; set; }
    public DateTime? ResponseDate { get; set; }

    public decimal? CounterProposalValue { get; set; }

    [MaxLength(1000)]
    public string? CounterProposalMessage { get; set; }

    public DateTime? CounterProposalDate { get; set; }

    public int CurrentRound { get; set; } = 0;
    public DateTime? ExpireAt { get; set; }

    [MaxLength(40)]
    public string? GeneratedOrderId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Order? Order { get; set; }
}
