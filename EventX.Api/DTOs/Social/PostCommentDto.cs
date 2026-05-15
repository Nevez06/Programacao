namespace EventX.Api.DTOs.Social;

public sealed class PostCommentDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
