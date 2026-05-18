using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Social;

public sealed class CreateCommentRequestDto
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}
