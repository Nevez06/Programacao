import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class StatusBadge extends StatelessWidget {
  const StatusBadge({
    required this.label,
    this.color,
    super.key,
  });

  final String label;
  final Color? color;

  @override
  Widget build(BuildContext context) {
    final resolvedColor = color ?? EventXColors.brand;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: resolvedColor.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(EventXRadius.pill),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: resolvedColor,
          fontWeight: FontWeight.w600,
          fontSize: 12,
        ),
      ),
    );
  }
}
