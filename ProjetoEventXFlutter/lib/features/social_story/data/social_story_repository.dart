import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/social_story_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/story_highlight_model.dart';

class SocialStoryRepository {
  SocialStoryRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<SocialStoryModel>> getStories({int take = 60}) async {
    final list = await _apiClient.getList(
      ApiEndpoints.stories,
      queryParameters: {'take': take},
    );
    return list
        .whereType<Map<String, dynamic>>()
        .map(SocialStoryModel.fromJson)
        .toList(growable: false);
  }

  Future<SocialStoryModel> createStory(CreateSocialStoryRequest request) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.stories,
      body: request.toJson(),
    );
    return SocialStoryModel.fromJson(json);
  }

  Future<void> markAsViewed(int storyId) async {
    await _apiClient.postJson(ApiEndpoints.storyView(storyId));
  }

  Future<List<StoryHighlightModel>> getMyHighlights() async {
    final list = await _apiClient.getList(ApiEndpoints.highlightsMe);
    return list
        .whereType<Map<String, dynamic>>()
        .map(StoryHighlightModel.fromJson)
        .toList(growable: false);
  }

  Future<StoryHighlightModel> createHighlight(
    CreateStoryHighlightRequest request,
  ) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.highlights,
      body: request.toJson(),
    );
    return StoryHighlightModel.fromJson(json);
  }
}
