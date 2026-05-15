namespace ProjetoEventX.DTOs.Social
{
    public class SocialPostSummaryDto
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorAvatar { get; set; }
        public int? EventId { get; set; }
        public string? EventName { get; set; }
        public string? ImageUrl { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? ContentType { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int SharesCount { get; set; }
        public int ViewsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsSavedByCurrentUser { get; set; }
    }
}