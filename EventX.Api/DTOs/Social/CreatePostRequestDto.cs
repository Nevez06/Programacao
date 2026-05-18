using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Social;

public sealed class CreatePostRequestDto
{
    [MaxLength(1000)]
    public string? Caption { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
