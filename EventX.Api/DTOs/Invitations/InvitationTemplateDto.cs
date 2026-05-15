namespace EventX.Api.DTOs.Invitations;

public sealed class InvitationTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Style { get; set; }
    public string? BackgroundColor { get; set; }
    public string? PrimaryColor { get; set; }
    public string? TextColor { get; set; }
    public string? Font { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? PreviewUrl { get; set; }
    public bool DefaultSystem { get; set; }
    public Guid? OrganizerId { get; set; }
    public int? EventId { get; set; }
}
