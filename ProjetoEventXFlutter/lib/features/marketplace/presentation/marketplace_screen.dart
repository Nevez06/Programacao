import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/marketplace/data/marketplace_product_mock_data.dart';
import 'package:projeto_eventx_flutter/features/marketplace/data/models/marketplace_product_model.dart';
import 'package:projeto_eventx_flutter/features/marketplace/presentation/widgets/marketplace_action_buttons.dart';
import 'package:projeto_eventx_flutter/features/marketplace/presentation/widgets/marketplace_header.dart';
import 'package:projeto_eventx_flutter/features/marketplace/presentation/widgets/marketplace_product_grid.dart';

class MarketplaceScreen extends StatefulWidget {
  const MarketplaceScreen({super.key});

  @override
  State<MarketplaceScreen> createState() => _MarketplaceScreenState();
}

class _MarketplaceScreenState extends State<MarketplaceScreen> {
  final _searchController = TextEditingController();
  String _query = '';

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  List<MarketplaceProductModel> get _filteredProducts {
    final query = _query.trim().toLowerCase();
    if (query.isEmpty) {
      return marketplaceProductMockData;
    }

    return marketplaceProductMockData
        .where((product) => product.title.toLowerCase().contains(query))
        .toList(growable: false);
  }

  @override
  Widget build(BuildContext context) {
    final products = _filteredProducts;

    return DecoratedBox(
      decoration: const BoxDecoration(color: EventXColors.socialBackground),
      child: CustomScrollView(
        slivers: [
          SliverToBoxAdapter(
            child: MarketplaceHeader(
              controller: _searchController,
              onChanged: (value) => setState(() => _query = value),
              onMenuTap: _openMenu,
              onProfileTap: () {},
            ),
          ),
          const SliverToBoxAdapter(
            child: MarketplaceActionButtons(),
          ),
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.fromLTRB(
                EventXSpacing.md,
                EventXSpacing.lg,
                EventXSpacing.md,
                EventXSpacing.sm,
              ),
              child: Row(
                children: [
                  const Expanded(
                    child: Text(
                      'Selecoes de hoje',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(
                        color: EventXColors.socialText,
                        fontSize: 22,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                  ),
                  const SizedBox(width: EventXSpacing.sm),
                  Icon(
                    Icons.location_on_rounded,
                    color: EventXColors.info,
                    size: 20,
                  ),
                  const SizedBox(width: EventXSpacing.xs),
                  Text(
                    'Vitoria, ES',
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      color: EventXColors.info,
                      fontSize: 16,
                      fontWeight: FontWeight.w800,
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (products.isEmpty)
            SliverFillRemaining(
              hasScrollBody: false,
              child: _EmptyMarketplace(query: _query),
            )
          else
            SliverToBoxAdapter(
              child: MarketplaceProductGrid(products: products),
            ),
        ],
      ),
    );
  }

  void _openMenu() {
    final scaffold = Scaffold.maybeOf(context);
    if (scaffold?.hasDrawer ?? false) {
      scaffold?.openDrawer();
    }
  }
}

class _EmptyMarketplace extends StatelessWidget {
  const _EmptyMarketplace({required this.query});

  final String query;

  @override
  Widget build(BuildContext context) {
    final label = query.trim().isEmpty
        ? 'Nenhum produto encontrado'
        : 'Nada para "$query"';

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(EventXSpacing.xl),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.search_off_rounded,
              color: EventXColors.socialTextMuted.withValues(alpha: 0.75),
              size: 44,
            ),
            const SizedBox(height: EventXSpacing.sm),
            Text(
              label,
              textAlign: TextAlign.center,
              style: const TextStyle(
                color: EventXColors.socialText,
                fontSize: 18,
                fontWeight: FontWeight.w800,
              ),
            ),
            const SizedBox(height: EventXSpacing.xs),
            const Text(
              'Tente buscar por buffet, decoracao, transporte ou fotografia.',
              textAlign: TextAlign.center,
              style: TextStyle(color: EventXColors.socialTextMuted),
            ),
          ],
        ),
      ),
    );
  }
}
