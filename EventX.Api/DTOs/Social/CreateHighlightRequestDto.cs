using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Social;

public sealed class CreateHighlightRequestDto
{
    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CapaUrl { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> StoryIds { get; set; } = [];
}
