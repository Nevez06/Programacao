import 'dart:async';
import 'dart:io';

import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/realtime/realtime_connection_status.dart';
import 'package:signalr_netcore/ihub_protocol.dart';
import 'package:signalr_netcore/signalr_client.dart';

class SignalREvent {
  const SignalREvent({
    required this.name,
    required this.arguments,
  });

  final String name;
  final List<Object?> arguments;
}

class SignalRService {
  SignalRService({
    required ApiClient apiClient,
    required this.hubPath,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;
  final String hubPath;

  HubConnection? _connection;
  Timer? _healthTimer;
  Timer? _reconnectTimer;
  bool _connecting = false;

  final _statusController =
      StreamController<RealtimeConnectionStatus>.broadcast();
  final _eventController = StreamController<SignalREvent>.broadcast();

  RealtimeConnectionStatus _status = RealtimeConnectionStatus.disconnected;
  final Map<String, void Function(List<Object?>)> _eventListeners =
      <String, void Function(List<Object?>)>{};

  Stream<RealtimeConnectionStatus> get statusStream => _statusController.stream;
  Stream<SignalREvent> get eventsStream => _eventController.stream;
  RealtimeConnectionStatus get status => _status;
  bool get isConnected => _status == RealtimeConnectionStatus.connected;

  Future<void> ensureConnected() async {
    if (_connecting || isConnected) {
      return;
    }

    _connecting = true;
    _setStatus(RealtimeConnectionStatus.connecting);

    try {
      final hubUrl = _apiClient.baseUri.resolve(hubPath);
      final headers = await _buildHeaders();
      _connection ??= HubConnectionBuilder()
          .withUrl(
            hubUrl.toString(),
            options: HttpConnectionOptions(headers: headers),
          )
          .build();

      await _connection!.start();
      _bindRegisteredEvents();
      _setStatus(RealtimeConnectionStatus.connected);
      _startHealthCheck();
    } catch (_) {
      _setStatus(RealtimeConnectionStatus.disconnected);
      _scheduleReconnect();
      rethrow;
    } finally {
      _connecting = false;
    }
  }

  void on(String eventName, void Function(List<Object?>) listener) {
    _eventListeners[eventName] = listener;
    _bindEvent(eventName, listener);
  }

  Future<void> invoke(String methodName, {List<Object>? args}) async {
    await ensureConnected();
    await _connection?.invoke(methodName, args: args);
  }

  Future<void> disconnect() async {
    _reconnectTimer?.cancel();
    _healthTimer?.cancel();
    _setStatus(RealtimeConnectionStatus.disconnected);
    try {
      await _connection?.stop();
    } catch (_) {
      // Sem impacto funcional.
    }
  }

  void dispose() {
    unawaited(disconnect());
    _statusController.close();
    _eventController.close();
  }

  Future<MessageHeaders?> _buildHeaders() async {
    final cookieHeader = await _apiClient.readCookieHeaderForPath(hubPath);
    final token = _apiClient.sessionStorage.readToken();

    final raw = <String, String>{};
    if (cookieHeader != null && cookieHeader.isNotEmpty) {
      raw[HttpHeaders.cookieHeader] = cookieHeader;
    }
    if (token != null && token.isNotEmpty) {
      raw[HttpHeaders.authorizationHeader] = 'Bearer $token';
    }
    if (raw.isEmpty) {
      return null;
    }

    final headers = MessageHeaders();
    raw.forEach(headers.setHeaderValue);
    return headers;
  }

  void _setStatus(RealtimeConnectionStatus next) {
    if (_status == next) {
      return;
    }
    _status = next;
    _statusController.add(next);
  }

  void _startHealthCheck() {
    _healthTimer?.cancel();
    _healthTimer = Timer.periodic(const Duration(seconds: 6), (_) {
      final state = _connection?.state.toString().toLowerCase() ?? '';
      if (state.contains('connected')) {
        return;
      }
      if (state.contains('connecting') || state.contains('reconnecting')) {
        _setStatus(RealtimeConnectionStatus.reconnecting);
        return;
      }
      _setStatus(RealtimeConnectionStatus.disconnected);
      _scheduleReconnect();
    });
  }

  void _scheduleReconnect() {
    if (_reconnectTimer?.isActive == true) {
      return;
    }

    _setStatus(RealtimeConnectionStatus.reconnecting);
    _reconnectTimer = Timer(const Duration(seconds: 4), () async {
      _reconnectTimer = null;
      try {
        await ensureConnected();
        _rebindListeners();
      } catch (_) {
        _scheduleReconnect();
      }
    });
  }

  void _rebindListeners() {
    _bindRegisteredEvents();
  }

  void _bindRegisteredEvents() {
    if (_eventListeners.isEmpty) {
      return;
    }

    _eventListeners.forEach(_bindEvent);
  }

  void _bindEvent(String eventName, void Function(List<Object?>) listener) {
    _connection?.off(eventName);
    _connection?.on(eventName, (args) {
      final normalized = args ?? const <Object?>[];
      _eventController.add(
        SignalREvent(name: eventName, arguments: normalized),
      );
      listener(normalized);
    });
  }
}
