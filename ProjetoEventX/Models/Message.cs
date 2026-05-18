using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        public Conversation? Conversation { get; set; }

        [Required]
        public int SenderUserId { get; set; }

        public ApplicationUser? SenderUser { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EditedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
