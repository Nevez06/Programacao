namespace EventX.Api.DTOs.Social;

public sealed class PostDetailsDto
{
    public int Id { get; set; }
    public Guid AuthorId { get; set; }
    public int? AuthorProfileId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorUsername { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string Caption { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public IReadOnlyList<PostCommentDto> Comments { get; set; } = [];
}
