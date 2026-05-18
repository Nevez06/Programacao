using EventX.Api.DTOs.Users;

namespace EventX.Api.Services;

public interface IUserService
{
    Task<UserProfileDto?> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<(UserProfileDto? Profile, bool EmailAlreadyInUse, bool InvalidPayload)> UpdateMyProfileAsync(
        Guid userId,
        UpdateUserProfileRequestDto request,
        CancellationToken cancellationToken);
}
