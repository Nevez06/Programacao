import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/features/budget/data/models/quote_model.dart';
import 'package:projeto_eventx_flutter/features/budget/data/quotes_repository.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/filter_panel.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/organizer_metric_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class QuotesPage extends StatefulWidget {
  const QuotesPage({super.key});

  @override
  State<QuotesPage> createState() => _QuotesPageState();
}

class _QuotesPageState extends State<QuotesPage> {
  late Future<List<QuoteModel>> _futureQuotes;
  String? _selectedStatus;
  bool _updating = false;

  static const _statusFilterValues = <String>[
    'Pendente',
    'Respondido',
    'EmNegociacao',
    'Aceito',
    'Recusado',
    'Cancelado',
  ];

  @override
  void initState() {
    super.initState();
    _futureQuotes = _loadQuotes();
  }

  Future<List<QuoteModel>> _loadQuotes() {
    return context.read<QuotesRepository>().getQuotes(status: _selectedStatus);
  }

  Future<void> _refresh() async {
    setState(() {
      _futureQuotes = _loadQuotes();
    });
    await _futureQuotes;
  }

  Future<void> _applyStatusFilter(String? status) async {
    setState(() {
      _selectedStatus = status;
      _futureQuotes = _loadQuotes();
    });
    await _futureQuotes;
  }

  Future<void> _updateStatus(QuoteModel quote, String status) async {
    if (_updating) {
      return;
    }
    setState(() => _updating = true);
    try {
      await context.read<QuotesRepository>().updateStatus(
            quote.id,
            UpdateQuoteStatusRequest(status: status),
          );
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text('Orcamento #${quote.id} atualizado para $status.')),
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

  Future<void> _negotiate(QuoteModel quote) async {
    final valueController = TextEditingController();
    final messageController = TextEditingController();
    bool saving = false;
    final formKey = GlobalKey<FormState>();

    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return StatefulBuilder(
          builder: (context, setStateDialog) {
            return AlertDialog(
              title: Text('Negociar orçamento #${quote.id}'),
              content: SizedBox(
                width: 520,
                child: Form(
                  key: formKey,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      TextFormField(
                        controller: valueController,
                        keyboardType: TextInputType.number,
                        decoration: const InputDecoration(
                          labelText: 'Novo valor',
                          hintText: 'Ex: 2500,00',
                        ),
                        validator: (value) {
                          if (_parseCurrency(value ?? '') <= 0) {
                            return 'Informe um valor valido.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 10),
                      TextFormField(
                        controller: messageController,
                        minLines: 2,
                        maxLines: 4,
                        decoration: const InputDecoration(
                          labelText: 'Mensagem de negociacao',
                        ),
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
                            await context.read<QuotesRepository>().negotiate(
                                  quote.id,
                                  NegotiateQuoteRequest(
                                    action: 'counter',
                                    value: _parseCurrency(valueController.text),
                                    message: messageController.text.trim(),
                                  ),
                                );
                            if (!mounted || !dialogContext.mounted) {
                              return;
                            }
                            Navigator.of(dialogContext).pop();
                            await _refresh();
                            if (!mounted) {
                              return;
                            }
                            ScaffoldMessenger.of(this.context).showSnackBar(
                              const SnackBar(
                                content: Text('Contraproposta enviada.'),
                              ),
                            );
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
                  child: Text(saving ? 'Enviando...' : 'Enviar'),
                ),
              ],
            );
          },
        );
      },
    );

    valueController.dispose();
    messageController.dispose();
  }

  String _resolveErrorMessage(Object? error) {
    if (error is ApiException) {
      return error.message;
    }
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar dados de orçamentos.';
    }
    return raw.replaceFirst('Exception: ', '');
  }

  double _parseCurrency(String value) {
    final normalized = value
        .replaceAll('R\$', '')
        .replaceAll('.', '')
        .replaceAll(',', '.')
        .trim();
    return double.tryParse(normalized) ?? 0;
  }

  @override
  Widget build(BuildContext context) {
    final tipoUsuario =
        context.read<AuthController>().session?.tipoUsuario.toLowerCase() ?? '';
    final isOrganizer = tipoUsuario == 'organizador';

    return OrganizerShell(
      currentRoute: AppRoutes.organizerBudget,
      title: 'Pagina de Orcamentos',
      subtitle: 'Negociacao e tomada de decisao com clareza',
      child: FutureBuilder<List<QuoteModel>>(
        future: _futureQuotes,
        builder: (context, snapshot) {
          final quotes = snapshot.data ?? const <QuoteModel>[];
          final statusCounters = _buildStatusCounters(quotes);

          return ListView(
            padding: const EdgeInsets.all(EventXSpacing.md),
            children: [
              DashboardHeroCard(
                title: 'Gestao de propostas',
                subtitle:
                    'Compare fornecedores, status e valores antes da aprovacao final.',
                primaryActionLabel: 'Solicitar no marketplace',
                onPrimaryAction: () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerMarketplace),
                secondaryActionLabel: 'Resumo financeiro',
                onSecondaryAction: () =>
                    Navigator.of(context).pushNamed(AppRoutes.organizerBudget),
              ),
              const SizedBox(height: EventXSpacing.md),
              _QuoteStatusMetrics(
                  statusCounters: statusCounters, quotes: quotes),
              const SizedBox(height: EventXSpacing.lg),
              LayoutBuilder(
                builder: (context, constraints) {
                  final isWide = constraints.maxWidth >= 980;
                  final filter = FilterPanel(
                    title: 'Filtros',
                    subtitle: 'Refine por status',
                    chips: const [
                      'Todos',
                      ..._statusFilterValues,
                    ],
                    actions: [
                      SizedBox(
                        width: 220,
                        child: DropdownButtonFormField<String>(
                          initialValue: _selectedStatus ?? '__all__',
                          decoration: const InputDecoration(
                            labelText: 'Status',
                            isDense: true,
                          ),
                          items: const [
                            DropdownMenuItem<String>(
                              value: '__all__',
                              child: Text('Todos'),
                            ),
                            DropdownMenuItem<String>(
                              value: 'Pendente',
                              child: Text('Pendente'),
                            ),
                            DropdownMenuItem<String>(
                              value: 'Respondido',
                              child: Text('Respondido'),
                            ),
                            DropdownMenuItem<String>(
                              value: 'EmNegociacao',
                              child: Text('Em negociacao'),
                            ),
                            DropdownMenuItem<String>(
                              value: 'Aceito',
                              child: Text('Aceito'),
                            ),
                            DropdownMenuItem<String>(
                              value: 'Recusado',
                              child: Text('Recusado'),
                            ),
                            DropdownMenuItem<String>(
                              value: 'Cancelado',
                              child: Text('Cancelado'),
                            ),
                          ],
                          onChanged: (value) {
                            final next = value == null || value == '__all__'
                                ? null
                                : value;
                            _applyStatusFilter(next);
                          },
                        ),
                      ),
                      OutlinedButton(
                        onPressed: () => _applyStatusFilter(null),
                        child: const Text('Limpar'),
                      ),
                      FilledButton.tonal(
                        onPressed: _refresh,
                        child: const Text('Atualizar'),
                      ),
                    ],
                  );

                  final resultList = Column(
                    children: [
                      const SectionHeader(
                        title: 'Resultados',
                        subtitle: 'Pipeline real de propostas em andamento',
                      ),
                      const SizedBox(height: EventXSpacing.sm),
                      if (snapshot.connectionState == ConnectionState.waiting)
                        const Center(child: CircularProgressIndicator())
                      else if (snapshot.hasError)
                        EmptyStateCard(
                          title: 'Falha ao carregar orçamento',
                          message: _resolveErrorMessage(snapshot.error),
                          icon: Icons.cloud_off_outlined,
                          action: FilledButton.tonal(
                            onPressed: _refresh,
                            child: const Text('Tentar novamente'),
                          ),
                        )
                      else if (quotes.isEmpty)
                        const EmptyStateCard(
                          title: 'Nenhum orçamento encontrado',
                          message:
                              'Solicite novos orçamentos no marketplace para iniciar negociação.',
                          icon: Icons.request_quote_outlined,
                        )
                      else
                        ...quotes.map(
                          (quote) => _QuoteRow(
                            item: quote,
                            isOrganizer: isOrganizer,
                            onChangeStatus: (status) =>
                                _updateStatus(quote, status),
                            onNegotiate:
                                isOrganizer ? () => _negotiate(quote) : null,
                          ),
                        ),
                    ],
                  );

                  if (isWide) {
                    return Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        SizedBox(width: 280, child: filter),
                        const SizedBox(width: EventXSpacing.md),
                        Expanded(child: resultList),
                      ],
                    );
                  }

                  return Column(
                    children: [
                      filter,
                      const SizedBox(height: EventXSpacing.sm),
                      resultList,
                    ],
                  );
                },
              ),
            ],
          );
        },
      ),
    );
  }

  Map<String, int> _buildStatusCounters(List<QuoteModel> quotes) {
    final map = <String, int>{
      'EmNegociacao': 0,
      'Pendente': 0,
      'Aceito': 0,
    };

    for (final quote in quotes) {
      map.update(quote.status, (value) => value + 1, ifAbsent: () => 1);
    }
    return map;
  }
}

class _QuoteStatusMetrics extends StatelessWidget {
  const _QuoteStatusMetrics({
    required this.statusCounters,
    required this.quotes,
  });

  final Map<String, int> statusCounters;
  final List<QuoteModel> quotes;

  double get _ticketMedio {
    if (quotes.isEmpty) {
      return 0;
    }
    final total = quotes.fold<double>(
      0,
      (value, quote) => value + (quote.responseValue ?? quote.estimatedValue),
    );
    return total / quotes.length;
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
          childAspectRatio: 1.28,
          physics: const NeverScrollableScrollPhysics(),
          children: [
            OrganizerMetricCard(
              title: 'Em negociacao',
              value: '${statusCounters['EmNegociacao'] ?? 0}',
              icon: Icons.handshake_outlined,
              trendText: 'Fluxo atual',
              highlightColor: const Color(0xFF2D7DF6),
            ),
            OrganizerMetricCard(
              title: 'Aguardando',
              value: '${statusCounters['Pendente'] ?? 0}',
              icon: Icons.hourglass_empty_rounded,
              trendText: 'Pendentes de resposta',
              trendPositive: false,
              highlightColor: const Color(0xFFB9691A),
            ),
            OrganizerMetricCard(
              title: 'Aprovados',
              value: '${statusCounters['Aceito'] ?? 0}',
              icon: Icons.verified_outlined,
              trendText: 'Conversao das propostas',
              highlightColor: const Color(0xFF008C6E),
            ),
            OrganizerMetricCard(
              title: 'Ticket medio',
              value: 'R\$ ${_ticketMedio.toStringAsFixed(0)}',
              icon: Icons.paid_outlined,
              trendText: 'Media das propostas',
            ),
          ],
        );
      },
    );
  }
}

class _QuoteRow extends StatelessWidget {
  const _QuoteRow({
    required this.item,
    required this.isOrganizer,
    required this.onChangeStatus,
    this.onNegotiate,
  });

  final QuoteModel item;
  final bool isOrganizer;
  final ValueChanged<String> onChangeStatus;
  final VoidCallback? onNegotiate;

  @override
  Widget build(BuildContext context) {
    final color = switch (item.status) {
      'Aceito' => EventXColors.success,
      'EmNegociacao' => EventXColors.warning,
      'Pendente' => EventXColors.brand,
      'Recusado' => const Color(0xFF8D8D8D),
      _ => EventXColors.organizerTextMuted,
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
                      item.supplierName,
                      style: Theme.of(context)
                          .textTheme
                          .titleMedium
                          ?.copyWith(fontWeight: FontWeight.w700),
                    ),
                    const SizedBox(height: 2),
                    Text(item.eventName,
                        style: Theme.of(context).textTheme.bodyMedium),
                    const SizedBox(height: 4),
                    Text(
                      'Atualizado: ${_formatDate(item.createdAt)}',
                      style: const TextStyle(
                        fontSize: 12,
                        color: EventXColors.organizerTextMuted,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: EventXSpacing.sm),
              Text(
                'R\$ ${(item.responseValue ?? item.estimatedValue).toStringAsFixed(0)}',
                style: const TextStyle(fontWeight: FontWeight.w800),
              ),
              const SizedBox(width: EventXSpacing.md),
              StatusBadge(label: item.status, color: color),
            ],
          ),
          const SizedBox(height: EventXSpacing.xs),
          Text(
            item.serviceName,
            style: const TextStyle(color: EventXColors.organizerTextMuted),
          ),
          const SizedBox(height: EventXSpacing.xs),
          Wrap(
            spacing: EventXSpacing.xs,
            runSpacing: EventXSpacing.xs,
            children: [
              if (isOrganizer)
                PopupMenuButton<String>(
                  tooltip: 'Alterar status',
                  onSelected: onChangeStatus,
                  itemBuilder: (_) => const [
                    PopupMenuItem(value: 'Pendente', child: Text('Pendente')),
                    PopupMenuItem(
                        value: 'Respondido', child: Text('Respondido')),
                    PopupMenuItem(
                        value: 'EmNegociacao', child: Text('Em negociacao')),
                    PopupMenuItem(value: 'Aceito', child: Text('Aceito')),
                    PopupMenuItem(value: 'Recusado', child: Text('Recusado')),
                    PopupMenuItem(value: 'Cancelado', child: Text('Cancelado')),
                  ],
                  child: const Chip(label: Text('Alterar status')),
                ),
              if (onNegotiate != null)
                ActionChip(
                  avatar: const Icon(Icons.swap_horiz_rounded, size: 16),
                  label: const Text('Negociar'),
                  onPressed: onNegotiate,
                ),
            ],
          ),
        ],
      ),
    );
  }

  String _formatDate(DateTime date) {
    final safe = date.toLocal();
    final day = safe.day.toString().padLeft(2, '0');
    final month = safe.month.toString().padLeft(2, '0');
    final year = safe.year;
    return '$day/$month/$year';
  }
}
