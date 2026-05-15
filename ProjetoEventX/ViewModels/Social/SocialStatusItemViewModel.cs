namespace ProjetoEventX.ViewModels.Social
{
    public class SocialStatusItemViewModel
    {
        public int Id { get; set; }
        public int PerfilId { get; set; }
        public int UserId { get; set; }
        public string NomePerfil { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string ImagemUrl { get; set; } = string.Empty;
        public string? TextoOverlay { get; set; }
        public DateTime CriadoEm { get; set; }
        public bool Visualizado { get; set; }
        public int SequenciaNoPerfil { get; set; }
        public int TotalNoPerfil { get; set; }
        public int? EventoId { get; set; }
        public string? NomeEvento { get; set; }
        public int? SharedPostId { get; set; }
        public string? SharedPostImageUrl { get; set; }
        public string? SharedPostCaption { get; set; }
        public int TotalViews { get; set; }
        public int TotalReactions { get; set; }
        public int EngagementScore { get; set; }
        public decimal RankingScore { get; set; }
        public bool IsOwner { get; set; }
        public string? UserReaction { get; set; }
        public bool CanReply { get; set; }
        public List<StoryMentionItemViewModel> Mentions { get; set; } = new();
        public Dictionary<string, int> ReactionsByType { get; set; } = new();
    }

    public class StoryMentionItemViewModel
    {
        public int PerfilId { get; set; }
        public string MentionText { get; set; } = string.Empty;
    }
}
