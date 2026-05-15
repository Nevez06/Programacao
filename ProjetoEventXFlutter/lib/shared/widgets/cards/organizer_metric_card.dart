import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/hover_card.dart';

class OrganizerMetricCard extends StatelessWidget {
  const OrganizerMetricCard({
    required this.title,
    required this.value,
    required this.icon,
    this.subtitle,
    this.trendText,
    this.trendPositive = true,
    this.highlightColor,
    super.key,
  });

  final String title;
  final String value;
  final IconData icon;
  final String? subtitle;
  final String? trendText;
  final bool trendPositive;
  final Color? highlightColor;

  @override
  Widget build(BuildContext context) {
    final brand = highlightColor ?? EventXColors.brand;
    final trendColor =
        trendPositive ? EventXColors.success : EventXColors.error;

    return HoverCard(
      borderRadius: BorderRadius.circular(EventXRadius.lg),
      hoverTranslateY: -3,
      hoverScale: 1.006,
      baseShadow: EventXShadows.soft,
      hoverShadow: EventXShadows.card,
      child: Container(
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
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        title,
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                      if (subtitle != null) ...[
                        const SizedBox(height: 2),
                        Text(
                          subtitle!,
                          style: const TextStyle(
                            fontSize: 12,
                            color: EventXColors.organizerTextMuted,
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
                Container(
                  width: 34,
                  height: 34,
                  decoration: BoxDecoration(
                    color: brand.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(EventXRadius.md),
                  ),
                  child: Icon(icon, color: brand, size: 18),
                ),
              ],
            ),
            const SizedBox(height: EventXSpacing.sm),
            Text(
              value,
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                    fontSize: 27,
                    fontWeight: FontWeight.w800,
                  ),
            ),
            if (trendText != null) ...[
              const SizedBox(height: EventXSpacing.xs),
              Text(
                trendText!,
                style: TextStyle(
                  color: trendColor,
                  fontWeight: FontWeight.w700,
                  fontSize: 12,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
