using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class SolicitacaoOrcamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrganizadorId { get; set; }

        [ForeignKey("OrganizadorId")]
        public Organizador? Organizador { get; set; }

        [Required]
        public int FornecedorId { get; set; }

        [ForeignKey("FornecedorId")]
        public Fornecedor? Fornecedor { get; set; }

        public int? EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Evento? Evento { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(100)]
        public string? TipoServicoDesejado { get; set; }

        public DateTime? DataEvento { get; set; }

        [StringLength(200)]
        public string? LocalEvento { get; set; }

        [Range(0, 10000000)]
        public decimal? OrcamentoEstimado { get; set; }

        [Range(1, 100000)]
        public int? QuantidadeConvidados { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pendente"; // Pendente, Respondido, Aceito, Recusado, Cancelado

        [StringLength(2000)]
        public string? RespostaFornecedor { get; set; }

        [Range(0, 10000000)]
        public decimal? ValorProposto { get; set; }

        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataResposta { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
