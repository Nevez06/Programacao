using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Stripe;

namespace ProjetoEventX.Models
{
    public class Pedido
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EventoId { get; set; }

        [ForeignKey("EventoId")]
        public required Event Evento { get; set; }

        [Required]
        public Guid ProdutoId { get; set; }

        [ForeignKey("ProdutoId")]
        public required Produto Produto { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantidade { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal PrecoTotal { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pendente"; // Pendente, Pago, Entregue

        public DateTime DataPedido { get; set; } = DateTime.Now;
    }
}