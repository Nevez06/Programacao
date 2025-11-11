using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Despesa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public required Evento Evento { get; set; }

        [Required]
        public required string Descricao { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Valor { get; set; }

        public DateTime DataDespesa { get; set; } = DateTime.Now;
    }
}