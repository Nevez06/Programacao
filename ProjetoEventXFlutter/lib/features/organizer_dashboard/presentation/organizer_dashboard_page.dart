import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/features/organizer_dashboard/data/dashboard_repository.dart';
import 'package:projeto_eventx_flutter/features/organizer_dashboard/data/models/dashboard_summary_model.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/action_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/budget_summary_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/organizer_metric_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/profile_info_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/animated_metric_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/animated_section_header.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/fade_slide_in.dart';

class OrganizerDashboardPage extends StatefulWidget {
  const OrganizerDashboardPage({super.key});

  @override
  State<OrganizerDashboardPage> createState() => _OrganizerDashboardPageState();
}

class _OrganizerDashboardPageState extends State<OrganizerDashboardPage> {
  late Future<DashboardSummaryModel> _futureDashboard;

  @override
  void initState() {
    super.initState();
    _futureDashboard = _loadDashboard();
  }

  Future<DashboardSummaryModel> _loadDashboard() {
    return context.read<DashboardRepository>().fetchSummary();
  }

  Future<void> _reload() async {
    setState(() {
      _futureDashboard = _loadDashboard();
    });
    await _futureDashboard;
  }

  String _resolveErrorMessage(Object? error) {
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar o dashboard.';
    }
    return raw
        .replaceFirst('Exception: ', '')
        .replaceFirst('DioException [connection error]: ', '')
        .replaceFirst('DioException [unknown]: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerDashboard,
      title: 'Dashboard',
      subtitle: 'Centro operacional do organizador EventX',
      topbarTrailing: IconButton(
        onPressed: () => OrganizerShell.logoutAndGoLogin(context),
        icon: const Icon(Icons.logout_rounded),
        tooltip: 'Sair',
      ),
      child: FutureBuilder<DashboardSummaryModel>(
        future: _futureDashboard,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(message: 'Carregando dashboard...');
          }

          if (snapshot.hasError || snapshot.data == null) {
            return ErrorView(
              message: _resolveErrorMessage(snapshot.error),
              onRetry: _reload,
            );
          }

          final dashboard = snapshot.data!;
          final profile = dashboard.profile;
          final displayName =
              profile.nome.trim().isNotEmpty ? profile.nome : 'Organizador';
          final budgetEvents = dashboard.events
              .where((event) => event.custoEstimado > 0)
              .toList()
            ..sort((a, b) => b.custoEstimado.compareTo(a.custoEstimado));
          final isEmptyDashboard = dashboard.totalEvents == 0 &&
              dashboard.totalOrders == 0 &&
              dashboard.totalQuotes == 0;

          if (isEmptyDashboard) {
            return RefreshIndicator(
              onRefresh: _reload,
              child: ListView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(EventXSpacing.md),
                children: [
                  EmptyStateCard(
                    title: 'Dashboard sem dados',
                    message:
                        'Nenhum evento encontrado. Crie seu primeiro evento para iniciar.',
                    icon: Icons.space_dashboard_outlined,
                    action: FilledButton.icon(
                      onPressed: () => Navigator.of(context)
                          .pushNamed(AppRoutes.organizerCreateEvent),
                      icon: const Icon(Icons.add_rounded),
                      label: const Text('Criar evento'),
                    ),
                  ),
                ],
              ),
            );
          }

          return RefreshIndicator(
            onRefresh: _reload,
            child: ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                FadeSlideIn(
                  child: DashboardHeroCard(
                    title: 'Bem-vindo, $displayName',
                    subtitle:
                        'Visao estrategica de eventos, convites, financeiro e fornecedores em um unico fluxo.',
                    primaryActionLabel: 'Criar novo evento',
                    onPrimaryAction: () => Navigator.of(context)
                        .pushNamed(AppRoutes.organizerCreateEvent),
                    secondaryActionLabel: 'Central de convites',
                    onSecondaryAction: () => Navigator.of(context)
                        .pushNamed(AppRoutes.organizerInvitations),
                  ),
                ),
                const SizedBox(height: EventXSpacing.md),
                FadeSlideIn(
                  delay: AppDurations.staggerStep,
                  child: _MetricsSection(
                    events: dashboard.events,
                    totalOrders: dashboard.totalOrders,
                    totalQuotes: dashboard.totalQuotes,
                  ),
                ),
                const SizedBox(height: EventXSpacing.lg),
                FadeSlideIn(
                  delay: AppDurations.staggerStep * 2,
                  child: AnimatedSectionHeader(
                    title: 'Acoes prioritarias',
                    subtitle:
                        'Ative os proximos passos do seu evento com um clique.',
                    trailing: FilledButton.tonalIcon(
                      onPressed: () =>
                          Navigator.of(context).pushNamed(AppRoutes.socialFeed),
                      icon: const Icon(Icons.dynamic_feed_outlined),
                      label: const Text('Abrir EventX Social'),
                    ),
                  ),
                ),
                const SizedBox(height: EventXSpacing.sm),
                FadeSlideIn(
                  delay: AppDurations.staggerStep * 3,
                  child: const _ActionCards(),
                ),
                const SizedBox(height: EventXSpacing.lg),
                FadeSlideIn(
                  delay: AppDurations.staggerStep * 4,
                  child: const AnimatedSectionHeader(
                    title: 'Proximos eventos',
                    subtitle: 'Agenda operacional com foco em execucao',
                  ),
                ),
                const SizedBox(height: EventXSpacing.sm),
                _UpcomingEventsSection(events: dashboard.recentEvents),
                const SizedBox(height: EventXSpacing.lg),
                FadeSlideIn(
                  delay: AppDurations.staggerStep * 8,
                  child: const AnimatedSectionHeader(
                    title: 'Resumo financeiro por evento',
                    subtitle:
                        'Monitoramento de custo previsto por carteira ativa',
                  ),
                ),
                const SizedBox(height: EventXSpacing.sm),
                if (budgetEvents.isEmpty)
                  const _SimpleInfoCard(
                    message:
                        'Nenhum custo estimado registrado nos eventos atuais.',
                  )
                else
                  ...budgetEvents.take(2).toList().asMap().entries.map(
                        (entry) => Padding(
                          padding: EdgeInsets.only(
                            bottom: entry.key == 0 ? EventXSpacing.sm : 0,
                          ),
                          child: FadeSlideIn(
                            delay: AppDurations.staggerStep * (9 + entry.key),
                            child: BudgetSummaryCard(
                              eventName: entry.value.nomeEvento,
                              spent: entry.value.custoEstimado,
                              totalBudget: entry.value.custoEstimado,
                              pendingLabel:
                                  'Status: ${_statusLabel(entry.value.statusEvento)}',
                            ),
                          ),
                        ),
                      ),
                const SizedBox(height: EventXSpacing.lg),
                FadeSlideIn(
                  delay: AppDurations.staggerStep * 11,
                  child: const AnimatedSectionHeader(
                    title: 'Perfil administrativo',
                    subtitle: 'Dados de conta e atalhos de configuracao',
                  ),
                ),
                const SizedBox(height: EventXSpacing.sm),
                FadeSlideIn(
                  delay: AppDurations.staggerStep * 12,
                  child: ProfileInfoCard(
                    name: profile.nome.trim().isEmpty
                        ? 'Organizador'
                        : profile.nome,
                    email: profile.email.trim().isEmpty ? '-' : profile.email,
                    roleLabel: profile.tipoUsuario.trim().isEmpty
                        ? 'Organizer'
                        : profile.tipoUsuario,
                    city: profile.cidade.trim().isEmpty ? null : profile.cidade,
                    state:
                        profile.estado.trim().isEmpty ? null : profile.estado,
                    phone: profile.telefone.trim().isEmpty
                        ? null
                        : profile.telefone,
                    avatarUrl: profile.fotoUrl,
                    actions: [
                      FilledButton.tonalIcon(
                        onPressed: () => Navigator.of(context)
                            .pushNamed(AppRoutes.organizerProfileAdmin),
                        icon: const Icon(Icons.settings_outlined),
                        label: const Text('Gerenciar conta'),
                      ),
                      OutlinedButton.icon(
                        onPressed: () => Navigator.of(context)
                            .pushNamed(AppRoutes.organizerNotifications),
                        icon: const Icon(Icons.notifications_outlined),
                        label: const Text('Notificacoes'),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _MetricsSection extends StatelessWidget {
  const _MetricsSection({
    required this.events,
    required this.totalOrders,
    required this.totalQuotes,
  });

  final List<EventListItem> events;
  final int totalOrders;
  final int totalQuotes;

  @override
  Widget build(BuildContext context) {
    final totalEvents = events.length;
    final publishedEvents = events
        .where((event) => _statusNormalized(event.statusEvento) == 'published')
        .length;
    final draftEvents = events
        .where((event) => _statusNormalized(event.statusEvento) == 'draft')
        .length;
    final activeEvents = events
        .where(
            (event) => _isActiveStatus(_statusNormalized(event.statusEvento)))
        .length;
    final openBudget = events
        .where(
            (event) => _isActiveStatus(_statusNormalized(event.statusEvento)))
        .fold<double>(0, (sum, event) => sum + event.custoEstimado);

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
          childAspectRatio: 1.3,
          physics: const NeverScrollableScrollPhysics(),
          children: [
            AnimatedMetricCard(
              delay: AppDurations.staggerStep,
              child: OrganizerMetricCard(
                title: 'Eventos ativos',
                subtitle: 'Operacoes em andamento',
                value: '$activeEvents',
                icon: Icons.event_outlined,
                trendText: '$totalEvents no total',
              ),
            ),
            AnimatedMetricCard(
              delay: AppDurations.staggerStep * 2,
              child: OrganizerMetricCard(
                title: 'Pedidos',
                subtitle: 'Total de pedidos vinculados',
                value: '$totalOrders',
                icon: Icons.receipt_long_outlined,
                trendText: '$publishedEvents eventos publicados',
                highlightColor: const Color(0xFF6C63FF),
              ),
            ),
            AnimatedMetricCard(
              delay: AppDurations.staggerStep * 3,
              child: OrganizerMetricCard(
                title: 'Orcamentos',
                subtitle: 'Cotacoes em acompanhamento',
                value: '$totalQuotes',
                icon: Icons.request_quote_outlined,
                trendText: '$draftEvents eventos em rascunho',
                highlightColor: const Color(0xFF2D7DF6),
              ),
            ),
            AnimatedMetricCard(
              delay: AppDurations.staggerStep * 4,
              child: OrganizerMetricCard(
                title: 'Orcamento previsto',
                subtitle: 'Custos totais dos eventos ativos',
                value: _formatCurrency(openBudget),
                icon: Icons.ssid_chart_outlined,
                trendText: '$activeEvents eventos no calculo',
                highlightColor: const Color(0xFF008C6E),
              ),
            ),
          ],
        );
      },
    );
  }
}

class _ActionCards extends StatelessWidget {
  const _ActionCards();

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final columns = constraints.maxWidth >= 1200
            ? 4
            : constraints.maxWidth >= 860
                ? 3
                : constraints.maxWidth >= 560
                    ? 2
                    : 1;

        return GridView.count(
          shrinkWrap: true,
          crossAxisCount: columns,
          crossAxisSpacing: EventXSpacing.sm,
          mainAxisSpacing: EventXSpacing.sm,
          childAspectRatio: 1.1,
          physics: const NeverScrollableScrollPhysics(),
          children: [
            AnimatedMetricCard(
              delay: AppDurations.staggerStep,
              child: ActionCard(
                title: 'Central de Convites',
                description:
                    'Escolha template, personalize e envie RSVP com controle.',
                icon: Icons.mail_outline_rounded,
                buttonLabel: 'Abrir convites',
                onTap: () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerInvitations),
              ),
            ),
            AnimatedMetricCard(
              delay: AppDurations.staggerStep * 2,
              child: ActionCard(
                title: 'Marketplace',
                description: 'Descubra fornecedores com ranking e avaliacao.',
                icon: Icons.storefront_outlined,
                buttonLabel: 'Explorar',
                onTap: () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerMarketplace),
                highlightColor: const Color(0xFF008C6E),
              ),
            ),
            AnimatedMetricCard(
              delay: AppDurations.staggerStep * 3,
              child: ActionCard(
                title: 'Orcamentos',
                description: 'Acompanhe propostas, negociacoes e aprovacoes.',
                icon: Icons.pie_chart_outline_rounded,
                buttonLabel: 'Gerenciar',
                onTap: () =>
                    Navigator.of(context).pushNamed(AppRoutes.organizerBudget),
                highlightColor: const Color(0xFF2D7DF6),
              ),
            ),
            AnimatedMetricCard(
              delay: AppDurations.staggerStep * 4,
              child: ActionCard(
                title: 'Assistente IA',
                description:
                    'Receba recomendacoes contextuais para acelerar decisoes.',
                icon: Icons.auto_awesome_outlined,
                buttonLabel: 'Conversar',
                onTap: () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerAiAssistant),
                highlightColor: const Color(0xFF7A50E5),
              ),
            ),
          ],
        );
      },
    );
  }
}

class _UpcomingEventsSection extends StatelessWidget {
  const _UpcomingEventsSection({
    required this.events,
  });

  final List<EventListItem> events;

  @override
  Widget build(BuildContext context) {
    final upcomingEvents =
        events.where((event) => event.id > 0).take(4).toList(growable: false);
    if (upcomingEvents.isEmpty) {
      return const _SimpleInfoCard(
        message: 'Nenhum evento encontrado. Crie um evento para iniciar.',
      );
    }

    return Column(
      children: List.generate(
        upcomingEvents.length,
        (index) => FadeSlideIn(
          delay: AppDurations.staggerStep * (5 + index),
          child: _UpcomingEventRow(event: upcomingEvents[index]),
        ),
      ),
    );
  }
}

class _SimpleInfoCard extends StatelessWidget {
  const _SimpleInfoCard({
    required this.message,
  });

  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: EventXSpacing.sm),
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Text(message),
    );
  }
}

class _UpcomingEventRow extends StatelessWidget {
  const _UpcomingEventRow({required this.event});

  final EventListItem event;

  @override
  Widget build(BuildContext context) {
    final normalizedStatus = _statusNormalized(event.statusEvento);
    final color = switch (normalizedStatus) {
      'published' => EventXColors.success,
      'draft' => EventXColors.warning,
      'completed' => EventXColors.brand,
      'cancelled' => EventXColors.error,
      _ => EventXColors.warning,
    };
    final date = event.dataEvento.toLocal();
    final dateLabel =
        '${date.day.toString().padLeft(2, '0')}/${date.month.toString().padLeft(2, '0')}/${date.year}';
    final locationLabel = (event.localNome ?? '').trim().isEmpty
        ? 'Local nao informado'
        : event.localNome!;

    return Container(
      margin: const EdgeInsets.only(bottom: EventXSpacing.sm),
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  event.nomeEvento,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w700,
                      ),
                ),
                const SizedBox(height: 4),
                Text(
                  '$dateLabel • $locationLabel',
                  style: Theme.of(context).textTheme.bodyMedium,
                ),
              ],
            ),
          ),
          StatusBadge(label: _statusLabel(event.statusEvento), color: color),
        ],
      ),
    );
  }
}

String _statusNormalized(String value) {
  final normalized = value.trim().toLowerCase();
  return switch (normalized) {
    'publicado' || 'confirmado' || 'published' => 'published',
    'cancelado' || 'cancelled' => 'cancelled',
    'finalizado' || 'concluido' || 'completed' => 'completed',
    'rascunho' || 'planejamento' || 'planejado' || 'draft' => 'draft',
    _ => normalized,
  };
}

bool _isActiveStatus(String normalizedStatus) {
  return normalizedStatus != 'cancelled' && normalizedStatus != 'completed';
}

String _statusLabel(String value) {
  return switch (_statusNormalized(value)) {
    'draft' => 'Rascunho',
    'published' => 'Publicado',
    'cancelled' => 'Cancelado',
    'completed' => 'Concluido',
    _ => value.trim().isEmpty ? 'Rascunho' : value,
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
