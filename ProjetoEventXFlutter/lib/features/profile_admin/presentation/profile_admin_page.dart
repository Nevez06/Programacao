import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/features/profile/data/models/user_profile_model.dart';
import 'package:projeto_eventx_flutter/features/profile/data/profile_repository.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/organizer_metric_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/profile_info_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class ProfileAdminPage extends StatefulWidget {
  const ProfileAdminPage({super.key});

  @override
  State<ProfileAdminPage> createState() => _ProfileAdminPageState();
}

class _ProfileAdminPageState extends State<ProfileAdminPage> {
  late Future<_ProfileAdminData> _futureData;

  @override
  void initState() {
    super.initState();
    _futureData = _load();
  }

  Future<_ProfileAdminData> _load() async {
    final results = await Future.wait([
      context.read<ProfileRepository>().fetchMe(),
      context.read<EventsRepository>().getEvents(),
    ]);

    return _ProfileAdminData(
      profile: results[0] as UserProfileModel,
      events: results[1] as List<EventListItem>,
    );
  }

  Future<void> _reload() async {
    setState(() {
      _futureData = _load();
    });
    await _futureData;
  }

  String _resolveErrorMessage(Object? error) {
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar perfil administrativo.';
    }
    return raw
        .replaceFirst('Exception: ', '')
        .replaceFirst('DioException [connection error]: ', '')
        .replaceFirst('DioException [unknown]: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerProfileAdmin,
      title: 'Perfil Administrativo',
      subtitle: 'Conta, seguranca e indicadores do organizador',
      child: FutureBuilder<_ProfileAdminData>(
        future: _futureData,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(
              message: 'Carregando perfil administrativo...',
            );
          }

          if (snapshot.hasError || snapshot.data == null) {
            return ErrorView(
              message: _resolveErrorMessage(snapshot.error),
              onRetry: _reload,
            );
          }

          final data = snapshot.data!;
          final profile = data.profile;
          if (profile.id == 0 &&
              profile.nome.trim().isEmpty &&
              profile.email.trim().isEmpty) {
            return ListView(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                EmptyStateCard(
                  title: 'Perfil indisponivel',
                  message:
                      'Nao foi possivel carregar os dados do perfil administrativo.',
                  icon: Icons.person_off_outlined,
                  action: FilledButton.icon(
                    onPressed: _reload,
                    icon: const Icon(Icons.refresh_rounded),
                    label: const Text('Tentar novamente'),
                  ),
                ),
              ],
            );
          }
          return RefreshIndicator(
            onRefresh: _reload,
            child: ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                DashboardHeroCard(
                  title: profile.nome,
                  subtitle:
                      'Area de conta premium do organizador com dados, seguranca e atalhos operacionais.',
                  primaryActionLabel: 'Editar perfil',
                  onPrimaryAction: () =>
                      Navigator.of(context).pushNamed(AppRoutes.profileEdit),
                  secondaryActionLabel: 'Notificacoes',
                  onSecondaryAction: () => Navigator.of(context)
                      .pushNamed(AppRoutes.organizerNotifications),
                ),
                const SizedBox(height: EventXSpacing.md),
                _ProfileMetrics(
                    events: data.events, isActive: profile.isActive),
                const SizedBox(height: EventXSpacing.lg),
                const SectionHeader(
                  title: 'Dados da conta',
                  subtitle: 'Informacoes pessoais e status administrativo',
                ),
                const SizedBox(height: EventXSpacing.sm),
                ProfileInfoCard(
                  name: profile.nome,
                  email: profile.email,
                  roleLabel: profile.tipoUsuario,
                  city: profile.cidade.trim().isEmpty ? null : profile.cidade,
                  state: profile.estado.trim().isEmpty ? null : profile.estado,
                  phone:
                      profile.telefone.trim().isEmpty ? null : profile.telefone,
                  cpf: profile.cpf.trim().isEmpty ? null : profile.cpf,
                  avatarUrl: profile.fotoUrl,
                  actions: [
                    FilledButton.tonalIcon(
                      onPressed: () {},
                      icon: const Icon(Icons.lock_outline_rounded),
                      label: const Text('Seguranca'),
                    ),
                    OutlinedButton.icon(
                      onPressed: () {},
                      icon: const Icon(Icons.manage_accounts_outlined),
                      label: const Text('Preferencias'),
                    ),
                  ],
                ),
                const SizedBox(height: EventXSpacing.lg),
                const SectionHeader(
                  title: 'Acoes rapidas',
                  subtitle:
                      'Atalhos essenciais para manter conta e operacao saudaveis',
                ),
                const SizedBox(height: EventXSpacing.sm),
                const _QuickActions(),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _ProfileMetrics extends StatelessWidget {
  const _ProfileMetrics({
    required this.events,
    required this.isActive,
  });

  final List<EventListItem> events;
  final bool isActive;

  @override
  Widget build(BuildContext context) {
    final total = events.length;
    final published = events
        .where(
          (event) => _normalizedStatus(event.statusEvento) == 'published',
        )
        .length;
    final active = events
        .where(
          (event) =>
              _normalizedStatus(event.statusEvento) != 'cancelled' &&
              _normalizedStatus(event.statusEvento) != 'completed',
        )
        .length;
    final totalBudget =
        events.fold<double>(0, (sum, item) => sum + item.custoEstimado);

    return LayoutBuilder(
      builder: (context, constraints) {
        final columns = constraints.maxWidth >= 1200
            ? 4
            : constraints.maxWidth >= 760
                ? 2
                : 1;

        return GridView.count(
          shrinkWrap: true,
          crossAxisCount: columns,
          crossAxisSpacing: EventXSpacing.sm,
          mainAxisSpacing: EventXSpacing.sm,
          childAspectRatio: 1.25,
          physics: const NeverScrollableScrollPhysics(),
          children: [
            OrganizerMetricCard(
              title: 'Eventos geridos',
              value: '$total',
              icon: Icons.event_available_outlined,
              trendText: '$active ativos agora',
            ),
            OrganizerMetricCard(
              title: 'Eventos publicados',
              value: '$published',
              icon: Icons.publish_outlined,
              trendText: '${total - published} em rascunho',
              highlightColor: const Color(0xFF6C63FF),
            ),
            OrganizerMetricCard(
              title: 'Investimento previsto',
              value: _formatCurrency(totalBudget),
              icon: Icons.account_balance_wallet_outlined,
              trendText: 'Somatorio dos eventos',
              highlightColor: const Color(0xFF008C6E),
            ),
            OrganizerMetricCard(
              title: 'Status da conta',
              value: isActive ? 'Ativa' : 'Inativa',
              icon: Icons.security_outlined,
              trendText: isActive ? 'Acesso liberado' : 'Requer regularizacao',
              highlightColor: const Color(0xFF2D7DF6),
            ),
          ],
        );
      },
    );
  }
}

class _QuickActions extends StatelessWidget {
  const _QuickActions();

  @override
  Widget build(BuildContext context) {
    return Wrap(
      spacing: EventXSpacing.sm,
      runSpacing: EventXSpacing.sm,
      children: [
        FilledButton.tonalIcon(
          onPressed: () {},
          icon: const Icon(Icons.password_outlined),
          label: const Text('Alterar senha'),
        ),
        FilledButton.tonalIcon(
          onPressed: () {},
          icon: const Icon(Icons.verified_user_outlined),
          label: const Text('Ativar 2FA'),
        ),
        FilledButton.tonalIcon(
          onPressed: () =>
              Navigator.of(context).pushNamed(AppRoutes.organizerNotifications),
          icon: const Icon(Icons.notifications_active_outlined),
          label: const Text('Preferencias de alerta'),
        ),
        FilledButton.tonalIcon(
          onPressed: () =>
              Navigator.of(context).pushNamed(AppRoutes.organizerDashboard),
          icon: const Icon(Icons.dashboard_customize_outlined),
          label: const Text('Voltar ao dashboard'),
        ),
      ],
    );
  }
}

class _ProfileAdminData {
  const _ProfileAdminData({
    required this.profile,
    required this.events,
  });

  final UserProfileModel profile;
  final List<EventListItem> events;
}

String _normalizedStatus(String value) {
  final normalized = value.trim().toLowerCase();
  return switch (normalized) {
    'publicado' || 'confirmado' || 'published' => 'published',
    'cancelado' || 'cancelled' => 'cancelled',
    'finalizado' || 'concluido' || 'completed' => 'completed',
    'rascunho' || 'planejamento' || 'planejado' || 'draft' => 'draft',
    _ => normalized,
  };
}

String _formatCurrency(double value) {
  final fixed = value.toStringAsFixed(2).replaceAll('.', ',');
  final parts = fixed.split(',');
  final integer = parts.first;
  final decimal = parts.length > 1 ? parts[1] : '00';
  final buffer = StringBuffer();

  for (var i = 0; i < integer.length; i++) {
    final positionFromRight = integer.length - i;
    buffer.write(integer[i]);
    if (positionFromRight > 1 && positionFromRight % 3 == 1) {
      buffer.write('.');
    }
  }

  return 'R\$ ${buffer.toString()},$decimal';
}
