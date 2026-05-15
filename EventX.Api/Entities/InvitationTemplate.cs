using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class InvitationTemplate
{
    public int Id { get; set; }

    [Required]
    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Style { get; set; }

    [MaxLength(20)]
    public string? BackgroundColor { get; set; }

    [MaxLength(20)]
    public string? PrimaryColor { get; set; }

    [MaxLength(20)]
    public string? TextColor { get; set; }

    [MaxLength(80)]
    public string? Font { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1200)]
    public string? Message { get; set; }

    [MaxLength(500)]
    public string? PreviewUrl { get; set; }

    public bool DefaultSystem { get; set; }

    public Guid? OrganizerId { get; set; }
    public User? Organizer { get; set; }

    public int? EventId { get; set; }
    public Event? Event { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<InvitationDraft> Drafts { get; set; } = new List<InvitationDraft>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
}
