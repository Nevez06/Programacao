using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ProjetoEventX.Models
{
    public class Convidado : IdentityUser<int>
    {
        [Required]
        public int PessoaId { get; set; }

        [ForeignKey("PessoaId")]
        public required Pessoa Pessoa { get; set; }

        [StringLength(50)]
        public string ConfirmaPresenca { get; set; } = "Pendente";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public ICollection<ListaConvidado> ListasConvidados { get; set; } = new List<ListaConvidado>();
    }
}