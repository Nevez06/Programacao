import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/features/orders/data/models/order_model.dart';
import 'package:projeto_eventx_flutter/features/orders/data/orders_repository.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/organizer_metric_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class OrdersPage extends StatefulWidget {
  const OrdersPage({super.key});

  @override
  State<OrdersPage> createState() => _OrdersPageState();
}

class _OrdersPageState extends State<OrdersPage> {
  late Future<List<OrderModel>> _futureOrders;
  bool _updating = false;

  @override
  void initState() {
    super.initState();
    _futureOrders = _loadOrders();
  }

  Future<List<OrderModel>> _loadOrders() {
    return context.read<OrdersRepository>().getOrders();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureOrders = _loadOrders();
    });
    await _futureOrders;
  }

  Future<void> _updateStatus(OrderModel order, String status) async {
    if (_updating) {
      return;
    }
    setState(() => _updating = true);
    try {
      await context.read<OrdersRepository>().updateStatus(
            order.id,
            UpdateOrderStatusRequest(status),
          );
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Pedido atualizado para $status.')),
      );
      await _refresh();
    } catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(_resolveErrorMessage(error))),
      );
    } finally {
      if (mounted) {
        setState(() => _updating = false);
      }
    }
  }

  Future<void> _openOrderDetails(OrderModel order) async {
    final repository = context.read<OrdersRepository>();
    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      backgroundColor: EventXColors.organizerSurface,
      shape: const RoundedRectangleBorder(
        borderRadius:
            BorderRadius.vertical(top: Radius.circular(EventXRadius.xl)),
      ),
      builder: (sheetContext) {
        return FutureBuilder<OrderModel>(
          future: repository.getOrderById(order.id),
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Padding(
                padding: EdgeInsets.all(EventXSpacing.lg),
                child: Center(child: CircularProgressIndicator()),
              );
            }

            if (snapshot.hasError || snapshot.data == null) {
              return Padding(
                padding: const EdgeInsets.all(EventXSpacing.lg),
                child: EmptyStateCard(
                  title: 'Falha ao abrir detalhes',
                  message: _resolveErrorMessage(snapshot.error),
                  icon: Icons.error_outline_rounded,
                  action: FilledButton.tonal(
                    onPressed: () => Navigator.of(sheetContext).pop(),
                    child: const Text('Fechar'),
                  ),
                ),
              );
            }

            final item = snapshot.data!;
            return SafeArea(
              child: Padding(
                padding: const EdgeInsets.all(EventXSpacing.md),
                child: SingleChildScrollView(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Pedido ${item.id}',
                        style: Theme.of(context)
                            .textTheme
                            .titleLarge
                            ?.copyWith(fontWeight: FontWeight.w800),
                      ),
                      const SizedBox(height: EventXSpacing.xs),
                      Text(
                        '${item.productName} • ${item.eventName}',
                        style: const TextStyle(
                          color: EventXColors.organizerTextMuted,
                        ),
                      ),
                      const SizedBox(height: EventXSpacing.md),
                      Wrap(
                        spacing: EventXSpacing.xs,
                        runSpacing: EventXSpacing.xs,
                        children: [
                          _DetailChip(
                            icon: Icons.inventory_2_outlined,
                            label: 'Quantidade: ${item.quantity}',
                          ),
                          _DetailChip(
                            icon: Icons.attach_money_rounded,
                            label:
                                'Valor: R\$ ${item.totalPrice.toStringAsFixed(2)}',
                          ),
                          _DetailChip(
                            icon: Icons.event_outlined,
                            label: 'Data: ${_formatDate(item.orderedAt)}',
                          ),
                          _DetailChip(
                            icon: Icons.flag_outlined,
                            label: 'Status: ${item.status}',
                          ),
                        ],
                      ),
                      const SizedBox(height: EventXSpacing.sm),
                      Text(
                        'Fornecedor: ${item.supplierName ?? 'Nao informado'}',
                      ),
                      const SizedBox(height: 4),
                      Text('Evento relacionado: ${item.eventName}'),
                      const SizedBox(height: 4),
                      Text(
                        'Despesa gerada: ${item.expenseGenerated ? 'Sim' : 'Nao'}',
                      ),
                      const SizedBox(height: EventXSpacing.md),
                      Align(
                        alignment: Alignment.centerRight,
                        child: FilledButton.tonal(
                          onPressed: () => Navigator.of(sheetContext).pop(),
                          child: const Text('Fechar'),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            );
          },
        );
      },
    );
  }

  Future<void> _openCreateOrderDialog() async {
    final eventsRepository = context.read<EventsRepository>();
    final ordersRepository = context.read<OrdersRepository>();

    final events = await eventsRepository.getEvents();
    if (!mounted) {
      return;
    }

    if (events.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Crie um evento antes de criar um pedido.'),
        ),
      );
      return;
    }

    final titleController = TextEditingController();
    final totalController = TextEditingController();
    final descriptionController = TextEditingController();
    final formKey = GlobalKey<FormState>();
    var selectedEventId = events.first.id;
    var saving = false;

    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return StatefulBuilder(
          builder: (context, setStateDialog) {
            return AlertDialog(
              title: const Text('Novo pedido'),
              content: SizedBox(
                width: 520,
                child: Form(
                  key: formKey,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      DropdownButtonFormField<int>(
                        initialValue: selectedEventId,
                        decoration: const InputDecoration(labelText: 'Evento'),
                        items: events
                            .map(
                              (event) => DropdownMenuItem<int>(
                                value: event.id,
                                child: Text(event.nomeEvento),
                              ),
                            )
                            .toList(growable: false),
                        onChanged: (value) {
                          if (value == null) {
                            return;
                          }
                          setStateDialog(() => selectedEventId = value);
                        },
                      ),
                      const SizedBox(height: 10),
                      TextFormField(
                        controller: titleController,
                        decoration: const InputDecoration(
                          labelText: 'Titulo do pedido',
                        ),
                        validator: (value) => (value ?? '').trim().isEmpty
                            ? 'Informe o titulo do pedido.'
                            : null,
                      ),
                      const SizedBox(height: 10),
                      TextFormField(
                        controller: totalController,
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          labelText: 'Valor total',
                          hintText: 'Ex: 4500,00',
                        ),
                        validator: (value) => _parseCurrency(value ?? '') <= 0
                            ? 'Informe um valor valido.'
                            : null,
                      ),
                      const SizedBox(height: 10),
                      TextFormField(
                        controller: descriptionController,
                        decoration: const InputDecoration(
                          labelText: 'Descricao',
                        ),
                        minLines: 2,
                        maxLines: 4,
                      ),
                    ],
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed:
                      saving ? null : () => Navigator.of(dialogContext).pop(),
                  child: const Text('Cancelar'),
                ),
                FilledButton(
                  onPressed: saving
                      ? null
                      : () async {
                          if (!formKey.currentState!.validate()) {
                            return;
                          }

                          setStateDialog(() => saving = true);
                          try {
                            await ordersRepository.createOrder(
                              CreateOrderRequest(
                                eventId: selectedEventId,
                                title: titleController.text.trim(),
                                total: _parseCurrency(totalController.text),
                                description:
                                    descriptionController.text.trim().isEmpty
                                        ? null
                                        : descriptionController.text.trim(),
                              ),
                            );

                            if (!mounted || !dialogContext.mounted) {
                              return;
                            }
                            Navigator.of(dialogContext).pop();
                            ScaffoldMessenger.of(this.context).showSnackBar(
                              const SnackBar(
                                content: Text('Pedido criado com sucesso.'),
                              ),
                            );
                            await _refresh();
                          } catch (error) {
                            if (!mounted) {
                              return;
                            }
                            setStateDialog(() => saving = false);
                            ScaffoldMessenger.of(this.context).showSnackBar(
                              SnackBar(
                                content: Text(_resolveErrorMessage(error)),
                              ),
                            );
                          }
                        },
                  child: Text(saving ? 'Criando...' : 'Criar pedido'),
                ),
              ],
            );
          },
        );
      },
    );

    titleController.dispose();
    totalController.dispose();
    descriptionController.dispose();
  }

  static String _formatDate(DateTime date) {
    final safe = date.toLocal();
    final day = safe.day.toString().padLeft(2, '0');
    final month = safe.month.toString().padLeft(2, '0');
    final year = safe.year;
    return '$day/$month/$year';
  }

  double _parseCurrency(String value) {
    final normalized = value
        .replaceAll('R\$', '')
        .replaceAll('.', '')
        .replaceAll(',', '.')
        .trim();
    return double.tryParse(normalized) ?? 0;
  }

  String _resolveErrorMessage(Object? error) {
    if (error is ApiException) {
      return error.message;
    }
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao processar pedidos.';
    }
    return raw.replaceFirst('Exception: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerOrders,
      title: 'Pedidos',
      subtitle: 'Gestao de compras e entregas com status operacional',
      child: FutureBuilder<List<OrderModel>>(
        future: _futureOrders,
        builder: (context, snapshot) {
          final orders = snapshot.data ?? const <OrderModel>[];
          final counters = _orderCounters(orders);

          return ListView(
            padding: const EdgeInsets.all(EventXSpacing.md),
            children: [
              DashboardHeroCard(
                title: 'Central de pedidos',
                subtitle:
                    'Monitore SLA, aprovacao e entrega dos pedidos vinculados aos seus eventos.',
                primaryActionLabel: 'Novo pedido',
                onPrimaryAction: _openCreateOrderDialog,
                secondaryActionLabel: 'Marketplace',
                onSecondaryAction: () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerMarketplace),
              ),
              const SizedBox(height: EventXSpacing.md),
              _OrderMetrics(counters: counters, orders: orders),
              const SizedBox(height: EventXSpacing.lg),
              const SectionHeader(
                title: 'Status dos pedidos',
                subtitle:
                    'Fluxo real de pedidos gerados pelos orçamentos aceitos',
              ),
              const SizedBox(height: EventXSpacing.sm),
              if (snapshot.connectionState == ConnectionState.waiting)
                const Center(child: CircularProgressIndicator())
              else if (snapshot.hasError)
                EmptyStateCard(
                  title: 'Falha ao carregar pedidos',
                  message: _resolveErrorMessage(snapshot.error),
                  icon: Icons.cloud_off_outlined,
                  action: FilledButton.tonal(
                    onPressed: _refresh,
                    child: const Text('Tentar novamente'),
                  ),
                )
              else if (orders.isEmpty)
                EmptyStateCard(
                  title: 'Nenhum pedido encontrado',
                  message:
                      'Quando um orçamento for aceito, os pedidos aparecem aqui.',
                  icon: Icons.shopping_bag_outlined,
                  action: FilledButton.tonal(
                    onPressed: _openCreateOrderDialog,
                    child: const Text('Criar pedido'),
                  ),
                )
              else
                ...orders.map(
                  (order) => _OrderRow(
                    item: order,
                    onChangeStatus: (status) => _updateStatus(order, status),
                    onOpenDetails: () => _openOrderDetails(order),
                  ),
                ),
            ],
          );
        },
      ),
    );
  }

  Map<String, int> _orderCounters(List<OrderModel> orders) {
    final counters = <String, int>{
      'Pendente': 0,
      'Enviado': 0,
      'Entregue': 0,
      'Cancelado': 0,
    };
    for (final order in orders) {
      counters.update(order.status, (value) => value + 1, ifAbsent: () => 1);
    }
    return counters;
  }
}

class _OrderMetrics extends StatelessWidget {
  const _OrderMetrics({
    required this.counters,
    required this.orders,
  });

  final Map<String, int> counters;
  final List<OrderModel> orders;

  double get _totalValue =>
      orders.fold<double>(0, (value, order) => value + order.totalPrice);

  double get _onTimeRate {
    if (orders.isEmpty) {
      return 0;
    }
    final delivered =
        orders.where((order) => order.status == 'Entregue').length;
    return (delivered / orders.length) * 100;
  }

  @override
  Widget build(BuildContext context) {
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
              title: 'Pedidos ativos',
              value: '${orders.length}',
              icon: Icons.shopping_bag_outlined,
              trendText: 'Fluxo atual de pedidos',
            ),
            OrganizerMetricCard(
              title: 'Em atraso',
              value: '${counters['Pendente'] ?? 0}',
              icon: Icons.warning_amber_rounded,
              trendText: 'Requer atencao hoje',
              trendPositive: false,
              highlightColor: const Color(0xFFB9691A),
            ),
            OrganizerMetricCard(
              title: 'Valor total',
              value: 'R\$ ${_totalValue.toStringAsFixed(0)}',
              icon: Icons.paid_outlined,
              trendText: 'Soma dos pedidos',
              highlightColor: const Color(0xFF2D7DF6),
            ),
            OrganizerMetricCard(
              title: 'Entrega no prazo',
              value: '${_onTimeRate.toStringAsFixed(0)}%',
              icon: Icons.local_shipping_outlined,
              trendText: 'Taxa de entregas concluídas',
              highlightColor: const Color(0xFF008C6E),
            ),
          ],
        );
      },
    );
  }
}

class _OrderRow extends StatelessWidget {
  const _OrderRow({
    required this.item,
    required this.onChangeStatus,
    required this.onOpenDetails,
  });

  final OrderModel item;
  final ValueChanged<String> onChangeStatus;
  final VoidCallback onOpenDetails;

  @override
  Widget build(BuildContext context) {
    final color = switch (item.status) {
      'Entregue' => EventXColors.success,
      'Enviado' => const Color(0xFF2D7DF6),
      'Pendente' => EventXColors.warning,
      'Cancelado' => const Color(0xFF8D8D8D),
      _ => EventXColors.brand,
    };

    return Container(
      margin: const EdgeInsets.only(bottom: EventXSpacing.sm),
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      '${item.productName} • ${item.eventName}',
                      style: Theme.of(context)
                          .textTheme
                          .titleMedium
                          ?.copyWith(fontWeight: FontWeight.w700),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      'Fornecedor: ${item.supplierName ?? 'Nao informado'}',
                      style: Theme.of(context).textTheme.bodyMedium,
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Pedido: ${item.id}',
                      style: const TextStyle(
                        color: EventXColors.organizerTextMuted,
                        fontSize: 12,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      'Data: ${_OrdersPageState._formatDate(item.orderedAt)}',
                      style: const TextStyle(
                        color: EventXColors.organizerTextMuted,
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ),
              Text(
                'R\$ ${item.totalPrice.toStringAsFixed(0)}',
                style: const TextStyle(fontWeight: FontWeight.w700),
              ),
              const SizedBox(width: EventXSpacing.md),
              StatusBadge(label: item.status, color: color),
            ],
          ),
          const SizedBox(height: EventXSpacing.xs),
          PopupMenuButton<String>(
            onSelected: onChangeStatus,
            itemBuilder: (_) => const [
              PopupMenuItem(value: 'Pendente', child: Text('Pendente')),
              PopupMenuItem(value: 'Pago', child: Text('Pago')),
              PopupMenuItem(value: 'Enviado', child: Text('Enviado')),
              PopupMenuItem(value: 'Entregue', child: Text('Entregue')),
              PopupMenuItem(value: 'Cancelado', child: Text('Cancelado')),
            ],
            child: const Chip(label: Text('Alterar status')),
          ),
          const SizedBox(height: EventXSpacing.xs),
          ActionChip(
            avatar: const Icon(Icons.visibility_outlined, size: 16),
            label: const Text('Ver detalhes'),
            onPressed: onOpenDetails,
          ),
        ],
      ),
    );
  }
}

class _DetailChip extends StatelessWidget {
  const _DetailChip({
    required this.icon,
    required this.label,
  });

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: EventXSpacing.xs,
        vertical: 6,
      ),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurfaceAlt,
        borderRadius: BorderRadius.circular(EventXRadius.pill),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: EventXColors.organizerTextMuted),
          const SizedBox(width: 4),
          Text(
            label,
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}
