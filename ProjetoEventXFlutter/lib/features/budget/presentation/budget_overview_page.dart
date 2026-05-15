import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/budget/data/models/quote_model.dart';
import 'package:projeto_eventx_flutter/features/budget/data/quotes_repository.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/features/orders/data/models/order_model.dart';
import 'package:projeto_eventx_flutter/features/orders/data/orders_repository.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/action_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/budget_summary_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/organizer_metric_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class BudgetOverviewPage extends StatefulWidget {
  const BudgetOverviewPage({super.key});

  @override
  State<BudgetOverviewPage> createState() => _BudgetOverviewPageState();
}

class _BudgetOverviewPageState extends State<BudgetOverviewPage> {
  late Future<_BudgetOverviewData> _future;

  @override
  void initState() {
    super.initState();
    _future = _loadData();
  }

  Future<_BudgetOverviewData> _loadData() async {
    final quotesRepository = context.read<QuotesRepository>();
    final ordersRepository = context.read<OrdersRepository>();
    final eventsRepository = context.read<EventsRepository>();

    final results = await Future.wait<dynamic>([
      quotesRepository.getQuotes(),
      ordersRepository.getOrders(),
      eventsRepository.getEvents(),
    ]);

    return _BudgetOverviewData(
      quotes: results[0] as List<QuoteModel>,
      orders: results[1] as List<OrderModel>,
      events: results[2] as List<EventListItem>,
    );
  }

  Future<void> _refresh() async {
    setState(() {
      _future = _loadData();
    });
    await _future;
  }

  String _resolveErrorMessage(Object? error) {
    if (error is ApiException) {
      return error.message;
    }
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar o financeiro.';
    }
    return raw.replaceFirst('Exception: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerBudget,
      title: 'Orcamento Geral',
      subtitle: 'Confianca financeira para decisoes do organizador',
      child: FutureBuilder<_BudgetOverviewData>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(
                message: 'Carregando dados financeiros...');
          }

          if (snapshot.hasError) {
            return ErrorView(
              message: _resolveErrorMessage(snapshot.error),
              onRetry: _refresh,
            );
          }

          final data = snapshot.data ??
              const _BudgetOverviewData(
                quotes: <QuoteModel>[],
                orders: <OrderModel>[],
                events: <EventListItem>[],
              );
          final hasAnyData = data.events.isNotEmpty ||
              data.orders.isNotEmpty ||
              data.quotes.isNotEmpty;

          if (!hasAnyData) {
            return Center(
              child: EmptyStateCard(
                title: 'Nenhum dado financeiro encontrado',
                message:
                    'Crie eventos e solicite orçamentos para iniciar seu painel financeiro.',
                icon: Icons.account_balance_wallet_outlined,
                action: FilledButton.tonal(
                  onPressed: () => Navigator.of(context)
                      .pushNamed(AppRoutes.organizerEvents),
                  child: const Text('Criar evento'),
                ),
              ),
            );
          }

          final quotesTotal = data.quotes.fold<double>(
            0,
            (sum, item) => sum + (item.responseValue ?? item.estimatedValue),
          );
          final ordersTotal = data.orders.fold<double>(
            0,
            (sum, item) => sum + item.totalPrice,
          );
          final balance = quotesTotal - ordersTotal;
          final pendingQuotes = data.quotes
              .where(
                (item) =>
                    item.status == 'Pendente' || item.status == 'EmNegociacao',
              )
              .length;
          final pendingOrders =
              data.orders.where((item) => item.status == 'Pendente').length;

          return ListView(
            padding: const EdgeInsets.all(EventXSpacing.md),
            children: [
              DashboardHeroCard(
                title: 'Saude financeira do portfolio',
                subtitle:
                    'Acompanhe receita, custos, saldo e variacao por evento com clareza operacional.',
                primaryActionLabel: 'Abrir pagina de orcamentos',
                onPrimaryAction: () =>
                    Navigator.of(context).pushNamed(AppRoutes.organizerQuotes),
                secondaryActionLabel: 'Pedidos',
                onSecondaryAction: () =>
                    Navigator.of(context).pushNamed(AppRoutes.organizerOrders),
              ),
              const SizedBox(height: EventXSpacing.md),
              _FinanceMetricsGrid(
                quotesTotal: quotesTotal,
                ordersTotal: ordersTotal,
                balance: balance,
                pendingItems: pendingQuotes + pendingOrders,
              ),
              const SizedBox(height: EventXSpacing.lg),
              const SectionHeader(
                title: 'Orcamento por evento',
                subtitle: 'Distribuicao de custos e margem por carteira ativa',
              ),
              const SizedBox(height: EventXSpacing.sm),
              if (data.events.isEmpty)
                const EmptyStateCard(
                  title: 'Nenhum evento com orçamento',
                  message:
                      'Crie eventos para começar a acompanhar custos por carteira.',
                  icon: Icons.event_busy_outlined,
                )
              else
                ..._buildEventSummaryCards(data),
              const SizedBox(height: EventXSpacing.lg),
              const SectionHeader(
                title: 'Acoes financeiras',
                subtitle: 'Atalhos para acelerar controle e previsibilidade',
              ),
              const SizedBox(height: EventXSpacing.sm),
              const _FinanceActions(),
            ],
          );
        },
      ),
    );
  }

  List<Widget> _buildEventSummaryCards(_BudgetOverviewData data) {
    final spentByEvent = <int, double>{};
    final pendingByEvent = <int, int>{};

    for (final order in data.orders) {
      spentByEvent.update(
        order.eventId,
        (value) => value + order.totalPrice,
        ifAbsent: () => order.totalPrice,
      );
      if (order.status == 'Pendente') {
        pendingByEvent.update(order.eventId, (value) => value + 1,
            ifAbsent: () => 1);
      }
    }

    for (final quote in data.quotes) {
      if (quote.status == 'Pendente' || quote.status == 'EmNegociacao') {
        pendingByEvent.update(
          quote.eventId,
          (value) => value + 1,
          ifAbsent: () => 1,
        );
      }
    }

    final sortedEvents = [...data.events]
      ..sort((a, b) => b.dataEvento.compareTo(a.dataEvento));

    return sortedEvents.take(6).map((event) {
      final spent = spentByEvent[event.id] ?? 0;
      final totalBudget = event.custoEstimado > 0
          ? event.custoEstimado
          : (spent > 0 ? spent * 1.25 : 1000.0);
      final pending = pendingByEvent[event.id] ?? 0;

      return Padding(
        padding: const EdgeInsets.only(bottom: EventXSpacing.sm),
        child: BudgetSummaryCard(
          eventName: event.nomeEvento,
          spent: spent,
          totalBudget: totalBudget,
          pendingLabel:
              pending > 0 ? '$pending pendencias' : 'Sem pendencias abertas',
          onTap: () => Navigator.of(context).pushNamed(
            AppRoutes.eventDetails,
            arguments: event.id,
          ),
        ),
      );
    }).toList(growable: false);
  }
}

class _FinanceMetricsGrid extends StatelessWidget {
  const _FinanceMetricsGrid({
    required this.quotesTotal,
    required this.ordersTotal,
    required this.balance,
    required this.pendingItems,
  });

  final double quotesTotal;
  final double ordersTotal;
  final double balance;
  final int pendingItems;

  @override
  Widget build(BuildContext context) {
    final margin = quotesTotal <= 0
        ? 0
        : ((quotesTotal - ordersTotal) / quotesTotal) * 100;

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
          childAspectRatio: 1.28,
          physics: const NeverScrollableScrollPhysics(),
          children: [
            OrganizerMetricCard(
              title: 'Receita prevista',
              subtitle: 'Orcamentos registrados',
              value: 'R\$ ${quotesTotal.toStringAsFixed(0)}',
              icon: Icons.trending_up_rounded,
              trendText: 'Base real da API',
            ),
            OrganizerMetricCard(
              title: 'Custos projetados',
              subtitle: 'Pedidos vinculados',
              value: 'R\$ ${ordersTotal.toStringAsFixed(0)}',
              icon: Icons.payments_outlined,
              trendText: 'Atualizado em tempo real',
              trendPositive: false,
              highlightColor: const Color(0xFF2D7DF6),
            ),
            OrganizerMetricCard(
              title: 'Margem media',
              subtitle: 'Receita x custos',
              value: '${margin.toStringAsFixed(1)}%',
              icon: Icons.pie_chart_outline_rounded,
              trendText: 'Saldo: R\$ ${balance.toStringAsFixed(0)}',
              highlightColor: const Color(0xFF008C6E),
            ),
            OrganizerMetricCard(
              title: 'Pendencias',
              subtitle: 'Orcamentos e pedidos',
              value: '$pendingItems',
              icon: Icons.warning_amber_rounded,
              trendText: 'Itens aguardando acao',
              trendPositive: pendingItems == 0,
              highlightColor: const Color(0xFFB9691A),
            ),
          ],
        );
      },
    );
  }
}

class _FinanceActions extends StatelessWidget {
  const _FinanceActions();

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final columns = constraints.maxWidth >= 1180
            ? 3
            : constraints.maxWidth >= 760
                ? 2
                : 1;

        return GridView.count(
          shrinkWrap: true,
          crossAxisCount: columns,
          crossAxisSpacing: EventXSpacing.sm,
          mainAxisSpacing: EventXSpacing.sm,
          childAspectRatio: 1.22,
          physics: const NeverScrollableScrollPhysics(),
          children: [
            ActionCard(
              title: 'Novo orçamento',
              description:
                  'Cadastre uma nova proposta para fornecedores do evento.',
              icon: Icons.request_quote_outlined,
              buttonLabel: 'Criar',
              onTap: () =>
                  Navigator.of(context).pushNamed(AppRoutes.organizerQuotes),
              highlightColor: const Color(0xFF2D7DF6),
            ),
            ActionCard(
              title: 'Aprovar pedidos',
              description:
                  'Vincule pedidos aprovados ao plano financeiro do evento.',
              icon: Icons.receipt_long_outlined,
              buttonLabel: 'Abrir pedidos',
              onTap: () =>
                  Navigator.of(context).pushNamed(AppRoutes.organizerOrders),
              highlightColor: const Color(0xFF008C6E),
            ),
            ActionCard(
              title: 'Comparar fornecedores',
              description:
                  'Cruze valor, avaliacao e SLA para reduzir risco de custo.',
              icon: Icons.storefront_outlined,
              buttonLabel: 'Comparar',
              onTap: () => Navigator.of(context)
                  .pushNamed(AppRoutes.organizerMarketplace),
            ),
          ],
        );
      },
    );
  }
}

class _BudgetOverviewData {
  const _BudgetOverviewData({
    required this.quotes,
    required this.orders,
    required this.events,
  });

  final List<QuoteModel> quotes;
  final List<OrderModel> orders;
  final List<EventListItem> events;
}
