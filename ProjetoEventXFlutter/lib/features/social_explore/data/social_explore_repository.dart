import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/social_explore/data/models/social_explore_model.dart';

class SocialExploreRepository {
  SocialExploreRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<SocialExploreModel> getExplore({int take = 24}) async {
    final json = await _apiClient.getJson(
      ApiEndpoints.explore,
      queryParameters: {'take': take},
    );
    return SocialExploreModel.fromJson(json);
  }
}
