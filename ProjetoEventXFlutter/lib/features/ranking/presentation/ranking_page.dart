import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/services/ranking_repository.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/ranking_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/animated_empty_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/animated_section_header.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/app_skeleton_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/fade_slide_in.dart';

class RankingPage extends StatefulWidget {
  const RankingPage({super.key});

  @override
  State<RankingPage> createState() => _RankingPageState();
}

class _RankingPageState extends State<RankingPage> {
  late Future<_RankingViewData> _futureRanking;
  String? _selectedCategory;
  String? _selectedCity;
  String? _selectedState;

  @override
  void initState() {
    super.initState();
    _futureRanking = _loadRanking();
  }

  Future<_RankingViewData> _loadRanking() async {
    final rankingRepository = context.read<RankingRepository>();
    final rankingEntries = await rankingRepository.getRanking(
      category: _selectedCategory,
      city: _selectedCity,
      state: _selectedState,
      take: 120,
    );

    if (rankingEntries.isEmpty) {
      return const _RankingViewData(ranking: <RankingEntry>[], top: []);
    }

    final hasFilters = (_selectedCategory?.isNotEmpty ?? false) ||
        (_selectedCity?.isNotEmpty ?? false) ||
        (_selectedState?.isNotEmpty ?? false);

    if (hasFilters) {
      return _RankingViewData(
        ranking: rankingEntries,
        top: rankingEntries.take(3).toList(growable: false),
      );
    }

    final topEntries = await rankingRepository.getTopSuppliers(take: 3);
    return _RankingViewData(
      ranking: rankingEntries,
      top: topEntries.isEmpty
          ? rankingEntries.take(3).toList(growable: false)
          : topEntries,
    );
  }

  void _reloadRanking() {
    setState(() {
      _futureRanking = _loadRanking();
    });
  }

  void _toggleCategory(String value) {
    setState(() {
      _selectedCategory = _selectedCategory == value ? null : value;
      _futureRanking = _loadRanking();
    });
  }

  void _toggleCity(String value) {
    setState(() {
      _selectedCity = _selectedCity == value ? null : value;
      _futureRanking = _loadRanking();
    });
  }

  void _toggleState(String value) {
    setState(() {
      _selectedState = _selectedState == value ? null : value;
      _futureRanking = _loadRanking();
    });
  }

  String _buildMetricsLabel(RankingEntry entry) {
    final response = (entry.responseRate * 100).round();
    final acceptance = (entry.acceptanceRate * 100).round();
    final rating = entry.rating > 0 ? entry.rating : entry.averageRating;
    return 'Nota ${rating.toStringAsFixed(1)} • '
        '${entry.reviewCount} avaliacoes • '
        '${entry.completedOrders} concluidos • '
        'Resp. $response% • Aceite $acceptance%';
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerRanking,
      title: 'Ranking de Fornecedores',
      subtitle: 'Score composto por qualidade, confiabilidade e performance',
      child: ListView(
        padding: const EdgeInsets.all(EventXSpacing.md),
        children: [
          FadeSlideIn(
            child: DashboardHeroCard(
              title: 'Ranking competitivo EventX',
              subtitle:
                  'Pontuacao inteligente baseada em nota media, confiabilidade, resposta, pontualidade e tendencia recente.',
              primaryActionLabel: 'Avaliar fornecedores',
              onPrimaryAction: () => Navigator.of(context)
                  .pushNamed(AppRoutes.organizerSupplierReview),
              secondaryActionLabel: 'Abrir marketplace',
              onSecondaryAction: () => Navigator.of(context)
                  .pushNamed(AppRoutes.organizerMarketplace),
            ),
          ),
          const SizedBox(height: EventXSpacing.md),
          FadeSlideIn(
            delay: AppDurations.staggerStep,
            child: _RankingFilters(
              selectedCategory: _selectedCategory,
              selectedCity: _selectedCity,
              selectedState: _selectedState,
              onCategoryChanged: _toggleCategory,
              onCityChanged: _toggleCity,
              onStateChanged: _toggleState,
            ),
          ),
          const SizedBox(height: EventXSpacing.lg),
          FutureBuilder<_RankingViewData>(
            future: _futureRanking,
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const _RankingSkeleton();
              }

              if (snapshot.hasError) {
                final errorMessage = snapshot.error?.toString().trim();
                return AnimatedEmptyState(
                  child: _RankingMessageCard(
                    title: 'Nao foi possivel carregar o ranking',
                    message: (errorMessage == null || errorMessage.isEmpty)
                        ? 'Tente novamente para atualizar a classificacao dos fornecedores.'
                        : errorMessage,
                    actionLabel: 'Recarregar',
                    onAction: _reloadRanking,
                  ),
                );
              }

              final data = snapshot.data ?? const _RankingViewData.empty();
              if (data.ranking.isEmpty) {
                return const AnimatedEmptyState(
                  child: _RankingMessageCard(
                    title: 'Sem dados de ranking',
                    message:
                        'Assim que houver historico de desempenho, o ranking sera exibido aqui.',
                    actionLabel: 'Ir para marketplace',
                  ),
                );
              }

              final topThree = data.top.isEmpty
                  ? data.ranking.take(3).toList(growable: false)
                  : data.top.take(3).toList(growable: false);
              final topIds = topThree
                  .map((entry) => entry.supplierId)
                  .where((id) => id > 0)
                  .toSet();

              final rest = data.ranking.where((entry) {
                if (topIds.isNotEmpty) {
                  return !topIds.contains(entry.supplierId);
                }
                return entry.position > 3;
              }).toList(growable: false);

              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const AnimatedSectionHeader(
                    title: 'Top 3 da semana',
                    subtitle: 'Destaques com maior confianca de entrega',
                  ),
                  const SizedBox(height: EventXSpacing.sm),
                  LayoutBuilder(
                    builder: (context, constraints) {
                      final columns = constraints.maxWidth >= 980 ? 3 : 1;
                      return GridView.builder(
                        shrinkWrap: true,
                        itemCount: topThree.length,
                        physics: const NeverScrollableScrollPhysics(),
                        gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                          crossAxisCount: columns,
                          mainAxisSpacing: EventXSpacing.sm,
                          crossAxisSpacing: EventXSpacing.sm,
                          childAspectRatio: 2.0,
                        ),
                        itemBuilder: (context, index) {
                          final entry = topThree[index];
                          return FadeSlideIn(
                            delay: AppDurations.staggerStep * index,
                            child: RankingCard(
                              position: entry.position <= 0
                                  ? index + 1
                                  : entry.position,
                              name: entry.name,
                              score: entry.score,
                              deltaLabel:
                                  entry.delta.isEmpty ? '—' : entry.delta,
                              categoryLabel: entry.category,
                              isTopHighlight: true,
                            ),
                          );
                        },
                      );
                    },
                  ),
                  const SizedBox(height: EventXSpacing.lg),
                  const AnimatedSectionHeader(
                    title: 'Ranking completo',
                    subtitle: 'Pontuacao com explicacao por fornecedor',
                  ),
                  const SizedBox(height: EventXSpacing.sm),
                  ...List.generate(rest.length, (index) {
                    final entry = rest[index];
                    return FadeSlideIn(
                      delay: AppDurations.staggerStep * (index + 1),
                      child: Padding(
                        padding:
                            const EdgeInsets.only(bottom: EventXSpacing.xs),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            RankingCard(
                              position: entry.position <= 0
                                  ? index + 4
                                  : entry.position,
                              name: entry.name,
                              score: entry.score,
                              deltaLabel:
                                  entry.delta.isEmpty ? '—' : entry.delta,
                              categoryLabel: entry.category,
                            ),
                            Padding(
                              padding: const EdgeInsets.only(
                                left: EventXSpacing.sm,
                                top: 4,
                                bottom: EventXSpacing.xs,
                              ),
                              child: Text(
                                _buildMetricsLabel(entry),
                                style: const TextStyle(
                                  fontSize: 12,
                                  color: EventXColors.organizerTextMuted,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  }),
                  const SizedBox(height: EventXSpacing.lg),
                  _ScoreExplanationCard(),
                ],
              );
            },
          ),
        ],
      ),
    );
  }
}

class _RankingFilters extends StatelessWidget {
  const _RankingFilters({
    required this.selectedCategory,
    required this.selectedCity,
    required this.selectedState,
    required this.onCategoryChanged,
    required this.onCityChanged,
    required this.onStateChanged,
  });

  final String? selectedCategory;
  final String? selectedCity;
  final String? selectedState;
  final ValueChanged<String> onCategoryChanged;
  final ValueChanged<String> onCityChanged;
  final ValueChanged<String> onStateChanged;

  static const categories = [
    'Decoracao',
    'Buffet',
    'Musica',
    'Foto e Video',
    'Estrutura',
    'Doceria',
  ];

  static const cities = [
    'Sao Paulo',
    'Campinas',
    'Santos',
    'Sorocaba',
    'Ribeirao Preto',
  ];

  static const states = ['SP', 'RJ', 'MG', 'PR', 'SC'];

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Filtrar ranking',
            style: TextStyle(fontWeight: FontWeight.w700),
          ),
          const SizedBox(height: EventXSpacing.xs),
          Wrap(
            spacing: EventXSpacing.xs,
            runSpacing: EventXSpacing.xs,
            children: [
              ...categories.map(
                (item) => FilterChip(
                  label: Text(item),
                  selected: selectedCategory == item,
                  onSelected: (_) => onCategoryChanged(item),
                ),
              ),
              ...cities.map(
                (item) => FilterChip(
                  label: Text(item),
                  selected: selectedCity == item,
                  onSelected: (_) => onCityChanged(item),
                ),
              ),
              ...states.map(
                (item) => FilterChip(
                  label: Text(item),
                  selected: selectedState == item,
                  onSelected: (_) => onStateChanged(item),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _ScoreExplanationCard extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: const Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Como a pontuacao e calculada',
            style: TextStyle(fontWeight: FontWeight.w700),
          ),
          SizedBox(height: EventXSpacing.xs),
          Text('1) Qualidade da nota media + confianca de avaliacoes'),
          Text('2) Pedidos concluidos + eventos atendidos'),
          Text('3) Taxa de resposta + taxa de aceitacao'),
          Text('4) Penalidade por cancelamentos'),
          SizedBox(height: EventXSpacing.xs),
          Text(
            'A formula prioriza confiabilidade real, nao apenas volume bruto.',
            style: TextStyle(
              color: EventXColors.organizerTextMuted,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}

class _RankingMessageCard extends StatelessWidget {
  const _RankingMessageCard({
    required this.title,
    required this.message,
    required this.actionLabel,
    this.onAction,
  });

  final String title;
  final String message;
  final String actionLabel;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(EventXSpacing.lg),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Column(
        children: [
          const Icon(
            Icons.insights_outlined,
            size: 30,
            color: EventXColors.organizerTextMuted,
          ),
          const SizedBox(height: EventXSpacing.sm),
          Text(
            title,
            textAlign: TextAlign.center,
            style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 18),
          ),
          const SizedBox(height: EventXSpacing.xs),
          Text(
            message,
            textAlign: TextAlign.center,
            style: const TextStyle(color: EventXColors.organizerTextMuted),
          ),
          const SizedBox(height: EventXSpacing.md),
          FilledButton.tonal(
            onPressed: onAction ??
                () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerMarketplace),
            child: Text(actionLabel),
          ),
        ],
      ),
    );
  }
}

class _RankingSkeleton extends StatelessWidget {
  const _RankingSkeleton();

  @override
  Widget build(BuildContext context) {
    return const Column(
      children: [
        AppSkeletonCard(height: 128),
        AppSkeletonCard(height: 128),
        AppSkeletonCard(height: 128),
      ],
    );
  }
}

class _RankingViewData {
  const _RankingViewData({
    required this.ranking,
    required this.top,
  });

  const _RankingViewData.empty() : this(ranking: const [], top: const []);

  final List<RankingEntry> ranking;
  final List<RankingEntry> top;
}
