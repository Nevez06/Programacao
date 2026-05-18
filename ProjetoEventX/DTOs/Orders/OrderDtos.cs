using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Orders
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderedAt { get; set; }
        public bool ExpenseGenerated { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}