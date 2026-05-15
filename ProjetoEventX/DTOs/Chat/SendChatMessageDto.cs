using System.ComponentModel.DataAnnotations;

namespace ProjetoEventX.DTOs.Chat
{
    public class SendChatMessageDto
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}
