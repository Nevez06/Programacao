namespace ProjetoEventX.DTOs.Chat
{
    public class ChatMessageDto
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public int SenderUserId { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsMine { get; set; }
    }
}
