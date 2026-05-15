import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class BudgetSummaryCard extends StatelessWidget {
  const BudgetSummaryCard({
    required this.eventName,
    required this.spent,
    required this.totalBudget,
    this.pendingLabel,
    this.onTap,
    super.key,
  });

  final String eventName;
  final double spent;
  final double totalBudget;
  final String? pendingLabel;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final progress =
        totalBudget <= 0 ? 0.0 : (spent / totalBudget).clamp(0.0, 1.0);
    final free = (totalBudget - spent).clamp(0.0, totalBudget);
    final statusColor = progress >= 0.9
        ? EventXColors.error
        : progress >= 0.75
            ? EventXColors.warning
            : EventXColors.success;

    return InkWell(
      borderRadius: BorderRadius.circular(EventXRadius.lg),
      onTap: onTap,
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
              children: [
                Expanded(
                  child: Text(
                    eventName,
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.w700,
                        ),
                  ),
                ),
                Text(
                  '${(progress * 100).toStringAsFixed(0)}%',
                  style: TextStyle(
                    color: statusColor,
                    fontWeight: FontWeight.w800,
                  ),
                ),
              ],
            ),
            const SizedBox(height: EventXSpacing.xs),
            Text(
              'Gasto ${_formatCurrency(spent)} de ${_formatCurrency(totalBudget)}',
              style: Theme.of(context).textTheme.bodyMedium,
            ),
            const SizedBox(height: EventXSpacing.sm),
            ClipRRect(
              borderRadius: BorderRadius.circular(EventXRadius.pill),
              child: LinearProgressIndicator(
                value: progress,
                minHeight: 8,
                backgroundColor: EventXColors.organizerSurfaceAlt,
                valueColor: AlwaysStoppedAnimation<Color>(statusColor),
              ),
            ),
            const SizedBox(height: EventXSpacing.xs),
            Row(
              children: [
                Expanded(
                  child: Text(
                    'Disponivel ${_formatCurrency(free)}',
                    style: const TextStyle(
                      fontWeight: FontWeight.w600,
                      color: EventXColors.organizerTextMuted,
                      fontSize: 12,
                    ),
                  ),
                ),
                if (pendingLabel != null)
                  Text(
                    pendingLabel!,
                    style: const TextStyle(
                      fontWeight: FontWeight.w600,
                      color: EventXColors.organizerTextMuted,
                      fontSize: 12,
                    ),
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  static String _formatCurrency(double value) {
    final formatted = value.toStringAsFixed(0);
    return 'R\$ $formatted';
  }
}
