namespace EventX.Api.DTOs.Auth;

public sealed class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAtUtc { get; set; }
    public CurrentUserResponseDto User { get; set; } = new();
}
