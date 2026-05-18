import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/hover_card.dart';

class TemplateCard extends StatelessWidget {
  const TemplateCard({
    required this.name,
    required this.category,
    this.previewLabel,
    this.previewGradient,
    this.onTap,
    super.key,
  });

  final String name;
  final String category;
  final String? previewLabel;
  final List<Color>? previewGradient;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return HoverCard(
      onTap: onTap,
      borderRadius: BorderRadius.circular(EventXRadius.lg),
      hoverScale: 1.012,
      hoverTranslateY: -5,
      baseShadow: EventXShadows.soft,
      hoverShadow: EventXShadows.card,
      child: Material(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(EventXRadius.lg),
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(EventXRadius.lg),
              border: Border.all(color: EventXColors.organizerStroke),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Container(
                  height: 142,
                  decoration: BoxDecoration(
                    borderRadius: const BorderRadius.only(
                      topLeft: Radius.circular(EventXRadius.lg),
                      topRight: Radius.circular(EventXRadius.lg),
                    ),
                    gradient: LinearGradient(
                      colors: previewGradient ??
                          const [Color(0xFFFAE0E0), Color(0xFFF8C7C0)],
                      begin: Alignment.topLeft,
                      end: Alignment.bottomRight,
                    ),
                  ),
                  child: Stack(
                    children: [
                      Positioned(
                        right: 12,
                        top: 12,
                        child: Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 8,
                            vertical: 4,
                          ),
                          decoration: BoxDecoration(
                            color: Colors.white.withValues(alpha: 0.78),
                            borderRadius:
                                BorderRadius.circular(EventXRadius.pill),
                          ),
                          child: Text(
                            previewLabel ?? 'Template',
                            style: const TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w700,
                              color: EventXColors.organizerText,
                            ),
                          ),
                        ),
                      ),
                      Align(
                        alignment: Alignment.center,
                        child: Container(
                          width: 128,
                          height: 82,
                          padding: const EdgeInsets.all(8),
                          decoration: BoxDecoration(
                            color: Colors.white.withValues(alpha: 0.82),
                            borderRadius:
                                BorderRadius.circular(EventXRadius.sm),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Container(
                                width: 56,
                                height: 7,
                                decoration: BoxDecoration(
                                  color: EventXColors.brand
                                      .withValues(alpha: 0.25),
                                  borderRadius: BorderRadius.circular(
                                    EventXRadius.pill,
                                  ),
                                ),
                              ),
                              const SizedBox(height: 8),
                              Container(
                                width: double.infinity,
                                height: 24,
                                decoration: BoxDecoration(
                                  color: EventXColors.brand
                                      .withValues(alpha: 0.12),
                                  borderRadius:
                                      BorderRadius.circular(EventXRadius.sm),
                                ),
                              ),
                              const Spacer(),
                              Row(
                                children: [
                                  Container(
                                    width: 28,
                                    height: 6,
                                    decoration: BoxDecoration(
                                      color: EventXColors.organizerTextMuted
                                          .withValues(alpha: 0.2),
                                      borderRadius: BorderRadius.circular(
                                        EventXRadius.pill,
                                      ),
                                    ),
                                  ),
                                  const SizedBox(width: 6),
                                  Expanded(
                                    child: Container(
                                      height: 6,
                                      decoration: BoxDecoration(
                                        color: EventXColors.organizerTextMuted
                                            .withValues(alpha: 0.2),
                                        borderRadius: BorderRadius.circular(
                                          EventXRadius.pill,
                                        ),
                                      ),
                                    ),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.all(EventXSpacing.sm),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        name,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(height: 2),
                      Text(
                        category,
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                      const SizedBox(height: EventXSpacing.sm),
                      FilledButton.tonalIcon(
                        onPressed: onTap,
                        icon:
                            const Icon(Icons.auto_fix_high_outlined, size: 18),
                        label: const Text('Usar template'),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
