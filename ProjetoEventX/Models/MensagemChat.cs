using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Stripe;

namespace ProjetoEventX.Models
{
    public class MensagemChat
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Event Evento { get; set; }

        [Required]
        public Guid ConvidadoId { get; set; }

        [ForeignKey("ConvidadoId")]
        public Convidado Convidado { get; set; }

        [Required]
        public string Mensagem { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;
    }
}