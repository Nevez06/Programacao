namespace ProjetoEventX.DTOs.Social
{
    public class StoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? PerfilSocialId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public string? Theme { get; set; }
        public int? EventId { get; set; }
        public string? EventName { get; set; }
        public int? SharedPostId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool ViewedByCurrentUser { get; set; }
        public int ViewsCount { get; set; }
    }
}