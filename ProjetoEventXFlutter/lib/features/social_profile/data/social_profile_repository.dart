import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/models/social_profile_model.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/models/update_social_profile_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/social_story_model.dart';

class SocialProfileRepository {
  SocialProfileRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<SocialProfileModel> getMe() async {
    final json = await _apiClient.getJson(ApiEndpoints.socialProfileMe);
    return SocialProfileModel.fromJson(json);
  }

  Future<SocialProfileModel> updateMe(UpdateSocialProfileModel model) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.socialProfileMe,
      body: model.toJson(),
    );
    return SocialProfileModel.fromJson(json);
  }

  Future<SocialProfileModel> getById(int id) async {
    final json = await _apiClient.getJson(ApiEndpoints.socialProfileById(id));
    return SocialProfileModel.fromJson(json);
  }

  Future<SocialProfileModel> getByUsername(String username) async {
    final normalized = username.trim().replaceAll('@', '');
    final profileId = int.tryParse(normalized);
    if (profileId != null) {
      return getById(profileId);
    }

    final json = await _apiClient.getJson(
      ApiEndpoints.socialProfileByUsername(normalized),
    );
    return SocialProfileModel.fromJson(json);
  }

  Future<List<PostModel>> getPostsByProfileId(int profileId,
      {int take = 60}) async {
    final list = await _apiClient.getList(
      ApiEndpoints.socialProfilePosts(profileId),
      queryParameters: {'take': take},
    );
    return list
        .whereType<Map<String, dynamic>>()
        .map(PostModel.fromJson)
        .toList(growable: false);
  }

  Future<List<SocialStoryModel>> getStoriesByProfileId(
    int profileId, {
    int take = 60,
  }) async {
    final list = await _apiClient.getList(
      ApiEndpoints.socialProfileStories(profileId),
      queryParameters: {'take': take},
    );
    return list
        .whereType<Map<String, dynamic>>()
        .map(SocialStoryModel.fromJson)
        .toList(growable: false);
  }

  Future<void> follow(int profileId) async {
    await _apiClient.postJson(ApiEndpoints.socialFollow(profileId));
  }

  Future<void> unfollow(int profileId) async {
    await _apiClient.deleteVoid(ApiEndpoints.socialFollow(profileId));
  }

  Future<List<SocialProfileModel>> getFollowers(
    int profileId, {
    int take = 60,
  }) async {
    final list = await _apiClient.getList(
      ApiEndpoints.socialFollowers(profileId),
      queryParameters: {'take': take},
    );
    return list
        .whereType<Map<String, dynamic>>()
        .map(SocialProfileModel.fromJson)
        .toList(growable: false);
  }

  Future<List<SocialProfileModel>> getFollowing(
    int profileId, {
    int take = 60,
  }) async {
    final list = await _apiClient.getList(
      ApiEndpoints.socialFollowing(profileId),
      queryParameters: {'take': take},
    );
    return list
        .whereType<Map<String, dynamic>>()
        .map(SocialProfileModel.fromJson)
        .toList(growable: false);
  }
}
