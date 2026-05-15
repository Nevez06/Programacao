using System.Security.Claims;
using EventX.Api.Data;
using EventX.Api.DTOs.Auth;
using EventX.Api.Entities;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const int MinPasswordLength = 8;

    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        AppDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasherService = passwordHasherService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (request.Password.Length < MinPasswordLength)
        {
            return BadRequest(new { message = $"Password must have at least {MinPasswordLength} characters." });
        }

        var exists = await _dbContext.Users.AnyAsync(
            x => x.Email.ToLower() == normalizedEmail,
            cancellationToken);

        if (exists)
        {
            return BadRequest(new { message = "Email already registered." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasherService.HashPassword(request.Password),
            UserType = request.UserType,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(BuildAuthResponse(user));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail && x.IsActive, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var isPasswordValid = _passwordHasherService.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        return Ok(BuildAuthResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrentUserResponseDto>> Me(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(MapCurrentUser(user));
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            User = MapCurrentUser(user)
        };
    }

    private static CurrentUserResponseDto MapCurrentUser(User user)
    {
        return new CurrentUserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            UserType = user.UserType.ToString(),
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}
