namespace EventX.Api.DTOs.Invitations;

public sealed class InvitationDraftDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Guid OrganizerId { get; set; }
    public int? TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LayoutJson { get; set; } = "{}";
    public string? PreviewHtml { get; set; }
    public string? PreviewUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}
