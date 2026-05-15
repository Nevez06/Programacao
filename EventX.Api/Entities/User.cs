using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserType UserType { get; set; } = UserType.Guest;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public SocialProfile? SocialProfile { get; set; }
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<StoryView> StoryViews { get; set; } = new List<StoryView>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Highlight> Highlights { get; set; } = new List<Highlight>();
    public ICollection<InvitationTemplate> InvitationTemplates { get; set; } = new List<InvitationTemplate>();
    public ICollection<InvitationDraft> InvitationDrafts { get; set; } = new List<InvitationDraft>();
    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<SocialFollow> FollowingProfiles { get; set; } = new List<SocialFollow>();
}
