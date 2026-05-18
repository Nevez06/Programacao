namespace ProjetoEventX.ViewModels.Social
{
    public class StoryHighlightViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CapaUrl { get; set; } = "/uploads/social/defaults/default-post.svg";
        public int TotalStories { get; set; }
        public int? PrimeiroStoryId { get; set; }
        public int? PerfilId { get; set; }
    }
}
