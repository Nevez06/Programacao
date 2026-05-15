import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class HighlightCard extends StatelessWidget {
  const HighlightCard({
    required this.title,
    required this.subtitle,
    this.imageUrl,
    this.onTap,
    this.isAdd = false,
    super.key,
  });

  final String title;
  final String subtitle;
  final String? imageUrl;
  final VoidCallback? onTap;
  final bool isAdd;

  @override
  Widget build(BuildContext context) {
    final gradient = isAdd
        ? const [Color(0x6646508A), Color(0x663C2861)]
        : const [Color(0xFF2B2F42), Color(0xFF1B2033)];

    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(EventXRadius.lg),
      child: Container(
        width: 160,
        padding: const EdgeInsets.all(EventXSpacing.sm),
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: gradient,
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(EventXRadius.lg),
          border: Border.all(color: EventXColors.socialStroke),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: ClipRRect(
                borderRadius: BorderRadius.circular(EventXRadius.md),
                child: _Preview(imageUrl: imageUrl, isAdd: isAdd),
              ),
            ),
            const SizedBox(height: EventXSpacing.xs),
            Text(
              title,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(
                color: EventXColors.socialText,
                fontWeight: FontWeight.w700,
              ),
            ),
            Text(
              subtitle,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(
                color: EventXColors.socialTextMuted,
                fontSize: 12,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Preview extends StatelessWidget {
  const _Preview({required this.imageUrl, required this.isAdd});

  final String? imageUrl;
  final bool isAdd;

  @override
  Widget build(BuildContext context) {
    if (isAdd) {
      return Container(
        color: const Color(0x222A2D44),
        alignment: Alignment.center,
        child: const Icon(Icons.add_circle_outline_rounded,
            color: EventXColors.socialText),
      );
    }

    if ((imageUrl ?? '').isNotEmpty) {
      return Image.network(
        imageUrl!,
        fit: BoxFit.cover,
        errorBuilder: (_, __, ___) => _placeholder(),
      );
    }

    return _placeholder();
  }

  Widget _placeholder() {
    return Container(
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            EventXColors.socialAccent.withValues(alpha: 0.32),
            EventXColors.socialAccentAlt.withValues(alpha: 0.24),
          ],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
      ),
      child: const Center(
        child: Icon(Icons.auto_awesome_mosaic_outlined,
            color: EventXColors.socialText),
      ),
    );
  }
}
