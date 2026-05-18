enum ChatMessageDeliveryStatus {
  sending,
  sent,
  failed,
  received,
}

class ChatMessageModel {
  const ChatMessageModel({
    required this.id,
    required this.eventoId,
    required this.remetenteId,
    required this.conteudo,
    required this.dataEnvio,
    required this.ehDoUsuarioAtual,
    required this.ehAssistente,
    required this.status,
    this.conversationId,
    this.servidorId,
    this.autorNome,
  });

  final String id;
  final int eventoId;
  final int remetenteId;
  final String conteudo;
  final DateTime dataEnvio;
  final bool ehDoUsuarioAtual;
  final bool ehAssistente;
  final ChatMessageDeliveryStatus status;
  final int? conversationId;
  final int? servidorId;
  final String? autorNome;

  int get resolvedConversationId => conversationId ?? eventoId;

  ChatMessageModel copyWith({
    ChatMessageDeliveryStatus? status,
  }) {
    return ChatMessageModel(
      id: id,
      eventoId: eventoId,
      remetenteId: remetenteId,
      conteudo: conteudo,
      dataEnvio: dataEnvio,
      ehDoUsuarioAtual: ehDoUsuarioAtual,
      ehAssistente: ehAssistente,
      status: status ?? this.status,
      conversationId: conversationId,
      servidorId: servidorId,
      autorNome: autorNome,
    );
  }

  factory ChatMessageModel.fromApiJson(
    Map<String, dynamic> json, {
    required int currentUserId,
    int? fallbackConversationId,
  }) {
    final conversationId = _readInt(
      json,
      ['conversationId', 'conversaId', 'eventoId'],
      fallback: fallbackConversationId ?? -1,
    );
    final senderId = _readInt(
      json,
      ['senderUserId', 'remetenteId', 'userId'],
      fallback: -1,
    );

    return ChatMessageModel(
      id: _readString(json, ['id', 'messageId', 'mensagemId']).isEmpty
          ? 'server-${DateTime.now().microsecondsSinceEpoch}'
          : _readString(json, ['id', 'messageId', 'mensagemId']),
      eventoId: conversationId,
      conversationId: conversationId,
      remetenteId: senderId,
      conteudo: _readString(json, ['content', 'conteudo', 'texto']),
      dataEnvio: _readDateTime(
        json,
        ['sentAt', 'dataEnvio', 'criadoEm', 'createdAt'],
      ),
      ehDoUsuarioAtual: senderId == currentUserId,
      ehAssistente: senderId == 0,
      status: ChatMessageDeliveryStatus.received,
      servidorId: _readNullableInt(json, ['id', 'messageId', 'mensagemId']),
      autorNome: _readNullableString(json, ['senderName', 'autorNome', 'nome']),
    );
  }

  static String _readString(Map<String, dynamic> json, List<String> keys) {
    for (final key in keys) {
      final value = json[key] ?? json[_pascalCase(key)];
      if (value != null) {
        return value.toString();
      }
    }
    return '';
  }

  static String? _readNullableString(
      Map<String, dynamic> json, List<String> keys) {
    final value = _readString(json, keys);
    return value.isEmpty ? null : value;
  }

  static int _readInt(
    Map<String, dynamic> json,
    List<String> keys, {
    int fallback = 0,
  }) {
    for (final key in keys) {
      final value = json[key] ?? json[_pascalCase(key)];
      if (value is int) {
        return value;
      }
      if (value is num) {
        return value.toInt();
      }
      final parsed = int.tryParse((value ?? '').toString());
      if (parsed != null) {
        return parsed;
      }
    }
    return fallback;
  }

  static int? _readNullableInt(Map<String, dynamic> json, List<String> keys) {
    final value = _readInt(json, keys, fallback: -1);
    if (value < 0) {
      return null;
    }
    return value;
  }

  static DateTime _readDateTime(Map<String, dynamic> json, List<String> keys) {
    for (final key in keys) {
      final value = json[key] ?? json[_pascalCase(key)];
      final date = DateTime.tryParse((value ?? '').toString());
      if (date != null) {
        return date;
      }
    }
    return DateTime.now();
  }

  static String _pascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
