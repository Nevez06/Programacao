namespace EventX.Api.DTOs.Orders;

public sealed class OrderQueryDto
{
    public int? EventId { get; set; }
    public string? Status { get; set; }
}
