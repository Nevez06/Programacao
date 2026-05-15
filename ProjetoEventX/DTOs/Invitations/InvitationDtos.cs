using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Invitations
{
    public class InvitationTemplateDto
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
        public int OrganizerId { get; set; }
        public int? EventId { get; set; }
    }

    public class InvitationDraftDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int OrganizerId { get; set; }
        public int? TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LayoutJson { get; set; } = "{}";
        public string? PreviewHtml { get; set; }
        public string? PreviewUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SaveInvitationDraftDto
    {
        public int? Id { get; set; }

        [Required]
        public int EventId { get; set; }

        public int? TemplateId { get; set; }

        [StringLength(120)]
        public string? Name { get; set; }

        [Required]
        public string LayoutJson { get; set; } = "{}";

        public string? PreviewHtml { get; set; }
        public string? PreviewUrl { get; set; }
    }

    public class CreateInvitationFromTemplateDto
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [StringLength(120)]
        public string? Name { get; set; }
    }
}