using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class SocialProfile
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(120)]
    public string? City { get; set; }

    [MaxLength(80)]
    public string? Category { get; set; }

    [MaxLength(120)]
    public string? Instagram { get; set; }

    [MaxLength(200)]
    public string? Site { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Highlight> Highlights { get; set; } = new List<Highlight>();
    public ICollection<SocialFollow> Followers { get; set; } = new List<SocialFollow>();
}
