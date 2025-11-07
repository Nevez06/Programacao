using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Pedido
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
        public Evento? Evento { get; set; } // ✅ Tornado anulável (ou use = default!)

        [Required]
        public Guid ProdutoId { get; set; }

        [ForeignKey("ProdutoId")]
        public Produto? Produto { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantidade { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal PrecoTotal { get; set; }

        [StringLength(50)]
        public string StatusPedido { get; set; } = "Pendente"; // Pendente, Pago, Entregue

        public DateTime DataPedido { get; set; } = DateTime.Now;
    }
}
