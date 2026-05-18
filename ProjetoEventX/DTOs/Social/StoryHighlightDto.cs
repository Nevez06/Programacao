namespace ProjetoEventX.DTOs.Social
{
    public class StoryHighlightDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? CapaUrl { get; set; }
        public int TotalStories { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<StoryDto> Stories { get; set; } = new();
    }
}