using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class Story
{
    public int Id { get; set; }

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public int? AuthorProfileId { get; set; }
    public SocialProfile? AuthorProfile { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Caption { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Highlight> Highlights { get; set; } = new List<Highlight>();
    public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
}
