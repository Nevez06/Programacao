import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/marketplace/data/models/marketplace_product_model.dart';
import 'package:projeto_eventx_flutter/features/marketplace/presentation/widgets/marketplace_product_card.dart';

class MarketplaceProductGrid extends StatelessWidget {
  const MarketplaceProductGrid({
    required this.products,
    super.key,
  });

  final List<MarketplaceProductModel> products;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 860),
        child: Padding(
          padding: const EdgeInsets.fromLTRB(
            EventXSpacing.md,
            0,
            EventXSpacing.md,
            EventXSpacing.lg,
          ),
          child: GridView.builder(
            itemCount: products.length,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 2,
              crossAxisSpacing: EventXSpacing.xs,
              mainAxisSpacing: EventXSpacing.sm,
              childAspectRatio: 0.74,
            ),
            itemBuilder: (context, index) {
              return MarketplaceProductCard(product: products[index]);
            },
          ),
        ),
      ),
    );
  }
}
