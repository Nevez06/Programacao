namespace ProjetoEventX.Models
{
    public class StoryHighlight
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string? CapaUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<StoryHighlightItem> Items { get; set; } = new List<StoryHighlightItem>();
    }
}
