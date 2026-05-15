import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/utils/date_formatter.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_message_model.dart';

class ChatMessageBubble extends StatelessWidget {
  const ChatMessageBubble({
    required this.message,
    super.key,
  });

  final ChatMessageModel message;

  @override
  Widget build(BuildContext context) {
    final isMine = message.ehDoUsuarioAtual;
    final colorScheme = Theme.of(context).colorScheme;
    final bubbleColor = isMine
        ? colorScheme.primaryContainer
        : colorScheme.surfaceContainerHighest;

    final align = isMine ? CrossAxisAlignment.end : CrossAxisAlignment.start;
    final icon = switch (message.status) {
      ChatMessageDeliveryStatus.sending => const SizedBox(
          width: 10,
          height: 10,
          child: CircularProgressIndicator(strokeWidth: 1.5),
        ),
      ChatMessageDeliveryStatus.failed =>
        Icon(Icons.error_outline, size: 14, color: colorScheme.error),
      _ => const SizedBox.shrink(),
    };

    return Align(
      alignment: isMine ? Alignment.centerRight : Alignment.centerLeft,
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 320),
        child: Card(
          color: bubbleColor,
          margin: const EdgeInsets.symmetric(vertical: 4),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
            child: Column(
              crossAxisAlignment: align,
              children: [
                if (message.ehAssistente && !isMine) ...[
                  Text(
                    'Assistente',
                    style: Theme.of(context).textTheme.labelSmall,
                  ),
                  const SizedBox(height: 4),
                ],
                Text(message.conteudo),
                const SizedBox(height: 4),
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(
                      DateFormatter.formatDateTime(message.dataEnvio),
                      style: Theme.of(context).textTheme.labelSmall,
                    ),
                    if (message.status != ChatMessageDeliveryStatus.sent &&
                        message.status !=
                            ChatMessageDeliveryStatus.received) ...[
                      const SizedBox(width: 6),
                      icon,
                    ],
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
