namespace ProjetoEventX.ViewModels.Social
{
    public class StoryAnalyticsViewModel
    {
        public int TotalStoriesPostados { get; set; }
        public int TotalVisualizacoes { get; set; }
        public int TotalReacoes { get; set; }
        public SocialStatusItemViewModel? StoryMaisVisto { get; set; }
        public SocialStatusItemViewModel? StoryMaisEngajado { get; set; }
        public List<StoryViewerViewModel> UltimosViewers { get; set; } = new();
        public List<StoryHighlightViewModel> Destaques { get; set; } = new();
    }
}
