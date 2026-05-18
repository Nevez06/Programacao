import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_conversation_model.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_message_model.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_send_message_request.dart';

class ChatHistoryResult {
  const ChatHistoryResult({
    required this.messages,
    required this.historyAvailable,
    this.note,
  });

  final List<ChatMessageModel> messages;
  final bool historyAvailable;
  final String? note;
}

class ChatRepository {
  ChatRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<ChatConversationModel>> getConversations() async {
    try {
      final list = await _apiClient.getList(ApiEndpoints.chatConversations);
      final conversations = list
          .whereType<Map<String, dynamic>>()
          .map(ChatConversationModel.fromApiJson)
          .toList(growable: false);

      if (conversations.isNotEmpty) {
        return conversations;
      }
    } on ApiException catch (error) {
      if (!(error.isNotFound || error.isForbidden)) {
        rethrow;
      }
    }
    return const <ChatConversationModel>[];
  }

  Future<ChatHistoryResult> getHistoryForEvent({
    required int eventoId,
    required int currentUserId,
  }) async {
    try {
      final list = await _apiClient.getList(
        ApiEndpoints.chatMessagesByConversation(eventoId),
      );

      final messages = list
          .whereType<Map<String, dynamic>>()
          .map(
            (json) => ChatMessageModel.fromApiJson(
              json,
              currentUserId: currentUserId,
              fallbackConversationId: eventoId,
            ),
          )
          .toList(growable: false)
        ..sort((a, b) => a.dataEnvio.compareTo(b.dataEnvio));

      return ChatHistoryResult(
        messages: messages,
        historyAvailable: true,
      );
    } on ApiException catch (error) {
      if (!error.isNotFound) {
        rethrow;
      }

      return const ChatHistoryResult(
        messages: <ChatMessageModel>[],
        historyAvailable: false,
        note:
            'Historico completo depende de endpoint API para mensagens de chat no backend.',
      );
    }
  }

  Future<ChatMessageModel?> sendMessage({
    required ChatSendMessageRequest request,
    required int currentUserId,
  }) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.chatMessages,
      body: request.toJson(),
    );

    if (json.isEmpty) {
      return null;
    }

    return ChatMessageModel.fromApiJson(
      json,
      currentUserId: currentUserId,
      fallbackConversationId: request.conversationId,
    );
  }
}
