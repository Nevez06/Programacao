using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class Order
{
    [MaxLength(40)]
    public string Id { get; set; } = string.Empty;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? QuoteId { get; set; }
    public Quote? Quote { get; set; }

    [MaxLength(80)]
    public string ProductId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;
    public decimal TotalPrice { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pendente;
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public bool ExpenseGenerated { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
