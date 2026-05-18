using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Auth;

public sealed class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
