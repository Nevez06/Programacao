using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class TemplateEvento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string TituloTemplateEvento { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string TipoEstilo { get; set; }

        [StringLength(100)]
        public string Categoria { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
    }
}