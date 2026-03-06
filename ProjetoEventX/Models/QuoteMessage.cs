using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoEventX.Models
{
    [Table("QuoteMessages")]
    public class QuoteMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuoteId { get; set; }

        [ForeignKey("QuoteId")]
        public Quote? Quote { get; set; }

        [Required]
        public int SenderUserId { get; set; }

        [Required]
        [StringLength(30)]
        public string SenderType { get; set; } = string.Empty; // "Organizador" ou "Fornecedor"

        [Required]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
