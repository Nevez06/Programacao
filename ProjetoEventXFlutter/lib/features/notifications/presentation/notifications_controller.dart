import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/error/async_state.dart';
import 'package:projeto_eventx_flutter/core/realtime/realtime_connection_status.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/models/notification_model.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/notifications_live_service.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/notifications_repository.dart';

class NotificationsController extends ChangeNotifier {
  NotificationsController({
    required NotificationsRepository notificationsRepository,
    required NotificationsLiveService liveService,
  })  : _notificationsRepository = notificationsRepository,
        _liveService = liveService;

  final NotificationsRepository _notificationsRepository;
  final NotificationsLiveService _liveService;

  AsyncState<List<NotificationModel>> _state = const AsyncState.idle();
  AsyncState<List<NotificationModel>> get state => _state;

  int _unreadCount = 0;
  int get unreadCount => _unreadCount;

  RealtimeConnectionStatus _liveStatus = RealtimeConnectionStatus.disconnected;
  RealtimeConnectionStatus get liveStatus => _liveStatus;

  bool _initialized = false;
  bool get supportsHub => _liveService.supportsNotificationsHub;

  StreamSubscription<int>? _unreadSub;
  StreamSubscription<RealtimeConnectionStatus>? _statusSub;

  Future<void> initialize() async {
    if (_initialized) {
      return;
    }
    _initialized = true;

    await _liveService.start();
    _liveStatus = _liveService.status;

    _unreadSub = _liveService.unreadStream.listen((count) {
      _unreadCount = count.clamp(0, 9999).toInt();
      notifyListeners();
      unawaited(refresh(silent: true));
    });

    _statusSub = _liveService.connectionStream.listen((status) {
      _liveStatus = status;
      notifyListeners();
    });

    await refresh();
  }

  Future<void> refresh({bool silent = false}) async {
    final previous = _state.data;
    if (!silent) {
      _state = AsyncState<List<NotificationModel>>.loading(previous);
      notifyListeners();
    }

    try {
      final items = await _notificationsRepository.getNotifications();
      _state = AsyncState<List<NotificationModel>>.success(items);
      _unreadCount = items.where((item) => !item.isRead).length;
      notifyListeners();
      await _liveService.refreshNow();
    } on ApiException catch (error) {
      _state = AsyncState<List<NotificationModel>>.failure(
        error.message,
        previous,
      );
      notifyListeners();
    } catch (_) {
      _state = AsyncState<List<NotificationModel>>.failure(
        'Nao foi possivel carregar notificacoes.',
        previous,
      );
      notifyListeners();
    }
  }

  Future<void> markAsRead(NotificationModel item) async {
    try {
      await _notificationsRepository.markAsRead(item.id);
      final current = state.data ?? const <NotificationModel>[];
      final updated = current
          .map((entry) =>
              entry.id == item.id ? entry.copyWith(isRead: true) : entry)
          .toList(growable: false);
      _state = AsyncState<List<NotificationModel>>.success(updated);
      _unreadCount = updated.where((entry) => !entry.isRead).length;
      notifyListeners();
      await _liveService.refreshNow();
    } on ApiException catch (error) {
      _state = AsyncState<List<NotificationModel>>.failure(
        error.message,
        _state.data,
      );
      notifyListeners();
    }
  }

  Future<void> markAllAsRead() async {
    try {
      final unread = await _notificationsRepository.markAllAsRead();
      final current = state.data ?? const <NotificationModel>[];
      final updated = current
          .map((entry) => entry.copyWith(isRead: true))
          .toList(growable: false);
      _state = AsyncState<List<NotificationModel>>.success(updated);
      _unreadCount = unread.count;
      notifyListeners();
      await _liveService.refreshNow();
    } on ApiException catch (error) {
      _state = AsyncState<List<NotificationModel>>.failure(
        error.message,
        _state.data,
      );
      notifyListeners();
    }
  }

  Future<void> shutdown() async {
    await _liveService.stop();
  }

  @override
  void dispose() {
    _unreadSub?.cancel();
    _statusSub?.cancel();
    super.dispose();
  }
}
