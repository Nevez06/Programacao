using EventX.Api.Data;
using EventX.Api.DTOs.Social;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class SocialService : ISocialService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public SocialService(AppDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<FeedPostDto>> GetFeedAsync(
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var posts = await _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.User)
            .Include(x => x.AuthorProfile)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (posts.Count == 0)
        {
            return [];
        }

        var postIds = posts.Select(x => x.Id).ToArray();
        var likesByPost = await _dbContext.Likes
            .AsNoTracking()
            .Where(x => postIds.Contains(x.PostId))
            .GroupBy(x => x.PostId)
            .Select(x => new { PostId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, cancellationToken);

        var commentsByPost = await _dbContext.Comments
            .AsNoTracking()
            .Where(x => postIds.Contains(x.PostId))
            .GroupBy(x => x.PostId)
            .Select(x => new { PostId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, cancellationToken);

        var likedPostIds = await _dbContext.Likes
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && postIds.Contains(x.PostId))
            .Select(x => x.PostId)
            .ToHashSetAsync(cancellationToken);

        return posts
            .Select(x => MapFeedPost(
                x,
                likesByPost.GetValueOrDefault(x.Id),
                commentsByPost.GetValueOrDefault(x.Id),
                likedPostIds.Contains(x.Id)))
            .ToList();
    }

    public async Task<PostDetailsDto?> GetPostByIdAsync(
        int postId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var post = await _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.Id == postId && x.IsActive)
            .Include(x => x.User)
            .Include(x => x.AuthorProfile)
            .FirstOrDefaultAsync(cancellationToken);

        if (post is null)
        {
            return null;
        }

        var likesCount = await _dbContext.Likes
            .AsNoTracking()
            .CountAsync(x => x.PostId == postId, cancellationToken);

        var comments = await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.PostId == postId)
            .Include(x => x.User)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var isLiked = await _dbContext.Likes
            .AsNoTracking()
            .AnyAsync(x => x.PostId == postId && x.UserId == currentUserId, cancellationToken);

        return new PostDetailsDto
        {
            Id = post.Id,
            AuthorId = post.UserId,
            AuthorProfileId = post.AuthorProfileId,
            AuthorName = post.AuthorProfile?.DisplayName ?? post.User.FullName,
            AuthorUsername = post.AuthorProfile?.UserName,
            AuthorAvatarUrl = post.AuthorProfile?.AvatarUrl,
            Caption = post.Caption,
            Content = post.Caption,
            ImageUrl = post.ImageUrl,
            CreatedAtUtc = post.CreatedAt,
            CreatedAt = post.CreatedAt,
            LikesCount = likesCount,
            CommentsCount = comments.Count,
            IsLikedByCurrentUser = isLiked,
            Comments = comments.Select(MapComment).ToList()
        };
    }

    public async Task<PostDetailsDto?> CreatePostAsync(
        Guid currentUserId,
        CreatePostRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var caption = request.Caption?.Trim() ?? string.Empty;
        var imageUrl = request.ImageUrl?.Trim();
        if (string.IsNullOrWhiteSpace(caption) && string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var profile = await GetOrCreateSocialProfileAsync(user, cancellationToken);

        var post = new Post
        {
            UserId = currentUserId,
            AuthorProfileId = profile.Id,
            Caption = caption,
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetPostByIdAsync(post.Id, currentUserId, cancellationToken);
    }

    public async Task<ToggleLikeResponseDto?> LikePostAsync(
        int postId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var post = await _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.Id == postId && x.IsActive)
            .Select(x => new { x.Id, x.UserId })
            .FirstOrDefaultAsync(cancellationToken);
        if (post is null)
        {
            return null;
        }

        var existing = await _dbContext.Likes
            .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId, cancellationToken);

        if (existing is null)
        {
            _dbContext.Likes.Add(new Like
            {
                PostId = postId,
                UserId = currentUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (post.UserId != currentUserId)
            {
                await _notificationService.CreateForUserAsync(
                    post.UserId,
                    "social",
                    "Nova curtida",
                    "Seu post recebeu uma nova curtida.",
                    $"/social/post/{postId}",
                    cancellationToken);
            }
        }

        var likesCount = await _dbContext.Likes
            .AsNoTracking()
            .CountAsync(x => x.PostId == postId, cancellationToken);

        return new ToggleLikeResponseDto
        {
            PostId = postId,
            IsLiked = true,
            LikesCount = likesCount
        };
    }

    public async Task<ToggleLikeResponseDto?> UnlikePostAsync(
        int postId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var postExists = await _dbContext.Posts
            .AsNoTracking()
            .AnyAsync(x => x.Id == postId && x.IsActive, cancellationToken);
        if (!postExists)
        {
            return null;
        }

        var existing = await _dbContext.Likes
            .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == currentUserId, cancellationToken);

        if (existing is not null)
        {
            _dbContext.Likes.Remove(existing);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var likesCount = await _dbContext.Likes
            .AsNoTracking()
            .CountAsync(x => x.PostId == postId, cancellationToken);

        return new ToggleLikeResponseDto
        {
            PostId = postId,
            IsLiked = false,
            LikesCount = likesCount
        };
    }

    public async Task<ToggleLikeResponseDto?> ToggleLikeAsync(
        int postId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Likes
            .AsNoTracking()
            .AnyAsync(x => x.PostId == postId && x.UserId == currentUserId, cancellationToken);

        return existing
            ? await UnlikePostAsync(postId, currentUserId, cancellationToken)
            : await LikePostAsync(postId, currentUserId, cancellationToken);
    }

    public async Task<IReadOnlyList<PostCommentDto>?> GetCommentsAsync(
        int postId,
        CancellationToken cancellationToken)
    {
        var postExists = await _dbContext.Posts
            .AsNoTracking()
            .AnyAsync(x => x.Id == postId && x.IsActive, cancellationToken);
        if (!postExists)
        {
            return null;
        }

        var comments = await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.PostId == postId)
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(MapComment).ToList();
    }

    public async Task<PostCommentDto?> AddCommentAsync(
        int postId,
        Guid currentUserId,
        CreateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var content = request.Content.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var post = await _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.Id == postId && x.IsActive)
            .Select(x => new { x.Id, x.UserId })
            .FirstOrDefaultAsync(cancellationToken);
        if (post is null)
        {
            return null;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var profile = await _dbContext.SocialProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);

        var comment = new Comment
        {
            PostId = postId,
            UserId = currentUserId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Comments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (post.UserId != currentUserId)
        {
            var authorName = profile?.DisplayName ?? user.FullName;
            await _notificationService.CreateForUserAsync(
                post.UserId,
                "social",
                "Novo comentário",
                $"{authorName} comentou no seu post.",
                $"/social/post/{postId}",
                cancellationToken);
        }

        return new PostCommentDto
        {
            Id = comment.Id,
            UserId = currentUserId,
            AuthorName = profile?.DisplayName ?? user.FullName,
            AuthorAvatarUrl = profile?.AvatarUrl,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAt
        };
    }

    public async Task<IReadOnlyList<StoryDto>> GetActiveStoriesAsync(
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var stories = await _dbContext.Stories
            .AsNoTracking()
            .Where(x => x.IsActive && x.ExpiresAt > now)
            .Include(x => x.Author)
            .Include(x => x.AuthorProfile)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (stories.Count == 0)
        {
            return [];
        }

        var storyIds = stories.Select(x => x.Id).ToArray();
        var viewsByStory = await _dbContext.StoryViews
            .AsNoTracking()
            .Where(x => storyIds.Contains(x.StoryId))
            .GroupBy(x => x.StoryId)
            .Select(x => new { StoryId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.StoryId, x => x.Count, cancellationToken);

        var viewedStories = await _dbContext.StoryViews
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && storyIds.Contains(x.StoryId))
            .Select(x => x.StoryId)
            .ToHashSetAsync(cancellationToken);

        return stories
            .Select(x => MapStory(
                x,
                viewedStories.Contains(x.Id),
                viewsByStory.GetValueOrDefault(x.Id)))
            .ToList();
    }

    public async Task<StoryDto?> CreateStoryAsync(
        Guid currentUserId,
        CreateStoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var imageUrl = (request.ImageUrl ?? request.MediaUrl)?.Trim();
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        var profile = await GetOrCreateSocialProfileAsync(user, cancellationToken);

        var hoursToExpire = request.ExpiresInHours.GetValueOrDefault(24);
        if (hoursToExpire < 1)
        {
            hoursToExpire = 1;
        }

        var story = new Story
        {
            AuthorId = currentUserId,
            AuthorProfileId = profile.Id,
            ImageUrl = imageUrl,
            Caption = string.IsNullOrWhiteSpace(request.Caption) ? null : request.Caption.Trim(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(hoursToExpire),
            IsActive = true
        };

        _dbContext.Stories.Add(story);
        await _dbContext.SaveChangesAsync(cancellationToken);

        story.Author = user;
        story.AuthorProfile = profile;

        return MapStory(story, viewedByCurrentUser: false, viewsCount: 0);
    }

    public async Task<bool> MarkStoryAsViewedAsync(
        int storyId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var storyExists = await _dbContext.Stories
            .AsNoTracking()
            .AnyAsync(x => x.Id == storyId && x.IsActive && x.ExpiresAt > DateTime.UtcNow, cancellationToken);
        if (!storyExists)
        {
            return false;
        }

        var alreadyViewed = await _dbContext.StoryViews
            .AnyAsync(x => x.StoryId == storyId && x.UserId == currentUserId, cancellationToken);
        if (alreadyViewed)
        {
            return true;
        }

        _dbContext.StoryViews.Add(new StoryView
        {
            StoryId = storyId,
            UserId = currentUserId,
            ViewedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<StoryHighlightDto>> GetMyHighlightsAsync(
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.SocialProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);
        if (profile is null)
        {
            return [];
        }

        var now = DateTime.UtcNow;
        var highlights = await _dbContext.Highlights
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && x.ProfileId == profile.Id)
            .Include(x => x.Story)
                .ThenInclude(x => x.Author)
            .Include(x => x.Story)
                .ThenInclude(x => x.AuthorProfile)
            .Where(x => x.Story.IsActive && x.Story.ExpiresAt > now)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (highlights.Count == 0)
        {
            return [];
        }

        var grouped = highlights
            .GroupBy(x => (x.Title ?? "Destaque").Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var ordered = group
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Story.CreatedAt)
                    .ToList();
                var first = ordered[0];
                var stories = ordered
                    .Select(item => MapStory(item.Story, viewedByCurrentUser: false, viewsCount: 0))
                    .ToList();

                return new StoryHighlightDto
                {
                    Id = first.Id,
                    Nome = first.Title?.Trim() is { Length: > 0 } title ? title : "Destaque",
                    CapaUrl = first.CoverUrl ?? first.Story.ImageUrl,
                    TotalStories = stories.Count,
                    CreatedAt = first.CreatedAt,
                    Stories = stories
                };
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return grouped;
    }

    public async Task<StoryHighlightDto?> CreateHighlightAsync(
        Guid currentUserId,
        CreateHighlightRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.StoryIds.Count == 0)
        {
            return null;
        }

        var profile = await _dbContext.SocialProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var cleanedTitle = request.Nome.Trim();
        if (string.IsNullOrWhiteSpace(cleanedTitle))
        {
            return null;
        }

        var validStories = await _dbContext.Stories
            .AsNoTracking()
            .Where(x =>
                request.StoryIds.Contains(x.Id) &&
                x.AuthorId == currentUserId &&
                x.IsActive &&
                x.ExpiresAt > DateTime.UtcNow)
            .Include(x => x.Author)
            .Include(x => x.AuthorProfile)
            .ToListAsync(cancellationToken);

        if (validStories.Count == 0)
        {
            return null;
        }

        var existingStoryIds = await _dbContext.Highlights
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && request.StoryIds.Contains(x.StoryId))
            .Select(x => x.StoryId)
            .ToHashSetAsync(cancellationToken);

        var newHighlightRows = validStories
            .Where(story => !existingStoryIds.Contains(story.Id))
            .Select(story => new Highlight
            {
                UserId = currentUserId,
                ProfileId = profile.Id,
                StoryId = story.Id,
                Title = cleanedTitle,
                CoverUrl = string.IsNullOrWhiteSpace(request.CapaUrl) ? null : request.CapaUrl.Trim(),
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (newHighlightRows.Count > 0)
        {
            _dbContext.Highlights.AddRange(newHighlightRows);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var grouped = await GetMyHighlightsAsync(currentUserId, cancellationToken);
        return grouped.FirstOrDefault(x => string.Equals(x.Nome, cleanedTitle, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<SocialProfileDto?> GetMyProfileAsync(
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var profile = await GetOrCreateSocialProfileAsync(user, cancellationToken);
        var postsCount = await _dbContext.Posts
            .AsNoTracking()
            .CountAsync(x => x.UserId == currentUserId && x.IsActive, cancellationToken);

        var followersCount = await _dbContext.SocialFollows
            .AsNoTracking()
            .CountAsync(x => x.FollowingProfileId == profile.Id, cancellationToken);
        var followingCount = await _dbContext.SocialFollows
            .AsNoTracking()
            .CountAsync(x => x.FollowerUserId == currentUserId, cancellationToken);

        return MapSocialProfile(
            user,
            profile,
            postsCount,
            followersCount,
            followingCount,
            isFollowing: false);
    }

    public async Task<SocialProfileDto?> UpdateMyProfileAsync(
        Guid currentUserId,
        UpdateSocialProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == currentUserId && x.IsActive, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var profile = await GetOrCreateSocialProfileAsync(user, cancellationToken);

        var requestedUserName = request.UserName?.Trim().TrimStart('@');
        if (!string.IsNullOrWhiteSpace(requestedUserName))
        {
            var normalized = requestedUserName.ToLowerInvariant();
            var isDuplicated = await _dbContext.SocialProfiles
                .AnyAsync(x =>
                    x.Id != profile.Id &&
                    x.UserName.ToLower() == normalized, cancellationToken);
            if (isDuplicated)
            {
                return null;
            }

            profile.UserName = requestedUserName;
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            profile.DisplayName = request.DisplayName.Trim();
        }

        if (request.Bio is not null)
        {
            profile.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        }

        if (request.AvatarUrl is not null)
        {
            profile.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
                ? null
                : request.AvatarUrl.Trim();
        }

        if (request.City is not null)
        {
            profile.City = string.IsNullOrWhiteSpace(request.City)
                ? null
                : request.City.Trim();
        }

        if (request.Category is not null)
        {
            profile.Category = string.IsNullOrWhiteSpace(request.Category)
                ? null
                : request.Category.Trim();
        }

        if (request.Instagram is not null)
        {
            profile.Instagram = string.IsNullOrWhiteSpace(request.Instagram)
                ? null
                : request.Instagram.Trim();
        }

        if (request.Site is not null)
        {
            profile.Site = string.IsNullOrWhiteSpace(request.Site)
                ? null
                : request.Site.Trim();
        }

        profile.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var postsCount = await _dbContext.Posts
            .AsNoTracking()
            .CountAsync(x => x.UserId == currentUserId && x.IsActive, cancellationToken);

        var followersCount = await _dbContext.SocialFollows
            .AsNoTracking()
            .CountAsync(x => x.FollowingProfileId == profile.Id, cancellationToken);
        var followingCount = await _dbContext.SocialFollows
            .AsNoTracking()
            .CountAsync(x => x.FollowerUserId == currentUserId, cancellationToken);

        return MapSocialProfile(
            user,
            profile,
            postsCount,
            followersCount,
            followingCount,
            isFollowing: false);
    }

    public async Task<SocialProfileDto?> GetProfileByIdAsync(
        int profileId,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.SocialProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == profileId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var postsCount = await _dbContext.Posts
            .AsNoTracking()
            .CountAsync(x => x.AuthorProfileId == profileId && x.IsActive, cancellationToken);
        var followersCount = await _dbContext.SocialFollows
            .AsNoTracking()
            .CountAsync(x => x.FollowingProfileId == profile.Id, cancellationToken);
        var followingCount = await _dbContext.SocialFollows
            .AsNoTracking()
            .CountAsync(x => x.FollowerUserId == profile.UserId, cancellationToken);
        var isFollowing = false;
        if (currentUserId.HasValue && currentUserId.Value != profile.UserId)
        {
            isFollowing = await _dbContext.SocialFollows
                .AsNoTracking()
                .AnyAsync(
                    x => x.FollowerUserId == currentUserId.Value && x.FollowingProfileId == profile.Id,
                    cancellationToken);
        }

        return MapSocialProfile(
            profile.User,
            profile,
            postsCount,
            followersCount,
            followingCount,
            isFollowing);
    }

    public async Task<SocialProfileDto?> GetProfileByUserNameAsync(
        string userName,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var normalized = userName.Trim().TrimStart('@').ToLowerInvariant();
        var profile = await _dbContext.SocialProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserName.ToLower() == normalized, cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var postsCount = await _dbContext.Posts
            .AsNoTracking()
            .CountAsync(x => x.AuthorProfileId == profile.Id && x.IsActive, cancellationToken);

        var followersCount = await _dbContext.SocialFollows
            .AsNoTracking()
            .CountAsync(x => x.FollowingProfileId == profile.Id, cancellationToken);
        var followingCount = await _dbContext.SocialFollows
            .AsNoTracking()
            .CountAsync(x => x.FollowerUserId == profile.UserId, cancellationToken);

        var isFollowing = false;
        if (currentUserId.HasValue && currentUserId.Value != profile.UserId)
        {
            isFollowing = await _dbContext.SocialFollows
                .AsNoTracking()
                .AnyAsync(
                    x => x.FollowerUserId == currentUserId.Value && x.FollowingProfileId == profile.Id,
                    cancellationToken);
        }

        return MapSocialProfile(
            profile.User,
            profile,
            postsCount,
            followersCount,
            followingCount,
            isFollowing);
    }

    public async Task<IReadOnlyList<FeedPostDto>> GetPostsByProfileIdAsync(
        int profileId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var posts = await _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.AuthorProfileId == profileId && x.IsActive)
            .Include(x => x.User)
            .Include(x => x.AuthorProfile)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (posts.Count == 0)
        {
            return [];
        }

        var postIds = posts.Select(x => x.Id).ToArray();
        var likesByPost = await _dbContext.Likes
            .AsNoTracking()
            .Where(x => postIds.Contains(x.PostId))
            .GroupBy(x => x.PostId)
            .Select(x => new { PostId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, cancellationToken);

        var commentsByPost = await _dbContext.Comments
            .AsNoTracking()
            .Where(x => postIds.Contains(x.PostId))
            .GroupBy(x => x.PostId)
            .Select(x => new { PostId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, cancellationToken);

        var likedPostIds = await _dbContext.Likes
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && postIds.Contains(x.PostId))
            .Select(x => x.PostId)
            .ToHashSetAsync(cancellationToken);

        return posts
            .Select(x => MapFeedPost(
                x,
                likesByPost.GetValueOrDefault(x.Id),
                commentsByPost.GetValueOrDefault(x.Id),
                likedPostIds.Contains(x.Id)))
            .ToList();
    }

    public async Task<IReadOnlyList<StoryDto>> GetStoriesByProfileIdAsync(
        int profileId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var stories = await _dbContext.Stories
            .AsNoTracking()
            .Where(x => x.AuthorProfileId == profileId && x.IsActive && x.ExpiresAt > now)
            .Include(x => x.Author)
            .Include(x => x.AuthorProfile)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (stories.Count == 0)
        {
            return [];
        }

        var storyIds = stories.Select(x => x.Id).ToArray();
        var viewsByStory = await _dbContext.StoryViews
            .AsNoTracking()
            .Where(x => storyIds.Contains(x.StoryId))
            .GroupBy(x => x.StoryId)
            .Select(x => new { StoryId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.StoryId, x => x.Count, cancellationToken);

        var viewedStories = await _dbContext.StoryViews
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId && storyIds.Contains(x.StoryId))
            .Select(x => x.StoryId)
            .ToHashSetAsync(cancellationToken);

        return stories
            .Select(x => MapStory(
                x,
                viewedStories.Contains(x.Id),
                viewsByStory.GetValueOrDefault(x.Id)))
            .ToList();
    }

    public async Task<bool> FollowProfileAsync(
        int profileId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.SocialProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == profileId, cancellationToken);
        if (profile is null || profile.UserId == currentUserId)
        {
            return false;
        }

        var exists = await _dbContext.SocialFollows
            .AnyAsync(
                x => x.FollowerUserId == currentUserId && x.FollowingProfileId == profileId,
                cancellationToken);
        if (exists)
        {
            return true;
        }

        _dbContext.SocialFollows.Add(new SocialFollow
        {
            FollowerUserId = currentUserId,
            FollowingProfileId = profileId,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnfollowProfileAsync(
        int profileId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var follow = await _dbContext.SocialFollows
            .FirstOrDefaultAsync(
                x => x.FollowerUserId == currentUserId && x.FollowingProfileId == profileId,
                cancellationToken);

        if (follow is null)
        {
            return false;
        }

        _dbContext.SocialFollows.Remove(follow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<SocialProfileDto>> GetFollowersAsync(
        int profileId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.SocialProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == profileId, cancellationToken);
        if (profile is null)
        {
            return [];
        }

        var followerUserIds = await _dbContext.SocialFollows
            .AsNoTracking()
            .Where(x => x.FollowingProfileId == profileId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.FollowerUserId)
            .ToListAsync(cancellationToken);

        if (followerUserIds.Count == 0)
        {
            return [];
        }

        var profiles = await _dbContext.SocialProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => followerUserIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        if (profiles.Count == 0)
        {
            return [];
        }

        return await MapProfilesAsync(profiles, currentUserId, cancellationToken);
    }

    public async Task<IReadOnlyList<SocialProfileDto>> GetFollowingAsync(
        int profileId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.SocialProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == profileId, cancellationToken);
        if (profile is null)
        {
            return [];
        }

        var followingProfileIds = await _dbContext.SocialFollows
            .AsNoTracking()
            .Where(x => x.FollowerUserId == profile.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.FollowingProfileId)
            .ToListAsync(cancellationToken);

        if (followingProfileIds.Count == 0)
        {
            return [];
        }

        var profiles = await _dbContext.SocialProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => followingProfileIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (profiles.Count == 0)
        {
            return [];
        }

        return await MapProfilesAsync(profiles, currentUserId, cancellationToken);
    }

    public async Task<ExploreResponseDto> GetExploreAsync(
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var trendingPosts = await GetFeedAsync(currentUserId, cancellationToken);
        var topPosts = trendingPosts
            .OrderByDescending(x => (x.LikesCount * 2) + x.CommentsCount)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(20)
            .ToList();
        var recommendedPosts = trendingPosts
            .OrderByDescending(x => x.CommentsCount)
            .ThenByDescending(x => x.LikesCount)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(20)
            .ToList();

        var profiles = await _dbContext.SocialProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Take(200)
            .ToListAsync(cancellationToken);

        var profileIds = profiles.Select(x => x.Id).ToArray();
        var userIds = profiles.Select(x => x.UserId).ToArray();
        var postCounts = await _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.AuthorProfileId != null &&
                        profileIds.Contains(x.AuthorProfileId.Value) &&
                        x.IsActive)
            .GroupBy(x => x.AuthorProfileId!.Value)
            .Select(x => new { ProfileId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ProfileId, x => x.Count, cancellationToken);

        var followersByProfile = await _dbContext.SocialFollows
            .AsNoTracking()
            .Where(x => profileIds.Contains(x.FollowingProfileId))
            .GroupBy(x => x.FollowingProfileId)
            .Select(x => new { ProfileId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ProfileId, x => x.Count, cancellationToken);

        var followingByUser = await _dbContext.SocialFollows
            .AsNoTracking()
            .Where(x => userIds.Contains(x.FollowerUserId))
            .GroupBy(x => x.FollowerUserId)
            .Select(x => new { UserId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

        var followedByCurrentUser = await _dbContext.SocialFollows
            .AsNoTracking()
            .Where(x => x.FollowerUserId == currentUserId && profileIds.Contains(x.FollowingProfileId))
            .Select(x => x.FollowingProfileId)
            .ToHashSetAsync(cancellationToken);

        var topProfiles = profiles
            .Select(x => MapSocialProfile(
                x.User,
                x,
                postCounts.GetValueOrDefault(x.Id),
                followersByProfile.GetValueOrDefault(x.Id),
                followingByUser.GetValueOrDefault(x.UserId),
                followedByCurrentUser.Contains(x.Id)))
            .OrderByDescending(x => x.PostsCount)
            .ThenBy(x => x.DisplayName)
            .Take(20)
            .ToList();

        return new ExploreResponseDto
        {
            Categories = topPosts
                .Select(x => x.Content)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty)
                .Where(x => x.Length > 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToList(),
            TrendingPosts = topPosts,
            RecommendedPosts = recommendedPosts,
            TrendingProfiles = topProfiles
        };
    }

    private async Task<SocialProfile> GetOrCreateSocialProfileAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var profile = await _dbContext.SocialProfiles
            .FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        var baseUserName = BuildBaseUserName(user.Email, user.FullName);
        var candidate = baseUserName;
        var counter = 1;
        while (await _dbContext.SocialProfiles.AnyAsync(x => x.UserName.ToLower() == candidate, cancellationToken))
        {
            candidate = $"{baseUserName}{counter}";
            counter++;
        }

        profile = new SocialProfile
        {
            UserId = user.Id,
            UserName = candidate,
            DisplayName = user.FullName,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.SocialProfiles.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return profile;
    }

    private static string BuildBaseUserName(string email, string fullName)
    {
        var fromEmail = email.Split('@', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        var source = string.IsNullOrWhiteSpace(fromEmail) ? fullName : fromEmail;
        source = source.Trim().ToLowerInvariant().Replace(" ", ".");
        if (source.Length > 30)
        {
            source = source[..30];
        }

        return string.IsNullOrWhiteSpace(source) ? "eventx.user" : source;
    }

    private static FeedPostDto MapFeedPost(
        Post post,
        int likesCount,
        int commentsCount,
        bool isLiked)
    {
        return new FeedPostDto
        {
            Id = post.Id,
            AuthorId = post.UserId,
            AuthorProfileId = post.AuthorProfileId,
            AuthorName = post.AuthorProfile?.DisplayName ?? post.User.FullName,
            AuthorUsername = post.AuthorProfile?.UserName,
            AuthorAvatarUrl = post.AuthorProfile?.AvatarUrl,
            Caption = post.Caption,
            Content = post.Caption,
            ImageUrl = post.ImageUrl,
            CreatedAtUtc = post.CreatedAt,
            CreatedAt = post.CreatedAt,
            LikesCount = likesCount,
            CommentsCount = commentsCount,
            IsLikedByCurrentUser = isLiked,
            IsLiked = isLiked
        };
    }

    private static PostCommentDto MapComment(Comment comment)
    {
        return new PostCommentDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            AuthorName = comment.User.FullName,
            AuthorAvatarUrl = null,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAt
        };
    }

    private static StoryDto MapStory(
        Story story,
        bool viewedByCurrentUser,
        int viewsCount)
    {
        return new StoryDto
        {
            Id = story.Id,
            AuthorId = story.AuthorId,
            AuthorProfileId = story.AuthorProfileId,
            AuthorName = story.AuthorProfile?.DisplayName ?? story.Author.FullName,
            AuthorUserName = story.AuthorProfile?.UserName,
            AuthorAvatarUrl = story.AuthorProfile?.AvatarUrl,
            ImageUrl = story.ImageUrl,
            MediaUrl = story.ImageUrl,
            Caption = story.Caption,
            CreatedAtUtc = story.CreatedAt,
            ExpiresAtUtc = story.ExpiresAt,
            CreatedAt = story.CreatedAt,
            ExpiresAt = story.ExpiresAt,
            ViewedByCurrentUser = viewedByCurrentUser,
            Viewed = viewedByCurrentUser,
            ViewsCount = viewsCount
        };
    }

    private static SocialProfileDto MapSocialProfile(
        User user,
        SocialProfile profile,
        int postsCount,
        int followersCount,
        int followingCount,
        bool isFollowing)
    {
        return new SocialProfileDto
        {
            Id = profile.Id,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            UserName = profile.UserName,
            DisplayName = profile.DisplayName,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            City = profile.City,
            Category = profile.Category ?? user.UserType.ToString(),
            Instagram = profile.Instagram,
            Site = profile.Site,
            FollowersCount = followersCount,
            FollowingCount = followingCount,
            IsFollowing = isFollowing,
            PostsCount = postsCount
        };
    }

    private async Task<IReadOnlyList<SocialProfileDto>> MapProfilesAsync(
        IReadOnlyList<SocialProfile> profiles,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var profileIds = profiles.Select(x => x.Id).ToArray();
        var userIds = profiles.Select(x => x.UserId).ToArray();

        var postsByProfile = await _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.AuthorProfileId != null && profileIds.Contains(x.AuthorProfileId.Value) && x.IsActive)
            .GroupBy(x => x.AuthorProfileId!.Value)
            .Select(x => new { ProfileId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ProfileId, x => x.Count, cancellationToken);

        var followersByProfile = await _dbContext.SocialFollows
            .AsNoTracking()
            .Where(x => profileIds.Contains(x.FollowingProfileId))
            .GroupBy(x => x.FollowingProfileId)
            .Select(x => new { ProfileId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ProfileId, x => x.Count, cancellationToken);

        var followingByUser = await _dbContext.SocialFollows
            .AsNoTracking()
            .Where(x => userIds.Contains(x.FollowerUserId))
            .GroupBy(x => x.FollowerUserId)
            .Select(x => new { UserId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

        var followedByCurrentUser = await _dbContext.SocialFollows
            .AsNoTracking()
            .Where(x => x.FollowerUserId == currentUserId && profileIds.Contains(x.FollowingProfileId))
            .Select(x => x.FollowingProfileId)
            .ToHashSetAsync(cancellationToken);

        return profiles
            .OrderBy(x => x.DisplayName)
            .Select(profile => MapSocialProfile(
                profile.User,
                profile,
                postsByProfile.GetValueOrDefault(profile.Id),
                followersByProfile.GetValueOrDefault(profile.Id),
                followingByUser.GetValueOrDefault(profile.UserId),
                followedByCurrentUser.Contains(profile.Id)))
            .ToList();
    }
}
