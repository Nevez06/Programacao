import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_conversation_model.dart';

class ChatConversationItem extends StatelessWidget {
  const ChatConversationItem({
    required this.conversation,
    required this.onTap,
    super.key,
  });

  final ChatConversationModel conversation;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      child: ListTile(
        onTap: onTap,
        leading: const CircleAvatar(
          child: Icon(Icons.smart_toy_outlined),
        ),
        title: Text(conversation.titulo),
        subtitle: Text(conversation.subtitulo),
        trailing: conversation.mensagensNaoLidas > 0
            ? CircleAvatar(
                radius: 12,
                child: Text(
                  conversation.mensagensNaoLidas.toString(),
                  style: Theme.of(context).textTheme.labelSmall,
                ),
              )
            : const Icon(Icons.chevron_right),
      ),
    );
  }
}
