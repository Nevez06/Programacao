import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/create_post_comment_model.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/create_post_model.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/toggle_like_model.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/update_post_model.dart';

class FeedRepository {
  FeedRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  bool get supportsFeedApi => true;

  Future<List<PostModel>> getPosts() async {
    final list = await _apiClient.getList(ApiEndpoints.socialFeed);
    return list
        .whereType<Map<String, dynamic>>()
        .map(PostModel.fromJson)
        .toList();
  }

  Future<PostModel> getPostDetails(int postId) async {
    final json = await _apiClient.getJson(ApiEndpoints.socialPostById(postId));
    return PostModel.fromJson(json);
  }

  Future<PostModel> createPost(CreatePostModel model) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.socialPosts,
      body: model.toJson(),
    );
    return PostModel.fromJson(json);
  }

  Future<PostModel> updatePost(int postId, UpdatePostModel model) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.postById(postId),
      body: model.toJson(),
    );
    return PostModel.fromJson(json);
  }

  Future<void> deletePost(int postId) async {
    await _apiClient.deleteVoid(ApiEndpoints.postById(postId));
  }

  Future<ToggleLikeResultModel> toggleLike(
    int postId, {
    bool? curtir,
  }) async {
    final shouldLike = curtir ?? true;
    final json = shouldLike
        ? await _apiClient.postJson(ApiEndpoints.socialPostLike(postId))
        : await _apiClient.deleteJson(ApiEndpoints.socialPostLike(postId));
    return ToggleLikeResultModel.fromJson(json);
  }

  Future<List<PostCommentModel>> getComments(int postId) async {
    final list = await _apiClient.getList(ApiEndpoints.socialPostComments(postId));
    return list
        .whereType<Map<String, dynamic>>()
        .map(PostCommentModel.fromJson)
        .toList(growable: false);
  }

  Future<PostCommentModel> addComment(
    int postId, {
    required String texto,
  }) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.socialPostComments(postId),
      body: CreatePostCommentModel(texto: texto).toJson(),
    );
    return PostCommentModel.fromJson(json);
  }
}
