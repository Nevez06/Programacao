import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/notifications/presentation/notifications_controller.dart';
import 'package:projeto_eventx_flutter/features/notifications/presentation/notifications_page.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class OrganizerNotificationsPage extends StatefulWidget {
  const OrganizerNotificationsPage({super.key});

  @override
  State<OrganizerNotificationsPage> createState() =>
      _OrganizerNotificationsPageState();
}

class _OrganizerNotificationsPageState extends State<OrganizerNotificationsPage> {
  Future<void> _markAllAsRead() async {
    await context.read<NotificationsController>().markAllAsRead();
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerNotifications,
      title: 'Notificacoes',
      subtitle: 'Alertas operacionais do organizador e sinais do ecossistema',
      child: ListView(
        padding: const EdgeInsets.all(EventXSpacing.md),
        children: [
          DashboardHeroCard(
            title: 'Central de alertas EventX',
            subtitle:
                'Acompanhe eventos, convites, pedidos, orcamentos, social, marketplace e sistema.',
            primaryActionLabel: 'Marcar tudo como lido',
            onPrimaryAction: () => unawaited(_markAllAsRead()),
            secondaryActionLabel: 'Abrir assistente IA',
            onSecondaryAction: () =>
                Navigator.of(context).pushNamed(AppRoutes.organizerAiAssistant),
          ),
          const SizedBox(height: EventXSpacing.md),
          const SectionHeader(
            title: 'Tipos de notificacao',
            subtitle: 'Classificacao visual para leitura rapida',
          ),
          const SizedBox(height: EventXSpacing.sm),
          const Wrap(
            spacing: EventXSpacing.xs,
            runSpacing: EventXSpacing.xs,
            children: [
              StatusBadge(label: 'evento'),
              StatusBadge(label: 'convite'),
              StatusBadge(label: 'pedido'),
              StatusBadge(label: 'orcamento'),
              StatusBadge(label: 'social'),
              StatusBadge(label: 'marketplace'),
              StatusBadge(label: 'sistema'),
            ],
          ),
          const SizedBox(height: EventXSpacing.md),
          const SizedBox(
            height: 640,
            child: NotificationsPage(embedded: true),
          ),
        ],
      ),
    );
  }
}
