import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class ProfileStatItem extends StatelessWidget {
  const ProfileStatItem({
    required this.label,
    required this.value,
    super.key,
  });

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Text(
          value,
          style: const TextStyle(
            color: EventXColors.socialText,
            fontWeight: FontWeight.w800,
            fontSize: 18,
          ),
        ),
        const SizedBox(height: 2),
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
}
