using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class Highlight
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProfileId { get; set; }
    public SocialProfile Profile { get; set; } = null!;

    public int StoryId { get; set; }
    public Story Story { get; set; } = null!;

    [MaxLength(120)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? CoverUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
