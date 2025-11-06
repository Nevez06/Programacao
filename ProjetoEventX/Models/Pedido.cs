using Stripe;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Pedido
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int EventoId { get; set; }

        [ForeignKey("EventoId")]
<<<<<<< HEAD:ProjetoEventX/Models/Pedido.cs
        public required Evento Evento { get; set; }
=======
        public Evento Evento { get; set; }
>>>>>>> 9c557d6 (feat: adiciona controladores e atualiza modelos e views):ProjetoEventX/Models/Pedidos.cs

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
        public string StatusPedido { get; set; } = "Pendente"; // Pendente, Pago, Entregue

        public DateTime DataPedido { get; set; } = DateTime.Now;
    }
}