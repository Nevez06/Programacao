using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Stripe;

namespace ProjetoEventX.Models
{
    public class Despesa
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Event Evento { get; set; }

        [Required]
        public string Descricao { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Valor { get; set; }

        public DateTime DataDespesa { get; set; } = DateTime.Now;
    }
}