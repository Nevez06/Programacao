import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/utils/date_formatter.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';

class EventCard extends StatelessWidget {
  const EventCard({
    required this.event,
    this.onTap,
    this.onEdit,
    this.onDelete,
    super.key,
  });

  final EventListItem event;
  final VoidCallback? onTap;
  final VoidCallback? onEdit;
  final VoidCallback? onDelete;

  @override
  Widget build(BuildContext context) {
    final status = _normalizeStatus(event.statusEvento);
    final statusColor = switch (status) {
      'published' => EventXColors.success,
      'completed' => EventXColors.brand,
      'cancelled' => EventXColors.error,
      _ => EventXColors.warning,
    };

    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
        boxShadow: EventXShadows.soft,
      ),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        child: Padding(
          padding: const EdgeInsets.all(EventXSpacing.md),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    width: 72,
                    height: 72,
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(EventXRadius.md),
                      gradient: const LinearGradient(
                        colors: [Color(0xFFF7C9C2), Color(0xFFFCE6E1)],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                    ),
                    child: const Icon(
                      Icons.event_available_outlined,
                      color: EventXColors.brand,
                      size: 28,
                    ),
                  ),
                  const SizedBox(width: EventXSpacing.md),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          event.nomeEvento,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style:
                              Theme.of(context).textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.w700,
                                  ),
                        ),
                        const SizedBox(height: 4),
                        Wrap(
                          spacing: EventXSpacing.xs,
                          runSpacing: EventXSpacing.xs,
                          children: [
                            StatusBadge(
                              label: _statusLabel(event.statusEvento),
                              color: statusColor,
                            ),
                            StatusBadge(
                              label: event.tipoEvento,
                              color: EventXColors.brand,
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: EventXSpacing.sm),
              Wrap(
                spacing: EventXSpacing.md,
                runSpacing: EventXSpacing.xs,
                children: [
                  _MetaText(
                    icon: Icons.calendar_today_outlined,
                    text: DateFormatter.formatDate(event.dataEvento),
                  ),
                  _MetaText(
                    icon: Icons.location_on_outlined,
                    text: (event.localNome ?? '').isNotEmpty
                        ? event.localNome!
                        : 'Local a definir',
                  ),
                  _MetaText(
                    icon: Icons.group_outlined,
                    text: '${event.publicoEstimado} convidados',
                  ),
                  _MetaText(
                    icon: Icons.paid_outlined,
                    text: 'R\$ ${event.custoEstimado.toStringAsFixed(0)}',
                  ),
                ],
              ),
              const SizedBox(height: EventXSpacing.sm),
              Row(
                children: [
                  FilledButton.tonalIcon(
                    onPressed: onTap,
                    icon: const Icon(Icons.remove_red_eye_outlined, size: 18),
                    label: const Text('Ver'),
                  ),
                  const SizedBox(width: EventXSpacing.xs),
                  OutlinedButton.icon(
                    onPressed: onEdit,
                    icon: const Icon(Icons.edit_outlined, size: 18),
                    label: const Text('Editar'),
                  ),
                  const SizedBox(width: EventXSpacing.xs),
                  OutlinedButton.icon(
                    onPressed: onDelete,
                    icon: const Icon(Icons.delete_outline_rounded, size: 18),
                    label: const Text('Excluir'),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

String _normalizeStatus(String value) {
  final normalized = value.trim().toLowerCase();
  return switch (normalized) {
    'publicado' || 'confirmado' || 'published' => 'published',
    'cancelado' || 'cancelled' => 'cancelled',
    'finalizado' || 'concluido' || 'completed' => 'completed',
    'rascunho' || 'planejamento' || 'planejado' || 'draft' => 'draft',
    _ => normalized,
  };
}

String _statusLabel(String value) {
  return switch (_normalizeStatus(value)) {
    'draft' => 'Rascunho',
    'published' => 'Publicado',
    'cancelled' => 'Cancelado',
    'completed' => 'Concluido',
    _ => value,
  };
}

class _MetaText extends StatelessWidget {
  const _MetaText({
    required this.icon,
    required this.text,
  });

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 14, color: EventXColors.organizerTextMuted),
        const SizedBox(width: 4),
        Text(
          text,
          style: const TextStyle(
            fontSize: 12,
            fontWeight: FontWeight.w600,
            color: EventXColors.organizerTextMuted,
          ),
        ),
      ],
    );
  }
}
