import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/hover_card.dart';

class ActionCard extends StatelessWidget {
  const ActionCard({
    required this.title,
    required this.description,
    required this.icon,
    this.buttonLabel = 'Abrir',
    this.onTap,
    this.highlightColor,
    super.key,
  });

  final String title;
  final String description;
  final IconData icon;
  final String buttonLabel;
  final VoidCallback? onTap;
  final Color? highlightColor;

  @override
  Widget build(BuildContext context) {
    final accent = highlightColor ?? EventXColors.brand;

    return HoverCard(
      onTap: onTap,
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
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              width: 36,
              height: 36,
              decoration: BoxDecoration(
                color: accent.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(EventXRadius.md),
              ),
              child: Icon(icon, color: accent, size: 18),
            ),
            const SizedBox(height: EventXSpacing.sm),
            Text(
              title,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.w700,
                  ),
            ),
            const SizedBox(height: 4),
            Text(
              description,
              style: Theme.of(context).textTheme.bodyMedium,
            ),
            const SizedBox(height: EventXSpacing.md),
            Align(
              alignment: Alignment.centerLeft,
              child: FilledButton.tonalIcon(
                onPressed: onTap,
                icon: const Icon(Icons.arrow_forward_rounded, size: 18),
                label: Text(buttonLabel),
              ),
            ),
          ],
        ),
      ),
    );
  }
}