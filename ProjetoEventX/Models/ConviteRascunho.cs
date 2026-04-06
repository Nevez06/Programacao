using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    [Table("ConvitesRascunhos")]
    public class ConviteRascunho
    {
        public int Id { get; set; }

        [Required]
        [Column("EventoId")]
        public int EventoId { get; set; }

        [Column("TemplateId")]
        public int? TemplateId { get; set; }

        [Required]
        [Column("OrganizadorId")]
        public int OrganizadorId { get; set; }

        [Column("NomeRascunho")]
        [MaxLength(120)]
        public string NomeRascunho { get; set; } = "Convite sem título";

        [Column("LayoutJson")]
        public string LayoutJson { get; set; } = "{}";

        [Column("PreviewHtml")]
        public string? PreviewHtml { get; set; }

        [Column("PreviewUrl")]
        public string? PreviewUrl { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Evento? Evento { get; set; }
        public virtual TemplateConvite? Template { get; set; }
    }
}
