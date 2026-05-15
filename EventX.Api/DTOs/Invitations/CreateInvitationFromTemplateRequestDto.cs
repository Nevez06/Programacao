using System.ComponentModel.DataAnnotations;

namespace EventX.Api.DTOs.Invitations;

public sealed class CreateInvitationFromTemplateRequestDto
{
    [Required]
    public int EventId { get; set; }

    [Required]
    public int TemplateId { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }
}
