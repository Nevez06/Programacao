using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    public class Pagamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal ValorTotal { get; set; }

        [StringLength(50)]
        public string StatusPagamento { get; set; } = "Pendente";

        [StringLength(50)]
        public string MetodoPagamento { get; set; }

        public DateTime? DataPagamento { get; set; }

        [StringLength(255)]
        public string Comprovante { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}