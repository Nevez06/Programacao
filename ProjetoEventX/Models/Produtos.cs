using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Produto
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(255)]
        public string Nome { get; set; }

        public string Descricao { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Preco { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } // Produto, Local

        [Required]
        public Guid FornecedorId { get; set; }

        [ForeignKey("FornecedorId")]
        public Fornecedor Fornecedor { get; set; }

        // Relacionamento
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}