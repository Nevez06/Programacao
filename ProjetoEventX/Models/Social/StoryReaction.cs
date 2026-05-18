namespace ProjetoEventX.Models
{
    public class StoryReaction
    {
        public int Id { get; set; }
        public int StoryId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ReactionType { get; set; } = "like";
        public DateTime CreatedAt { get; set; }

        public Story Story { get; set; } = null!;
    }
}
