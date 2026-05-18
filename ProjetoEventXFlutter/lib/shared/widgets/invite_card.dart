import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/utils/date_formatter.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/invite_model.dart';

class InviteCard extends StatelessWidget {
  const InviteCard({
    required this.invite,
    this.onTap,
    super.key,
  });

  final InviteModel invite;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      child: ListTile(
        onTap: onTap,
        title: Text(invite.titulo),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Status: ${invite.status}'),
            if (invite.eventoNome.isNotEmpty)
              Text('Evento: ${invite.eventoNome}'),
            Text('Data: ${DateFormatter.formatDate(invite.dataConvite)}'),
          ],
        ),
        trailing: const Icon(Icons.chevron_right),
      ),
    );
  }
}
