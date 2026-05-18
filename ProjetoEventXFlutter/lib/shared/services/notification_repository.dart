import 'package:projeto_eventx_flutter/features/notifications/data/models/notification_model.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/notifications_repository.dart';

class NotificationRepository {
  NotificationRepository(this._source);

  final NotificationsRepository _source;

  Future<List<NotificationModel>> getNotifications() =>
      _source.getNotifications();
}
