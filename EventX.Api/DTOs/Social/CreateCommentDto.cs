using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Social;

public sealed class CreateCommentDto
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}
