using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Quotes;

public sealed class CreateQuoteRequestDto
{
    [Range(1, int.MaxValue)]
    public int EventId { get; set; }

    [Range(1, int.MaxValue)]
    public int SupplierId { get; set; }

    [Required]
    [MaxLength(180)]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    [MaxLength(2500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999999)]
    public decimal EstimatedValue { get; set; }

    public DateTime? ExpireAt { get; set; }
}
