namespace EventX.Api.DTOs.Social;

public sealed class SocialProfileDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? City { get; set; }
    public string? Category { get; set; }
    public string? Instagram { get; set; }
    public string? Site { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsFollowing { get; set; }
    public int PostsCount { get; set; }
}
