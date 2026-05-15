namespace EventX.Api.Entities;

public sealed class SocialFollow
{
    public int Id { get; set; }

    public Guid FollowerUserId { get; set; }
    public User FollowerUser { get; set; } = null!;

    public int FollowingProfileId { get; set; }
    public SocialProfile FollowingProfile { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
