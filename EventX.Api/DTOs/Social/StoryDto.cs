namespace EventX.Api.DTOs.Social;

public sealed class StoryDto
{
    public int Id { get; set; }
    public Guid AuthorId { get; set; }
    public int? AuthorProfileId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorUserName { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool ViewedByCurrentUser { get; set; }
    public bool Viewed { get; set; }
    public int ViewsCount { get; set; }
}
