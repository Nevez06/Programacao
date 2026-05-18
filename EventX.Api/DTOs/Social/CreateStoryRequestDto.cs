using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Social;

public sealed class CreateStoryRequestDto
{
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? MediaUrl { get; set; }

    [MaxLength(500)]
    public string? Caption { get; set; }

    [Range(1, 168)]
    public int? ExpiresInHours { get; set; }
}
