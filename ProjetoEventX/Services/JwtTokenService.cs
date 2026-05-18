using Microsoft.IdentityModel.Tokens;
using ProjetoEventX.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjetoEventX.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(ApplicationUser user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                ?? _configuration["Jwt:Key"]
                ?? "EventX-Dev-Only-Replace-This-Key-Immediately-2026!";
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? _configuration["Jwt:Issuer"]
                ?? "EventX";
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? _configuration["Jwt:Audience"]
                ?? "EventX.Clients";

            var expiresMinutes = int.TryParse(
                Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES") ?? _configuration["Jwt:ExpiresMinutes"],
                out var parsedExpires)
                ? Math.Clamp(parsedExpires, 5, 1440)
                : 120;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString()),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new("tipoUsuario", user.TipoUsuario ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}