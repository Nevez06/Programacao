using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class ConversationParticipant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        public Conversation? Conversation { get; set; }

        [Required]
        public int UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LeftAt { get; set; }
    }
}
