import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/realtime/realtime_connection_status.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/models/notification_model.dart';
import 'package:projeto_eventx_flutter/features/notifications/presentation/notifications_controller.dart';
import 'package:projeto_eventx_flutter/shared/widgets/empty_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/notification_tile.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class NotificationsPage extends StatefulWidget {
  const NotificationsPage({
    this.embedded = false,
    this.onUnreadCountChanged,
    super.key,
  });

  final bool embedded;
  final ValueChanged<int>? onUnreadCountChanged;

  @override
  State<NotificationsPage> createState() => _NotificationsPageState();
}

class _NotificationsPageState extends State<NotificationsPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      unawaited(context.read<NotificationsController>().initialize());
    });
  }

  Future<void> _refresh() {
    return context.read<NotificationsController>().refresh();
  }

  Future<void> _markAsRead(NotificationModel item) {
    return context.read<NotificationsController>().markAsRead(item);
  }

  Future<void> _markAllAsRead() {
    return context.read<NotificationsController>().markAllAsRead();
  }

  String _liveLabel(NotificationsController controller) {
    if (!controller.supportsHub) {
      return 'Atualizacao automatica por polling.';
    }

    return switch (controller.liveStatus) {
      RealtimeConnectionStatus.connected => 'Tempo real conectado.',
      RealtimeConnectionStatus.connecting => 'Conectando no hub...',
      RealtimeConnectionStatus.reconnecting => 'Reconectando no hub...',
      RealtimeConnectionStatus.disconnected => 'Hub desconectado.',
    };
  }

  Widget _buildBody(NotificationsController controller) {
    final items = controller.state.data ?? const <NotificationModel>[];
    final error = controller.state.error;

    widget.onUnreadCountChanged?.call(controller.unreadCount);

    if (controller.state.loading && items.isEmpty) {
      return const LoadingView(message: 'Carregando notificacoes...');
    }

    if (error != null && items.isEmpty) {
      return ErrorView(
        message: error,
        onRetry: _refresh,
      );
    }

    if (items.isEmpty) {
      return RefreshIndicator(
        onRefresh: _refresh,
        child: ListView(
          children: const [
            SizedBox(height: 100),
            EmptyState(
              icon: Icons.notifications_off_outlined,
              message: 'Voce nao possui notificacoes.',
            ),
          ],
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _refresh,
      child: ListView(
        padding: const EdgeInsets.only(top: 8, bottom: 16),
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Expanded(
                  child: SectionTitle(
                    title: 'Notificacoes',
                    subtitle: 'Lista vinda de /api/notifications',
                  ),
                ),
                TextButton(
                  onPressed: _markAllAsRead,
                  child: const Text('Marcar todas'),
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Text(
              _liveLabel(controller),
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
          const SizedBox(height: 6),
          ...items.map(
            (item) => NotificationTile(
              notification: item,
              onMarkAsRead: item.isRead ? null : () => _markAsRead(item),
            ),
          ),
          if (error != null)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              child: Text(
                error,
                style: Theme.of(context)
                    .textTheme
                    .bodySmall
                    ?.copyWith(color: Theme.of(context).colorScheme.error),
              ),
            ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final controller = context.watch<NotificationsController>();

    if (widget.embedded) {
      return _buildBody(controller);
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Notificacoes')),
      body: _buildBody(controller),
    );
  }
}
