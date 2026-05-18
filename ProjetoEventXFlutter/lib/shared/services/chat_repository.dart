import 'package:projeto_eventx_flutter/features/chat/data/chat_repository.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_conversation_model.dart';

class ChatFeatureRepository {
  ChatFeatureRepository(this._source);

  final ChatRepository _source;

  Future<List<ChatConversationModel>> getConversations() =>
      _source.getConversations();
}
