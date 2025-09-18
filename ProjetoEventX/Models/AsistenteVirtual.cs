using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class AssistenteVirtual
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public required string AlgoritmoIA { get; set; }

        public required string Sugestoes { get; set; }

        public DateTime DataGeracao { get; set; } = DateTime.Now;

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public required Evento Evento { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}