namespace ProjetoEventX.DTOs.Chat
{
    public class ChatConversationDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int ParticipantCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ChatMessageDto? LastMessage { get; set; }
    }
}
