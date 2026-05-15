import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/models/notification_model.dart';

class NotificationTile extends StatelessWidget {
  const NotificationTile({
    required this.notification,
    this.onTap,
    this.onMarkAsRead,
    super.key,
  });

  final NotificationModel notification;
  final VoidCallback? onTap;
  final VoidCallback? onMarkAsRead;

  @override
  Widget build(BuildContext context) {
    final iconData = _iconByType(notification.type);
    final iconColor = _colorByType(notification.type);

    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: notification.isRead
            ? EventXColors.organizerSurface
            : iconColor.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: ListTile(
        onTap: onTap,
        contentPadding: const EdgeInsets.symmetric(
          horizontal: 12,
          vertical: 8,
        ),
        leading: Container(
          width: 38,
          height: 38,
          decoration: BoxDecoration(
            color: iconColor.withValues(alpha: 0.16),
            borderRadius: BorderRadius.circular(EventXRadius.md),
          ),
          child: Icon(iconData, color: iconColor, size: 20),
        ),
        title: Text(
          notification.title,
          style: Theme.of(context).textTheme.titleSmall?.copyWith(
                fontWeight:
                    notification.isRead ? FontWeight.w600 : FontWeight.w700,
              ),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 4),
            Text(notification.message),
            const SizedBox(height: 4),
            Text(
              notification.timeAgo,
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ],
        ),
        trailing: notification.isRead
            ? null
            : IconButton(
                tooltip: 'Marcar como lida',
                onPressed: onMarkAsRead,
                icon: const Icon(Icons.done_all_rounded),
              ),
      ),
    );
  }

  static IconData _iconByType(String type) {
    switch (type.trim().toLowerCase()) {
      case 'evento':
        return Icons.event_outlined;
      case 'convite':
        return Icons.mail_outline_rounded;
      case 'pedido':
        return Icons.shopping_bag_outlined;
      case 'orcamento':
        return Icons.pie_chart_outline_rounded;
      case 'social':
        return Icons.dynamic_feed_outlined;
      case 'marketplace':
        return Icons.storefront_outlined;
      case 'sistema':
        return Icons.settings_outlined;
      default:
        return Icons.notifications_none_rounded;
    }
  }

  static Color _colorByType(String type) {
    switch (type.trim().toLowerCase()) {
      case 'evento':
        return EventXColors.brand;
      case 'convite':
        return const Color(0xFF6C63FF);
      case 'pedido':
        return const Color(0xFF008C6E);
      case 'orcamento':
        return const Color(0xFF3E7BFA);
      case 'social':
        return const Color(0xFFE55284);
      case 'marketplace':
        return const Color(0xFFB9691A);
      case 'sistema':
        return EventXColors.organizerTextMuted;
      default:
        return EventXColors.brand;
    }
  }
}
