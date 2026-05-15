import 'package:projeto_eventx_flutter/models/event_list_item.dart';

class ChatConversationModel {
  const ChatConversationModel({
    required this.eventoId,
    required this.titulo,
    required this.subtitulo,
    this.ultimaMensagem,
    this.ultimaMensagemEm,
    this.mensagensNaoLidas = 0,
    this.hasHistoryApi = false,
    this.conversationId,
    this.participantes,
  });

  final int eventoId;
  final String titulo;
  final String subtitulo;
  final String? ultimaMensagem;
  final DateTime? ultimaMensagemEm;
  final int mensagensNaoLidas;
  final bool hasHistoryApi;
  final int? conversationId;
  final List<String>? participantes;

  int get resolvedConversationId => conversationId ?? eventoId;

  factory ChatConversationModel.fromEvent(EventListItem event) {
    final local = (event.localNome ?? '').trim();
    final subtitulo = local.isEmpty
        ? '${event.tipoEvento} - ${event.statusEvento}'
        : '${event.tipoEvento} - $local';

    return ChatConversationModel(
      eventoId: event.id,
      conversationId: event.id,
      titulo: event.nomeEvento,
      subtitulo: subtitulo,
      hasHistoryApi: false,
    );
  }

  factory ChatConversationModel.fromApiJson(Map<String, dynamic> json) {
    int readInt(String key) {
      final value = json[key] ?? json[_pascalCase(key)];
      if (value is int) {
        return value;
      }
      if (value is num) {
        return value.toInt();
      }
      return int.tryParse((value ?? '').toString()) ?? 0;
    }

    String readString(String key) {
      final value = json[key] ?? json[_pascalCase(key)];
      return (value ?? '').toString();
    }

    DateTime? readDate(String key) {
      final value = json[key] ?? json[_pascalCase(key)];
      return DateTime.tryParse((value ?? '').toString());
    }

    final participantsRaw =
        (json['participants'] ?? json['Participantes']) as List<dynamic>?;

    final id = readInt('conversationId');
    final fallbackId = id == 0 ? readInt('id') : id;

    return ChatConversationModel(
      eventoId: fallbackId,
      conversationId: fallbackId,
      titulo: readString('title').isEmpty
          ? readString('nomeConversa')
          : readString('title'),
      subtitulo: readString('subtitle').isEmpty
          ? readString('descricao')
          : readString('subtitle'),
      ultimaMensagem:
          readString('lastMessage').isEmpty ? null : readString('lastMessage'),
      ultimaMensagemEm: readDate('lastMessageAt'),
      mensagensNaoLidas: readInt('unreadCount'),
      hasHistoryApi: true,
      participantes: participantsRaw
          ?.map((item) => item.toString())
          .where((text) => text.trim().isNotEmpty)
          .toList(growable: false),
    );
  }

  static String _pascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
