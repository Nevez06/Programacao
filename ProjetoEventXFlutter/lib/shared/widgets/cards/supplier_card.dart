import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/hover_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';

class SupplierCard extends StatelessWidget {
  const SupplierCard({
    required this.name,
    required this.category,
    required this.city,
    required this.rating,
    required this.startingPrice,
    this.highlightLabel,
    this.onTap,
    super.key,
  });

  final String name;
  final String category;
  final String city;
  final double rating;
  final String startingPrice;
  final String? highlightLabel;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final safeRating = rating.clamp(0, 5);
    return HoverCard(
      onTap: onTap,
      borderRadius: BorderRadius.circular(EventXRadius.lg),
      hoverTranslateY: -4,
      hoverScale: 1.008,
      baseShadow: EventXShadows.soft,
      hoverShadow: EventXShadows.card,
      child: Material(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        child: InkWell(
          borderRadius: BorderRadius.circular(EventXRadius.lg),
          onTap: onTap,
          child: Container(
            padding: const EdgeInsets.all(EventXSpacing.md),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(EventXRadius.lg),
              border: Border.all(color: EventXColors.organizerStroke),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    CircleAvatar(
                      radius: 22,
                      backgroundColor:
                          EventXColors.brand.withValues(alpha: 0.1),
                      child: Text(
                        name.trim().isEmpty
                            ? 'S'
                            : name.trim().characters.first,
                        style: const TextStyle(
                          color: EventXColors.brand,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    const SizedBox(width: EventXSpacing.sm),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            name,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: Theme.of(context).textTheme.titleMedium,
                          ),
                          Text(
                            '$category • $city',
                            style: Theme.of(context).textTheme.bodyMedium,
                          ),
                        ],
                      ),
                    ),
                    if (highlightLabel != null)
                      StatusBadge(label: highlightLabel!),
                  ],
                ),
                const SizedBox(height: EventXSpacing.sm),
                Wrap(
                  spacing: EventXSpacing.xs,
                  runSpacing: EventXSpacing.xs,
                  children: [
                    _MetaPill(icon: Icons.location_on_outlined, text: city),
                    _MetaPill(icon: Icons.sell_outlined, text: category),
                  ],
                ),
                const SizedBox(height: EventXSpacing.sm),
                Row(
                  children: [
                    ...List.generate(
                      5,
                      (index) => Icon(
                        index < safeRating.round()
                            ? Icons.star_rounded
                            : Icons.star_border_rounded,
                        color: EventXColors.warning,
                        size: 16,
                      ),
                    ),
                    const SizedBox(width: 6),
                    Text(
                      rating.toStringAsFixed(1),
                      style: const TextStyle(fontWeight: FontWeight.w700),
                    ),
                    const Spacer(),
                    Text(
                      'A partir de $startingPrice',
                      style: const TextStyle(fontWeight: FontWeight.w700),
                    ),
                  ],
                ),
                const SizedBox(height: EventXSpacing.sm),
                Row(
                  children: [
                    Expanded(
                      child: FilledButton.icon(
                        onPressed: onTap,
                        icon: const Icon(Icons.storefront_outlined),
                        label: const Text('Ver fornecedor'),
                      ),
                    ),
                    const SizedBox(width: EventXSpacing.xs),
                    IconButton.filledTonal(
                      onPressed: onTap,
                      icon: const Icon(Icons.bookmark_border_rounded),
                      tooltip: 'Salvar fornecedor',
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _MetaPill extends StatelessWidget {
  const _MetaPill({
    required this.icon,
    required this.text,
  });

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
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
            text,
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
