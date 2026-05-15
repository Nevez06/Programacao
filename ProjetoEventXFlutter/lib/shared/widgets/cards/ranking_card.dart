import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class RankingCard extends StatelessWidget {
  const RankingCard({
    required this.position,
    required this.name,
    required this.score,
    required this.deltaLabel,
    this.categoryLabel,
    this.isTopHighlight = false,
    super.key,
  });

  final int position;
  final String name;
  final int score;
  final String deltaLabel;
  final String? categoryLabel;
  final bool isTopHighlight;

  @override
  Widget build(BuildContext context) {
    final medalColor = switch (position) {
      1 => const Color(0xFFFFC840),
      2 => const Color(0xFFCED3DE),
      3 => const Color(0xFFC98A52),
      _ => EventXColors.organizerStroke,
    };

    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
        boxShadow: isTopHighlight ? EventXShadows.card : EventXShadows.soft,
      ),
      child: Row(
        children: [
          Container(
            width: isTopHighlight ? 38 : 34,
            height: isTopHighlight ? 38 : 34,
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: medalColor.withValues(alpha: 0.22),
              borderRadius: BorderRadius.circular(EventXRadius.pill),
            ),
            child: Text(
              '$position',
              style: TextStyle(
                color: medalColor == EventXColors.organizerStroke
                    ? EventXColors.organizerText
                    : medalColor,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(width: EventXSpacing.sm),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(name, style: Theme.of(context).textTheme.titleMedium),
                const SizedBox(height: 2),
                Text(deltaLabel, style: Theme.of(context).textTheme.bodyMedium),
                if (categoryLabel != null) ...[
                  const SizedBox(height: 4),
                  Text(
                    categoryLabel!,
                    style: const TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                      color: EventXColors.organizerTextMuted,
                    ),
                  ),
                ],
              ],
            ),
          ),
          Text(
            '$score pts',
            style: const TextStyle(
              fontWeight: FontWeight.w800,
              color: EventXColors.brand,
            ),
          ),
        ],
      ),
    );
  }
}
