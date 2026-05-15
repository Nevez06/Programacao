namespace EventX.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "EventX";
    public string Audience { get; set; } = "EventX.Clients";
    public string Key { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 120;
}
