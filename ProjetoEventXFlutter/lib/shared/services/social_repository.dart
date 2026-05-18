import 'package:projeto_eventx_flutter/features/feed/data/feed_repository.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/social_explore/data/models/social_explore_model.dart';
import 'package:projeto_eventx_flutter/features/social_explore/data/social_explore_repository.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/models/social_profile_model.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/models/update_social_profile_model.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/social_profile_repository.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/social_story_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/story_highlight_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/social_story_repository.dart';
import 'package:projeto_eventx_flutter/shared/models/content_intelligence_models.dart';
import 'package:projeto_eventx_flutter/shared/services/feed_ranking_service.dart';

class SocialRepository {
  SocialRepository(
    this._feedRepository, {
    required SocialStoryRepository storyRepository,
    required SocialProfileRepository profileRepository,
    required SocialExploreRepository exploreRepository,
    FeedRankingService rankingService = const FeedRankingService(),
  })  : _storyRepository = storyRepository,
        _profileRepository = profileRepository,
        _exploreRepository = exploreRepository,
        _rankingService = rankingService;

  final FeedRepository _feedRepository;
  final SocialStoryRepository _storyRepository;
  final SocialProfileRepository _profileRepository;
  final SocialExploreRepository _exploreRepository;
  final FeedRankingService _rankingService;

  Future<List<PostModel>> getFeed() => _feedRepository.getPosts();
  Future<void> toggleLike(int postId, {required bool currentlyLiked}) async {
    await _feedRepository.toggleLike(postId, curtir: !currentlyLiked);
  }

  Future<List<SocialStoryModel>> getStories({int take = 100}) =>
      _storyRepository.getStories(take: take);

  Future<SocialStoryModel> createStory(CreateSocialStoryRequest request) =>
      _storyRepository.createStory(request);

  Future<void> markStoryAsViewed(int storyId) =>
      _storyRepository.markAsViewed(storyId);

  Future<SocialProfileModel> getMyProfile() => _profileRepository.getMe();

  Future<SocialProfileModel> updateMyProfile(UpdateSocialProfileModel model) =>
      _profileRepository.updateMe(model);

  Future<SocialProfileModel> getProfileById(int profileId) =>
      _profileRepository.getById(profileId);

  Future<SocialProfileModel> getProfileByUsername(String username) =>
      _profileRepository.getByUsername(username);

  Future<List<PostModel>> getPostsByProfileId(int profileId,
          {int take = 120}) =>
      _profileRepository.getPostsByProfileId(profileId, take: take);

  Future<List<SocialStoryModel>> getStoriesByProfileId(
    int profileId, {
    int take = 60,
  }) =>
      _profileRepository.getStoriesByProfileId(profileId, take: take);

  Future<void> followProfile(int profileId) =>
      _profileRepository.follow(profileId);

  Future<void> unfollowProfile(int profileId) =>
      _profileRepository.unfollow(profileId);

  Future<List<StoryHighlightModel>> getMyHighlights() async {
    return _storyRepository.getMyHighlights();
  }

  Future<SocialExploreModel> getExplore({int take = 24}) =>
      _exploreRepository.getExplore(take: take);

  Future<List<RankedPost>> getRankedFeed({
    FeedRankingContext context = const FeedRankingContext(),
    FeedSortMode mode = FeedSortMode.forYou,
  }) async {
    final posts = await _feedRepository.getPosts();
    return _rankingService.rankFeedPosts(posts, context, mode: mode);
  }

  List<RankedStory> rankStories(
    List<StoryCandidate> stories, {
    StoryRankingContext context = const StoryRankingContext(),
  }) {
    return _rankingService.rankStories(stories, context);
  }
}
