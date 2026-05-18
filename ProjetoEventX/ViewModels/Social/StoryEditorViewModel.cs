namespace ProjetoEventX.ViewModels.Social
{
    public class StoryEditorViewModel
    {
        public string MediaUrl { get; set; } = string.Empty;
        public string? TextOverlay { get; set; }
        public string? MentionsSerialized { get; set; }
        public string TextColor { get; set; } = "#ffffff";
        public bool TextBackground { get; set; }
        public int? EventoId { get; set; }
        public int? SharedPostId { get; set; }
        public FeedPostViewModel? SharedPost { get; set; }
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> EventosDisponiveis { get; set; } = new();
    }
}
