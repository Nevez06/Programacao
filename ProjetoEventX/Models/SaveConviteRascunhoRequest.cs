using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class SaveConviteRascunhoRequest
    {
        public int? RascunhoId { get; set; }

        [Required]
        public int EventoId { get; set; }

        public int? TemplateId { get; set; }

        [Required]
        public string NomeRascunho { get; set; } = "Convite sem título";

        [Required]
        public string LayoutJson { get; set; } = "{}";

        public string? PreviewHtml { get; set; }
        public string? PreviewUrl { get; set; }
    }
}
