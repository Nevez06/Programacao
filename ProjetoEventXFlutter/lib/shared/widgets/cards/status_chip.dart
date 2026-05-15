import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';

class StatusChip extends StatelessWidget {
  const StatusChip({
    required this.label,
    this.color,
    super.key,
  });

  final String label;
  final Color? color;

  @override
  Widget build(BuildContext context) {
    return StatusBadge(label: label, color: color);
  }
}
