using EventX.Api.DTOs.Social;

namespace EventX.Api.Services;

public interface ISocialService
{
    Task<IReadOnlyList<FeedPostDto>> GetFeedAsync(Guid currentUserId, CancellationToken cancellationToken);
    Task<PostDetailsDto?> GetPostByIdAsync(int postId, Guid currentUserId, CancellationToken cancellationToken);
    Task<PostDetailsDto?> CreatePostAsync(Guid currentUserId, CreatePostRequestDto request, CancellationToken cancellationToken);
    Task<ToggleLikeResponseDto?> LikePostAsync(int postId, Guid currentUserId, CancellationToken cancellationToken);
    Task<ToggleLikeResponseDto?> UnlikePostAsync(int postId, Guid currentUserId, CancellationToken cancellationToken);
    Task<ToggleLikeResponseDto?> ToggleLikeAsync(int postId, Guid currentUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PostCommentDto>?> GetCommentsAsync(int postId, CancellationToken cancellationToken);
    Task<PostCommentDto?> AddCommentAsync(int postId, Guid currentUserId, CreateCommentRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<StoryDto>> GetActiveStoriesAsync(Guid currentUserId, CancellationToken cancellationToken);
    Task<StoryDto?> CreateStoryAsync(Guid currentUserId, CreateStoryRequestDto request, CancellationToken cancellationToken);
    Task<bool> MarkStoryAsViewedAsync(int storyId, Guid currentUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StoryHighlightDto>> GetMyHighlightsAsync(Guid currentUserId, CancellationToken cancellationToken);
    Task<StoryHighlightDto?> CreateHighlightAsync(Guid currentUserId, CreateHighlightRequestDto request, CancellationToken cancellationToken);
    Task<SocialProfileDto?> GetMyProfileAsync(Guid currentUserId, CancellationToken cancellationToken);
    Task<SocialProfileDto?> UpdateMyProfileAsync(Guid currentUserId, UpdateSocialProfileRequestDto request, CancellationToken cancellationToken);
    Task<SocialProfileDto?> GetProfileByIdAsync(int profileId, Guid? currentUserId, CancellationToken cancellationToken);
    Task<SocialProfileDto?> GetProfileByUserNameAsync(string userName, Guid? currentUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FeedPostDto>> GetPostsByProfileIdAsync(int profileId, Guid currentUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StoryDto>> GetStoriesByProfileIdAsync(int profileId, Guid currentUserId, CancellationToken cancellationToken);
    Task<bool> FollowProfileAsync(int profileId, Guid currentUserId, CancellationToken cancellationToken);
    Task<bool> UnfollowProfileAsync(int profileId, Guid currentUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SocialProfileDto>> GetFollowersAsync(int profileId, Guid currentUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SocialProfileDto>> GetFollowingAsync(int profileId, Guid currentUserId, CancellationToken cancellationToken);
    Task<ExploreResponseDto> GetExploreAsync(Guid currentUserId, CancellationToken cancellationToken);
}
