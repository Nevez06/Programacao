import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/hover_card.dart';

class MetricCard extends StatelessWidget {
  const MetricCard({
    required this.title,
    required this.value,
    required this.icon,
    this.trendText,
    this.trendPositive = true,
    super.key,
  });

  final String title;
  final String value;
  final IconData icon;
  final String? trendText;
  final bool trendPositive;

  @override
  Widget build(BuildContext context) {
    final trendColor =
        trendPositive ? EventXColors.success : EventXColors.error;
    return HoverCard(
      borderRadius: BorderRadius.circular(EventXRadius.lg),
      hoverScale: 1.006,
      hoverTranslateY: -3,
      baseShadow: EventXShadows.soft,
      hoverShadow: EventXShadows.card,
      child: Container(
        padding: const EdgeInsets.all(EventXSpacing.md),
        decoration: BoxDecoration(
          color: EventXColors.organizerSurface,
          border: Border.all(color: EventXColors.organizerStroke),
          borderRadius: BorderRadius.circular(EventXRadius.lg),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    title,
                    style: Theme.of(context).textTheme.bodyMedium,
                  ),
                ),
                Icon(icon, color: EventXColors.brand),
              ],
            ),
            const SizedBox(height: EventXSpacing.sm),
            Text(
              value,
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                    fontSize: 26,
                  ),
            ),
            if (trendText != null) ...[
              const SizedBox(height: EventXSpacing.xs),
              Text(
                trendText!,
                style:
                    TextStyle(color: trendColor, fontWeight: FontWeight.w600),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
