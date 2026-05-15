namespace ProjetoEventX.Models
{
    public class StoryMention
    {
        public int Id { get; set; }
        public int StoryId { get; set; }
        public string MentionedUserId { get; set; } = string.Empty;
        public int MentionedPerfilId { get; set; }
        public string MentionText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public Story Story { get; set; } = null!;
    }
}
