using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public required string AvaliacaoFeedback { get; set; }

        public string? ComentarioFeedback { get; set; }

        public DateTime DataEnvioFeedback { get; set; } = DateTime.Now;

        [StringLength(50)]
        public required string TipoFeedback { get; set; }

        public int? FornecedorId { get; set; }

        [ForeignKey("FornecedorId")]
        public  Fornecedor? Fornecedor { get; set; }

        public int? EventoId { get; set; }

        [ForeignKey("EventoId")]
        public required Evento Evento { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}