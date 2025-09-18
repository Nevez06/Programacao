using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class ListaConvidado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConvidadoId { get; set; }

        [ForeignKey("ConvidadoId")]
        public required Convidado Convidado { get; set; }

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public required Evento Evento { get; set; }

        public DateTime DataInclusao { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string ConfirmaPresenca { get; set; } = "Pendente";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}