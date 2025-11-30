using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ProjetoEventX.Models
{
    public class Fornecedor : IdentityUser<int>
    {
        [Required]
        public int PessoaId { get; set; }

        [ForeignKey("PessoaId")]
        public required Pessoa Pessoa { get; set; }

        [Required]
        [StringLength(18)]
        public required string Cnpj { get; set; }

        [StringLength(255)]
        public required string TipoServico { get; set; }

        
        [Required]
        [StringLength(100)]
        public required string Cidade { get; set; }

        [Required]
        [StringLength(2)] 
        public required string UF { get; set; }
        // ------------------------------------

        public decimal AvaliacaoMedia { get; set; } = 0.0m;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relacionamentos
        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}