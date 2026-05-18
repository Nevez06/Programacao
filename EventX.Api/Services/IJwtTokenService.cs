using EventX.Api.Entities;

namespace EventX.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
