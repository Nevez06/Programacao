using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class TemplateEvento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public required string TituloTemplateEvento { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [StringLength(100)]
        public required string TipoEstilo { get; set; }

        [StringLength(100)]
        public required string Categoria { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
    }
}