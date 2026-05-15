using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class Notification
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1200)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Type { get; set; } = "sistema";

    [MaxLength(500)]
    public string? Link { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
