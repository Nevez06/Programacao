namespace ProjetoEventX.Models
{
    public class StoryReply
    {
        public int Id { get; set; }
        public int StoryId { get; set; }
        public string FromUserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        public Story Story { get; set; } = null!;
    }
}
