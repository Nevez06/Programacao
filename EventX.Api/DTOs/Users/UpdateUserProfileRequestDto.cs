using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Users;

public sealed class UpdateUserProfileRequestDto
{
    [MaxLength(150)]
    public string? FullName { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }
}
