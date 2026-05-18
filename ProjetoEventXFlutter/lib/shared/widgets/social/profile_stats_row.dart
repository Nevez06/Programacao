import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class ProfileStatsRow extends StatelessWidget {
  const ProfileStatsRow({
    required this.posts,
    required this.followers,
    required this.following,
    super.key,
  });

  final int posts;
  final int followers;
  final int following;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(
        vertical: EventXSpacing.sm,
        horizontal: EventXSpacing.md,
      ),
      decoration: BoxDecoration(
        color: EventXColors.socialSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.socialStroke),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          _StatItem(label: 'Posts', value: posts),
          _StatDivider(),
          _StatItem(label: 'Seguidores', value: followers),
          _StatDivider(),
          _StatItem(label: 'Seguindo', value: following),
        ],
      ),
    );
  }
}

class _StatItem extends StatelessWidget {
  const _StatItem({required this.label, required this.value});

  final String label;
  final int value;

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          _compact(value),
          style: const TextStyle(
            color: EventXColors.socialText,
            fontSize: 18,
            fontWeight: FontWeight.w800,
          ),
        ),
        const SizedBox(height: 3),
        Text(
          label,
          style: const TextStyle(
            color: EventXColors.socialTextMuted,
            fontSize: 12,
          ),
        ),
      ],
    );
  }

  String _compact(int number) {
    if (number >= 1000000) {
      return '${(number / 1000000).toStringAsFixed(1)}M';
    }
    if (number >= 1000) {
      return '${(number / 1000).toStringAsFixed(1)}k';
    }
    return number.toString();
  }
}

class _StatDivider extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      width: 1,
      height: 34,
      color: EventXColors.socialStroke,
    );
  }
}
