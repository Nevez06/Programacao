using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class AvaliacaoFornecedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Nota { get; set; }

        [StringLength(1000)]
        public string? Comentario { get; set; }

        [Required]
        public int FornecedorId { get; set; }

        [ForeignKey("FornecedorId")]
        public Fornecedor? Fornecedor { get; set; }

        [Required]
        public int OrganizadorId { get; set; }

        [ForeignKey("OrganizadorId")]
        public Organizador? Organizador { get; set; }

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Evento? Evento { get; set; }

        public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
