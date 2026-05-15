import 'dart:async';

import 'package:projeto_eventx_flutter/config/app_config.dart';
import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/realtime/realtime_connection_status.dart';
import 'package:projeto_eventx_flutter/core/realtime/signalr_service.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_message_model.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_presence_event.dart';

class ChatRealtimeService {
  ChatRealtimeService({
    required ApiClient apiClient,
  }) : _signalR = SignalRService(
          apiClient: apiClient,
          hubPath: AppConfig.chatHubPath,
        );

  final SignalRService _signalR;

  final _messagesController = StreamController<ChatMessageModel>.broadcast();
  final _connectionController =
      StreamController<RealtimeConnectionStatus>.broadcast();
  final _userOnlineController = StreamController<ChatPresenceEvent>.broadcast();
  final _userOfflineController =
      StreamController<ChatPresenceEvent>.broadcast();
  final _typingController = StreamController<ChatPresenceEvent>.broadcast();
  final _joinedController = StreamController<ChatPresenceEvent>.broadcast();
  final _leftController = StreamController<ChatPresenceEvent>.broadcast();

  StreamSubscription<RealtimeConnectionStatus>? _statusSub;
  bool _eventsBound = false;

  Stream<ChatMessageModel> get messagesStream => _messagesController.stream;
  Stream<RealtimeConnectionStatus> get connectionStream =>
      _connectionController.stream;
  Stream<ChatPresenceEvent> get userOnlineStream =>
      _userOnlineController.stream;
  Stream<ChatPresenceEvent> get userOfflineStream =>
      _userOfflineController.stream;
  Stream<ChatPresenceEvent> get typingStream => _typingController.stream;
  Stream<ChatPresenceEvent> get joinedStream => _joinedController.stream;
  Stream<ChatPresenceEvent> get leftStream => _leftController.stream;

  RealtimeConnectionStatus get status => _signalR.status;
  bool get isConnected => _signalR.isConnected;

  Future<void> ensureConnected() async {
    _statusSub ??= _signalR.statusStream.listen(_connectionController.add);
    await _signalR.ensureConnected();
    _bindEvents();
  }

  Future<void> disconnect() async {
    await _signalR.disconnect();
  }

  Future<void> joinConversation(int conversationId) async {
    await ensureConnected();
    await _signalR.invoke('JoinConversation', args: [conversationId]);
  }

  Future<void> leaveConversation(int conversationId) async {
    if (!isConnected) {
      return;
    }
    await _signalR.invoke('LeaveConversation', args: [conversationId]);
  }

  Future<void> heartbeat() async {
    await ensureConnected();
    await _signalR.invoke('Heartbeat');
  }

  Future<void> typing(int conversationId) async {
    await ensureConnected();
    await _signalR.invoke('Typing', args: [conversationId]);
  }

  Future<void> sendToAssistant({
    required int remetenteId,
    required String conteudo,
    required int eventoId,
  }) async {
    await ensureConnected();
    await _signalR.invoke(
      'SendToAssistant',
      args: [remetenteId, conteudo, eventoId],
    );
  }

  Future<void> sendMessage({
    required int remetenteId,
    required int destinatarioId,
    required String tipoDestinatario,
    required String conteudo,
    required int eventoId,
  }) async {
    await ensureConnected();
    await _signalR.invoke(
      'SendMessage',
      args: [remetenteId, destinatarioId, tipoDestinatario, conteudo, eventoId],
    );
  }

  void dispose() {
    unawaited(disconnect());
    _statusSub?.cancel();
    _messagesController.close();
    _connectionController.close();
    _userOnlineController.close();
    _userOfflineController.close();
    _typingController.close();
    _joinedController.close();
    _leftController.close();
    _signalR.dispose();
  }

  void _bindEvents() {
    if (_eventsBound) {
      return;
    }
    _eventsBound = true;

    _signalR.on('ReceiveMessage', _onReceiveMessage);
    _signalR.on('UserOnline', _onUserOnline);
    _signalR.on('UserOffline', _onUserOffline);
    _signalR.on('UserTyping', _onUserTyping);
    _signalR.on('UserJoinedConversation', _onUserJoinedConversation);
    _signalR.on('UserLeftConversation', _onUserLeftConversation);
  }

  void _onReceiveMessage(List<Object?> args) {
    final payload = _toMap(args.firstOrNull);

    final remetenteId = _toInt(
        payload['senderUserId'] ?? payload['remetenteId'] ?? args.firstOrNull);
    final conteudo = (payload['content'] ??
            payload['conteudo'] ??
            args.elementAtOrNull(1) ??
            '')
        .toString();
    final conversationId = _toInt(
      payload['conversationId'] ??
          payload['conversaId'] ??
          payload['eventoId'] ??
          args.elementAtOrNull(2) ??
          args.elementAtOrNull(3),
    );

    if (conteudo.trim().isEmpty) {
      return;
    }

    final message = ChatMessageModel(
      id: payload['id']?.toString() ??
          'server-${DateTime.now().microsecondsSinceEpoch}',
      eventoId: conversationId,
      remetenteId: remetenteId,
      conteudo: conteudo,
      dataEnvio: DateTime.tryParse(
            (payload['sentAt'] ?? payload['dataEnvio'] ?? '').toString(),
          ) ??
          DateTime.now(),
      ehDoUsuarioAtual: false,
      ehAssistente: remetenteId == 0,
      status: ChatMessageDeliveryStatus.received,
      conversationId: conversationId,
      servidorId: _toNullableInt(payload['id']),
      autorNome: (payload['senderName'] ?? payload['autorNome'])?.toString(),
    );

    _messagesController.add(message);
  }

  void _onUserOnline(List<Object?> args) {
    _userOnlineController.add(
      ChatPresenceEvent(
        userId: _toInt(args.firstOrNull),
      ),
    );
  }

  void _onUserOffline(List<Object?> args) {
    _userOfflineController.add(
      ChatPresenceEvent(
        userId: _toInt(args.firstOrNull),
      ),
    );
  }

  void _onUserTyping(List<Object?> args) {
    final payload = _toMap(args.firstOrNull);
    _typingController.add(
      ChatPresenceEvent(
        userId: _toInt(payload['userId'] ?? args.firstOrNull),
        conversationId: _toNullableInt(
          payload['conversationId'] ?? args.elementAtOrNull(1),
        ),
      ),
    );
  }

  void _onUserJoinedConversation(List<Object?> args) {
    final payload = _toMap(args.firstOrNull);
    _joinedController.add(
      ChatPresenceEvent(
        userId: _toInt(payload['userId'] ?? args.firstOrNull),
        conversationId: _toNullableInt(
          payload['conversationId'] ?? args.elementAtOrNull(1),
        ),
      ),
    );
  }

  void _onUserLeftConversation(List<Object?> args) {
    final payload = _toMap(args.firstOrNull);
    _leftController.add(
      ChatPresenceEvent(
        userId: _toInt(payload['userId'] ?? args.firstOrNull),
        conversationId: _toNullableInt(
          payload['conversationId'] ?? args.elementAtOrNull(1),
        ),
      ),
    );
  }

  int _toInt(Object? value) {
    if (value is int) {
      return value;
    }
    if (value is num) {
      return value.toInt();
    }
    return int.tryParse((value ?? '').toString()) ?? -1;
  }

  int? _toNullableInt(Object? value) {
    final parsed = _toInt(value);
    if (parsed < 0) {
      return null;
    }
    return parsed;
  }

  Map<String, dynamic> _toMap(Object? value) {
    if (value is Map<String, dynamic>) {
      return value;
    }
    return <String, dynamic>{};
  }
}

extension _ObjectListX on List<Object?> {
  Object? get firstOrNull => isEmpty ? null : first;

  Object? elementAtOrNull(int index) {
    if (index < 0 || index >= length) {
      return null;
    }
    return this[index];
  }
}
