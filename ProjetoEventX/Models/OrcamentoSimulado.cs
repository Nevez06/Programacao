using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class OrcamentoSimulado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Evento? Evento { get; set; }

        [Required]
        [StringLength(100)]
        public required string Categoria { get; set; }

        [Required]
        [Range(0, 10000000)]
        public decimal ValorEstimado { get; set; }

        public DateTime DataSimulacao { get; set; } = DateTime.UtcNow;
    }
}
