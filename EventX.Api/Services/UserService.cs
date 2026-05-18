using EventX.Api.Data;
using EventX.Api.DTOs.Users;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfileDto?> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.SocialProfile)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        return user is null ? null : Map(user);
    }

    public async Task<(UserProfileDto? Profile, bool EmailAlreadyInUse, bool InvalidPayload)> UpdateMyProfileAsync(
        Guid userId,
        UpdateUserProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.SocialProfile)
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken);

        if (user is null)
        {
            return (null, false, false);
        }

        var hasName = request.FullName is not null;
        var hasEmail = request.Email is not null;
        if (!hasName && !hasEmail)
        {
            return (null, false, true);
        }

        if (hasName)
        {
            var fullName = request.FullName!.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (null, false, true);
            }

            user.FullName = fullName;
        }

        if (hasEmail)
        {
            var normalizedEmail = request.Email!.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return (null, false, true);
            }

            var emailInUse = await _dbContext.Users
                .AnyAsync(x => x.Id != userId && x.Email.ToLower() == normalizedEmail, cancellationToken);

            if (emailInUse)
            {
                return (null, true, false);
            }

            user.Email = normalizedEmail;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (Map(user), false, false);
    }

    private static UserProfileDto Map(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.FullName,
            FullName = user.FullName,
            Email = user.Email,
            UserType = user.UserType.ToString(),
            AvatarUrl = user.SocialProfile?.AvatarUrl,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}
