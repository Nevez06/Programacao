using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class Event
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Type { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public int? EstimatedAudience { get; set; }
    public decimal? EstimatedCost { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;

    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<InvitationTemplate> InvitationTemplates { get; set; } = new List<InvitationTemplate>();
    public ICollection<InvitationDraft> InvitationDrafts { get; set; } = new List<InvitationDraft>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
