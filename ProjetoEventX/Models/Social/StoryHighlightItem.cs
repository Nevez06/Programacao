namespace ProjetoEventX.Models
{
    public class StoryHighlightItem
    {
        public int Id { get; set; }
        public int HighlightId { get; set; }
        public int StoryId { get; set; }
        public int Ordem { get; set; }

        public StoryHighlight Highlight { get; set; } = null!;
        public Story Story { get; set; } = null!;
    }
}
