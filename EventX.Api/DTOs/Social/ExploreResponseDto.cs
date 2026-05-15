namespace EventX.Api.DTOs.Social;

public sealed class ExploreResponseDto
{
    public IReadOnlyList<string> Categories { get; set; } = [];
    public IReadOnlyList<FeedPostDto> TrendingPosts { get; set; } = [];
    public IReadOnlyList<FeedPostDto> RecommendedPosts { get; set; } = [];
    public IReadOnlyList<SocialProfileDto> TrendingProfiles { get; set; } = [];
}
