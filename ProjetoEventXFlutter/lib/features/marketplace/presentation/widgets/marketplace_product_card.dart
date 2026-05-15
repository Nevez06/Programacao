import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/marketplace/data/models/marketplace_product_model.dart';

class MarketplaceProductCard extends StatelessWidget {
  const MarketplaceProductCard({
    required this.product,
    super.key,
  });

  final MarketplaceProductModel product;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: EventXColors.socialSurface,
      borderRadius: BorderRadius.circular(EventXRadius.sm),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: () {},
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            AspectRatio(
              aspectRatio: 1,
              child: _ProductImage(imageUrl: product.imageUrl),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(
                EventXSpacing.xs,
                EventXSpacing.xs,
                EventXSpacing.xs,
                EventXSpacing.sm,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _ProductPrice(product: product),
                  const SizedBox(height: 3),
                  Text(
                    product.title,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: EventXColors.socialText,
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ProductImage extends StatelessWidget {
  const _ProductImage({required this.imageUrl});

  final String imageUrl;

  @override
  Widget build(BuildContext context) {
    return Image.network(
      imageUrl,
      fit: BoxFit.cover,
      loadingBuilder: (context, child, loadingProgress) {
        if (loadingProgress == null) {
          return child;
        }

        return const ColoredBox(
          color: EventXColors.socialSurfaceAlt,
          child: Center(
            child: CircularProgressIndicator(strokeWidth: 2),
          ),
        );
      },
      errorBuilder: (context, error, stackTrace) {
        return const ColoredBox(
          color: EventXColors.socialSurfaceAlt,
          child: Center(
            child: Icon(
              Icons.image_not_supported_outlined,
              color: EventXColors.socialTextMuted,
            ),
          ),
        );
      },
    );
  }
}

class _ProductPrice extends StatelessWidget {
  const _ProductPrice({required this.product});

  final MarketplaceProductModel product;

  @override
  Widget build(BuildContext context) {
    final previousPrice = product.previousPriceCents;

    return Row(
      children: [
        Expanded(
          child: Text(
            _formatBrl(product.priceCents),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              color: EventXColors.socialText,
              fontSize: 17,
              fontWeight: FontWeight.w900,
            ),
          ),
        ),
        if (previousPrice != null) ...[
          const SizedBox(width: EventXSpacing.xxs),
          Flexible(
            child: Text(
              _formatBrl(previousPrice),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(
                color: EventXColors.socialTextMuted,
                fontSize: 13,
                fontWeight: FontWeight.w700,
                decoration: TextDecoration.lineThrough,
                decorationColor: EventXColors.socialTextMuted,
              ),
            ),
          ),
        ],
      ],
    );
  }

  String _formatBrl(int cents) {
    final reais = cents ~/ 100;
    final centavos = (cents % 100).toString().padLeft(2, '0');
    final source = reais.toString();
    final buffer = StringBuffer();

    for (var i = 0; i < source.length; i++) {
      final positionFromEnd = source.length - i;
      buffer.write(source[i]);
      if (positionFromEnd > 1 && positionFromEnd % 3 == 1) {
        buffer.write('.');
      }
    }

    return 'R\$ ${buffer.toString()},$centavos';
  }
}
