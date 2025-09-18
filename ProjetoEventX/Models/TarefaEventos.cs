using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class TarefaEvento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? DescricaoTarefaEvento { get; set; }

        public int? ResponsavelId { get; set; }

        [ForeignKey("ResponsavelId")]
        public Pessoa? Responsavel { get; set; }

        [StringLength(50)]
        public string StatusConclusao { get; set; } = "Pendente";

        public DateTime? DataLimite { get; set; }

        [StringLength(50)]
        public string? PrioridadeTarefaEvento { get; set; }

        public DateTime? DataConclusao { get; set; }

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public required Evento Evento { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}