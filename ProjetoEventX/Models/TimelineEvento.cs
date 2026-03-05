using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class TimelineEvento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Evento? Evento { get; set; }

        [Required]
        [StringLength(255)]
        public required string Titulo { get; set; }

        [Required]
        public int DiasAntesEvento { get; set; }

        public bool Concluido { get; set; } = false;

        [StringLength(50)]
        public string? Categoria { get; set; }

        public int Ordem { get; set; } = 0;
    }
}
