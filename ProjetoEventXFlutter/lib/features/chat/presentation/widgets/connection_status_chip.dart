import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/realtime/realtime_connection_status.dart';

class ConnectionStatusChip extends StatelessWidget {
  const ConnectionStatusChip({
    required this.status,
    super.key,
  });

  final RealtimeConnectionStatus status;

  @override
  Widget build(BuildContext context) {
    final (label, color) = switch (status) {
      RealtimeConnectionStatus.connected => ('Conectado', Colors.green),
      RealtimeConnectionStatus.connecting => ('Conectando', Colors.blue),
      RealtimeConnectionStatus.reconnecting => ('Reconectando', Colors.orange),
      RealtimeConnectionStatus.disconnected => ('Desconectado', Colors.red),
    };

    return Chip(
      visualDensity: VisualDensity.compact,
      avatar: Icon(
        Icons.circle,
        size: 10,
        color: color,
      ),
      label: Text(label),
    );
  }
}
