namespace ProjetoEventX.Models
{
    public class StoryView
    {
        public int Id { get; set; }
        public int StoryId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime ViewedAt { get; set; }

        public Story Story { get; set; } = null!;
    }
}
