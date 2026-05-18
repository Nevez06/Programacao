import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';

class EventFeedRemoteDataSource {
  EventFeedRemoteDataSource({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<Map<String, dynamic>>> getEvents() async {
    final list = await _apiClient.getList(ApiEndpoints.events);
    return list.whereType<Map<String, dynamic>>().toList(growable: false);
  }
}
