using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ProjetoEventX.Models
{
    public class Organizador : IdentityUser<int>
    {
        [Required]
        public int PessoaId { get; set; }

        [ForeignKey("PessoaId")]
        public required Pessoa Pessoa { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
        public ICollection<Administracao> Administracoes { get; set; } = new List<Administracao>();
    }
}