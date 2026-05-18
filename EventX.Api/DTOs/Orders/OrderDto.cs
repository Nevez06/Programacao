namespace EventX.Api.DTOs.Orders;

public sealed class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderedAt { get; set; }
    public bool ExpenseGenerated { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
