using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    [Table("Quotes")]
    public class Quote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Evento? Event { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Fornecedor? Supplier { get; set; }

        [Required]
        public int OrganizadorId { get; set; }

        [ForeignKey("OrganizadorId")]
        public Organizador? Organizador { get; set; }

        [Required]
        [StringLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 10000000)]
        public decimal EstimatedValue { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pendente";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(2000)]
        public string? ResponseMessage { get; set; }

        [Range(0, 10000000)]
        public decimal? ResponseValue { get; set; }

        public DateTime? ResponseDate { get; set; }

        public Guid? PedidoGeradoId { get; set; }

        [ForeignKey("PedidoGeradoId")]
        public Pedido? PedidoGerado { get; set; }
    }
}
