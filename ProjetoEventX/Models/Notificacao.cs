using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Notificacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MensagemNotificacao { get; set; }

        [StringLength(50)]
        public string Tipo { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;

        public bool Lida { get; set; } = false;

        [StringLength(50)]
        public string PrioridadeNotificacao { get; set; }

        [Required]
        public int DestinatarioId { get; set; }

        [ForeignKey("DestinatarioId")]
        public Pessoa Destinatario { get; set; }

        public int? EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Evento Evento { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}