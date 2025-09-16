using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class Local
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string NomeLocal { get; set; }

        [Required]
        [StringLength(255)]
        public string EnderecoLocal { get; set; }

        [Required]
        public int Capacidade { get; set; }

        [StringLength(100)]
        public string TipoLocal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
    }
}