import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/profile/data/models/update_user_profile_model.dart';
import 'package:projeto_eventx_flutter/features/profile/data/models/user_profile_model.dart';

class ProfileRepository {
  ProfileRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<UserProfileModel> fetchMe() async {
    final json = await _apiClient.getJson(ApiEndpoints.me);
    return UserProfileModel.fromJson(json);
  }

  Future<UserProfileModel> fetchById(int userId) async {
    final json = await _apiClient.getJson(ApiEndpoints.userById(userId));
    return UserProfileModel.fromJson(json);
  }

  Future<UserProfileModel> updateMe(UpdateUserProfileModel model) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.me,
      body: model.toJson(),
    );
    return UserProfileModel.fromJson(json);
  }
}
