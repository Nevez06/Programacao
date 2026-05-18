using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Orders;

public sealed class CreateOrderRequestDto
{
    [Range(1, int.MaxValue)]
    public int EventId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [Range(0.01, 999999999)]
    public decimal Total { get; set; }

    [Range(1, int.MaxValue)]
    public int? SupplierId { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(40)]
    public string? Status { get; set; }
}
