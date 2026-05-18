using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Invitations;

public sealed class SaveInvitationDraftRequestDto
{
    public int? Id { get; set; }

    [Required]
    public int EventId { get; set; }

    public int? TemplateId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [Required]
    [MaxLength(32000)]
    public string LayoutJson { get; set; } = "{}";

    [MaxLength(16000)]
    public string? PreviewHtml { get; set; }

    [MaxLength(500)]
    public string? PreviewUrl { get; set; }
}
