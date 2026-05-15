import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/models/notification_model.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/models/unread_count_model.dart';

class NotificationsRepository {
  NotificationsRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<NotificationModel>> getNotifications({int take = 100}) async {
    final list = await _apiClient.getList(
      ApiEndpoints.notifications,
      queryParameters: {'take': take},
    );
    return list
        .whereType<Map<String, dynamic>>()
        .map(NotificationModel.fromJson)
        .toList();
  }

  Future<UnreadCountModel> getUnreadCount() async {
    final json =
        await _apiClient.getJson(ApiEndpoints.notificationsUnreadCount);
    return UnreadCountModel.fromJson(json);
  }

  Future<void> markAsRead(int notificationId) async {
    await _apiClient.putVoid(ApiEndpoints.notificationRead(notificationId));
  }

  Future<UnreadCountModel> markAllAsRead() async {
    final json = await _apiClient.putJson(ApiEndpoints.notificationsReadAll);
    return UnreadCountModel.fromJson(json);
  }
}
