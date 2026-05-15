using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Social;

public sealed class CreateSocialPostDto
{
    [MaxLength(1000)]
    public string? Content { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
