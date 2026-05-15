using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class InvitationDraft
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    public int? TemplateId { get; set; }
    public InvitationTemplate? Template { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(32000)]
    public string LayoutJson { get; set; } = "{}";

    [MaxLength(16000)]
    public string? PreviewHtml { get; set; }

    [MaxLength(500)]
    public string? PreviewUrl { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}
