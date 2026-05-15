import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/features/events/presentation/events_page.dart';
import 'package:projeto_eventx_flutter/features/feed/presentation/feed_page.dart';
import 'package:projeto_eventx_flutter/features/invites/presentation/invites_page.dart';
import 'package:projeto_eventx_flutter/features/notifications/presentation/notifications_controller.dart';
import 'package:projeto_eventx_flutter/features/notifications/presentation/notifications_page.dart';
import 'package:projeto_eventx_flutter/features/profile/presentation/profile_page.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/notification_badge.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  int _currentIndex = 0;

  static const _titles = [
    'Home',
    'Feed',
    'Eventos',
    'Convites',
    'Notificacoes',
    'Perfil',
  ];

  Future<void> _refreshUnreadCount() async {
    try {
      await context.read<NotificationsController>().refresh(silent: true);
    } catch (_) {
      // Falha temporaria nao bloqueia a navegacao.
    }
  }

  Future<void> _openChat() async {
    await Navigator.of(context).pushNamed(AppRoutes.chat);
    if (!mounted) {
      return;
    }
    await _refreshUnreadCount();
  }

  @override
  Widget build(BuildContext context) {
    final authController = context.watch<AuthController>();
    final notificationsController = context.watch<NotificationsController>();

    final profile = authController.profile;
    final session = authController.session;
    final nome = profile?.nome.isNotEmpty == true
        ? profile!.nome
        : (session?.userName ?? 'Usuario');

    final unreadNotifications = notificationsController.unreadCount;

    final pages = [
      _HomeDashboardTab(
        nome: nome,
        email: profile?.email ?? session?.email ?? '',
        unreadNotifications: unreadNotifications,
        onOpenChat: _openChat,
        onSelectTab: (index) {
          setState(() {
            _currentIndex = index;
          });
        },
      ),
      const FeedPage(embedded: true),
      const EventsPage(embedded: true),
      const InvitesPage(embedded: true),
      const NotificationsPage(embedded: true),
      const ProfilePage(embedded: true),
    ];

    return Scaffold(
      appBar: AppBar(
        title: Text(_titles[_currentIndex]),
        actions: [
          IconButton(
            tooltip: 'Chat',
            onPressed: _openChat,
            icon: const Icon(Icons.chat_bubble_outline),
          ),
          IconButton(
            tooltip: 'Sair',
            onPressed: () async {
              final notifications = context.read<NotificationsController>();
              final auth = context.read<AuthController>();
              await notifications.shutdown();
              await auth.logout();
              if (!context.mounted) {
                return;
              }
              Navigator.of(context).pushNamedAndRemoveUntil(
                AppRoutes.login,
                (route) => false,
              );
            },
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: IndexedStack(
        index: _currentIndex,
        children: pages,
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentIndex,
        onTap: (index) {
          setState(() {
            _currentIndex = index;
          });
          if (index == 4) {
            unawaited(_refreshUnreadCount());
          }
        },
        type: BottomNavigationBarType.fixed,
        items: [
          const BottomNavigationBarItem(
            icon: Icon(Icons.home_outlined),
            label: 'Home',
          ),
          const BottomNavigationBarItem(
            icon: Icon(Icons.dynamic_feed_outlined),
            label: 'Feed',
          ),
          const BottomNavigationBarItem(
            icon: Icon(Icons.event_outlined),
            label: 'Eventos',
          ),
          const BottomNavigationBarItem(
            icon: Icon(Icons.mail_outline),
            label: 'Convites',
          ),
          BottomNavigationBarItem(
            icon: NotificationBadge(count: unreadNotifications),
            label: 'Notificacoes',
          ),
          const BottomNavigationBarItem(
            icon: Icon(Icons.person_outline),
            label: 'Perfil',
          ),
        ],
      ),
    );
  }
}

class _HomeDashboardTab extends StatelessWidget {
  const _HomeDashboardTab({
    required this.nome,
    required this.email,
    required this.unreadNotifications,
    required this.onSelectTab,
    this.onOpenChat,
  });

  final String nome;
  final String email;
  final int unreadNotifications;
  final ValueChanged<int> onSelectTab;
  final VoidCallback? onOpenChat;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        const SectionTitle(
          title: 'ProjetoEventX',
          subtitle: 'Base Flutter em evolucao incremental',
        ),
        const SizedBox(height: 16),
        Card(
          child: ListTile(
            title: Text('Bem-vindo, $nome'),
            subtitle: Text(email),
            leading: const CircleAvatar(
              child: Icon(Icons.person),
            ),
          ),
        ),
        const SizedBox(height: 16),
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: [
            OutlinedButton.icon(
              onPressed: () => onSelectTab(1),
              icon: const Icon(Icons.dynamic_feed_outlined),
              label: const Text('Feed'),
            ),
            OutlinedButton.icon(
              onPressed: () => onSelectTab(2),
              icon: const Icon(Icons.event_outlined),
              label: const Text('Eventos'),
            ),
            OutlinedButton.icon(
              onPressed: () => onSelectTab(3),
              icon: const Icon(Icons.mail_outline),
              label: const Text('Convites'),
            ),
            OutlinedButton.icon(
              onPressed: onOpenChat,
              icon: const Icon(Icons.chat_bubble_outline),
              label: const Text('Chat'),
            ),
            OutlinedButton.icon(
              onPressed: () => onSelectTab(4),
              icon: const Icon(Icons.notifications_outlined),
              label: Text(
                unreadNotifications > 0
                    ? 'Notificacoes ($unreadNotifications)'
                    : 'Notificacoes',
              ),
            ),
            OutlinedButton.icon(
              onPressed: () => onSelectTab(5),
              icon: const Icon(Icons.person_outline),
              label: const Text('Perfil'),
            ),
          ],
        ),
      ],
    );
  }
}
