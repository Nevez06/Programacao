using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Quotes;

public sealed class UpdateQuoteStatusRequestDto
{
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Message { get; set; }

    [Range(0.01, 999999999)]
    public decimal? Value { get; set; }
}
