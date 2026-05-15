namespace ProjetoEventX.DTOs.Social
{
    public class ExploreResponseDto
    {
        public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();
        public IReadOnlyList<SocialPostSummaryDto> TrendingPosts { get; set; } = Array.Empty<SocialPostSummaryDto>();
        public IReadOnlyList<SocialProfileDto> TrendingProfiles { get; set; } = Array.Empty<SocialProfileDto>();
        public IReadOnlyList<SocialPostSummaryDto> RecommendedPosts { get; set; } = Array.Empty<SocialPostSummaryDto>();
    }
}