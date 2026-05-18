using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    [Table("TemplatesConvites")]
    public class TemplateConvite
    {
        public int Id { get; set; }

        [Required]
        [Column("NomeTemplate")]
        public string Nome { get; set; } = string.Empty;

        [Column("EstiloLayout")]
        public string? Estilo { get; set; }
        public bool Ativo { get; set; } = true;

        [Column("CorFundo")]
        public string? CorFundo { get; set; }
        [Column("CorTexto")]
        public string? CorTexto { get; set; }
        [Column("CorPrimaria")]
        public string? CorPrimaria { get; set; }

        [Column("FonteTexto")]
        public string? Fonte { get; set; }

        [Column("TituloConvite")]
        public string? Titulo { get; set; }
        [NotMapped]
        public string? Saudacao { get; set; } = "Olá!";

        [Column("MensagemPrincipal")]
        public string? Mensagem { get; set; }
        [NotMapped]
        public string? TextoBotao { get; set; } = "Confirmar Presença";

        [Column("EventoId")]
        public int? EventoId { get; set; }
        [Column("OrganizadorId")]
        public int OrganizadorId { get; set; }

        [Column("PadraoSistema")]
        public bool PadraoSistema { get; set; } = false;

        [Column("TamanhoFonteTitulo")]
        public int TamanhoFonteTitulo { get; set; } = 34;

        [Column("TamanhoFonteTexto")]
        public int TamanhoFonteTexto { get; set; } = 18;

        [Column("MostrarLogo")]
        public bool MostrarLogo { get; set; } = false;

        [Column("MostrarFotoEvento")]
        public bool MostrarFotoEvento { get; set; } = false;

        [Column("MostrarMapa")]
        public bool MostrarMapa { get; set; } = true;

        [Column("MostrarQRCode")]
        public bool MostrarQRCode { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Evento? Evento { get; set; }

        [Column("CSSPersonalizado")]
        public string? LayoutJson { get; set; }
    }
}
