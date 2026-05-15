using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventX.Api.Auth;
using EventX.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventX.Api.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.UserType.ToString()),
            new Claim("user_type", user.UserType.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiresAtUtc);
    }
}
