import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class EmptyStateCard extends StatelessWidget {
  const EmptyStateCard({
    required this.title,
    required this.message,
    this.icon = Icons.inbox_outlined,
    this.action,
    super.key,
  });

  final String title;
  final String message;
  final IconData icon;
  final Widget? action;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.lg),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        border: Border.all(color: EventXColors.organizerStroke),
        borderRadius: BorderRadius.circular(EventXRadius.lg),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 34, color: EventXColors.organizerTextMuted),
          const SizedBox(height: EventXSpacing.sm),
          Text(
            title,
            style: Theme.of(context).textTheme.titleMedium,
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: EventXSpacing.xs),
          Text(
            message,
            style: Theme.of(context).textTheme.bodyMedium,
            textAlign: TextAlign.center,
          ),
          if (action != null) ...[
            const SizedBox(height: EventXSpacing.md),
            action!,
          ],
        ],
      ),
    );
  }
}
