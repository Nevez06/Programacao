using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        public int? CreatedByUserId { get; set; }

        public ApplicationUser? CreatedByUser { get; set; }

        public bool IsGroup { get; set; }

        public bool IsArchived { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
