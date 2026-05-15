using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Social;

public sealed class UpdateSocialProfileRequestDto
{
    [MaxLength(50)]
    public string? UserName { get; set; }

    [MaxLength(150)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(120)]
    public string? City { get; set; }

    [MaxLength(80)]
    public string? Category { get; set; }

    [MaxLength(120)]
    public string? Instagram { get; set; }

    [MaxLength(200)]
    public string? Site { get; set; }
}
