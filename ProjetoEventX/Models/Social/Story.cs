namespace ProjetoEventX.Models
{
    public class Story
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? PerfilSocialId { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public string TextOverlay { get; set; } = string.Empty;
        public int? SharedPostId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool Ativo { get; set; } = true;
        public int? EventoId { get; set; }

        public Evento? Evento { get; set; }
        public PerfilSocial? PerfilSocial { get; set; }
        public SocialPost? SharedPost { get; set; }
        public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
        public ICollection<StoryReaction> Reactions { get; set; } = new List<StoryReaction>();
        public ICollection<StoryReply> Replies { get; set; } = new List<StoryReply>();
        public ICollection<StoryMention> Mentions { get; set; } = new List<StoryMention>();
        public ICollection<StoryHighlightItem> HighlightItems { get; set; } = new List<StoryHighlightItem>();
    }
}
