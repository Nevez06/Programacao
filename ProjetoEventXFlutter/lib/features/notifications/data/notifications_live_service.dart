import 'dart:async';

import 'package:projeto_eventx_flutter/config/app_config.dart';
import 'package:projeto_eventx_flutter/core/realtime/realtime_connection_status.dart';
import 'package:projeto_eventx_flutter/core/realtime/signalr_service.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/notifications_repository.dart';

class NotificationsLiveService {
  NotificationsLiveService({
    required NotificationsRepository notificationsRepository,
    required SignalRServiceFactory signalRFactory,
  })  : _notificationsRepository = notificationsRepository,
        _signalRFactory = signalRFactory;

  final NotificationsRepository _notificationsRepository;
  final SignalRServiceFactory _signalRFactory;

  final _unreadController = StreamController<int>.broadcast();
  final _connectionController =
      StreamController<RealtimeConnectionStatus>.broadcast();

  Timer? _pollingTimer;
  SignalRService? _signalR;
  StreamSubscription<RealtimeConnectionStatus>? _signalRStatusSub;

  bool _started = false;
  int _lastUnread = 0;
  RealtimeConnectionStatus _status = RealtimeConnectionStatus.disconnected;

  Stream<int> get unreadStream => _unreadController.stream;
  Stream<RealtimeConnectionStatus> get connectionStream =>
      _connectionController.stream;
  RealtimeConnectionStatus get status => _status;
  int get lastUnread => _lastUnread;

  bool get supportsNotificationsHub =>
      AppConfig.notificationsHubPath.trim().isNotEmpty;

  Future<void> start() async {
    if (_started) {
      return;
    }
    _started = true;

    if (supportsNotificationsHub) {
      await _startHubMode();
    } else {
      _setStatus(RealtimeConnectionStatus.disconnected);
      _startPollingMode();
    }
  }

  Future<void> stop() async {
    _started = false;
    _pollingTimer?.cancel();
    _pollingTimer = null;

    _signalRStatusSub?.cancel();
    _signalRStatusSub = null;

    final hub = _signalR;
    _signalR = null;
    await hub?.disconnect();
    hub?.dispose();

    _setStatus(RealtimeConnectionStatus.disconnected);
  }

  Future<void> refreshNow() async {
    await _fetchUnread();
  }

  void dispose() {
    unawaited(stop());
    _unreadController.close();
    _connectionController.close();
  }

  Future<void> _startHubMode() async {
    _setStatus(RealtimeConnectionStatus.connecting);
    try {
      final hub = _signalRFactory(AppConfig.notificationsHubPath);
      _signalR = hub;

      _signalRStatusSub = hub.statusStream.listen(_setStatus);

      hub.on('UnreadCountChanged', (args) async {
        if (args.isEmpty) {
          await _fetchUnread();
          return;
        }
        final value = _readInt(args.first);
        _publishUnread(value);
      });

      hub.on('NotificationReceived', (_) async {
        await _fetchUnread();
      });

      hub.on('NotificationsUpdated', (_) async {
        await _fetchUnread();
      });

      await hub.ensureConnected();
      _setStatus(RealtimeConnectionStatus.connected);
      await _fetchUnread();
    } catch (_) {
      _setStatus(RealtimeConnectionStatus.disconnected);
      _startPollingMode();
    }
  }

  void _startPollingMode() {
    _pollingTimer?.cancel();
    _pollingTimer = Timer.periodic(
      Duration(seconds: AppConfig.notificationsPollingSeconds),
      (_) => _fetchUnread(),
    );
    _fetchUnread();
  }

  Future<void> _fetchUnread() async {
    if (!_started) {
      return;
    }
    try {
      final count = await _notificationsRepository.getUnreadCount();
      _publishUnread(count.count);
    } catch (_) {
      // Mantemos o ultimo valor de badge em caso de erro temporario.
    }
  }

  void _publishUnread(int value) {
    final normalized = value < 0 ? 0 : value;
    if (_lastUnread == normalized) {
      return;
    }
    _lastUnread = normalized;
    _unreadController.add(_lastUnread);
  }

  void _setStatus(RealtimeConnectionStatus next) {
    if (_status == next) {
      return;
    }
    _status = next;
    _connectionController.add(next);
  }

  int _readInt(Object? value) {
    if (value is int) {
      return value;
    }
    if (value is num) {
      return value.toInt();
    }
    if (value is Map<String, dynamic>) {
      final count = value['count'] ?? value['Count'];
      return _readInt(count);
    }
    return int.tryParse((value ?? '').toString()) ?? 0;
  }
}

typedef SignalRServiceFactory = SignalRService Function(String hubPath);
