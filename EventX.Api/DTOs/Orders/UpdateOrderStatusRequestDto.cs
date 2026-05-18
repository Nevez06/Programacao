using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Orders;

public sealed class UpdateOrderStatusRequestDto
{
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;
}
