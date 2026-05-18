using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class Post
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int? AuthorProfileId { get; set; }
    public SocialProfile? AuthorProfile { get; set; }

    [MaxLength(1000)]
    public string Caption { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
