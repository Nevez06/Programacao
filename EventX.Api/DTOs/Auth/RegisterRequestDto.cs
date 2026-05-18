using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EventX.Api.Entities;

namespace EventX.Api.DTOs.Auth;

public sealed class RegisterRequestDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserType UserType { get; set; } = UserType.Guest;
}
