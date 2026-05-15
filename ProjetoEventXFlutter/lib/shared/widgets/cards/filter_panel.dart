import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class FilterPanel extends StatelessWidget {
  const FilterPanel({
    required this.title,
    required this.chips,
    this.subtitle,
    this.actions = const <Widget>[],
    super.key,
  });

  final String title;
  final List<String> chips;
  final String? subtitle;
  final List<Widget> actions;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
        boxShadow: EventXShadows.soft,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title, style: Theme.of(context).textTheme.titleMedium),
          if (subtitle != null) ...[
            const SizedBox(height: 4),
            Text(
              subtitle!,
              style: Theme.of(context).textTheme.bodyMedium,
            ),
          ],
          const SizedBox(height: EventXSpacing.sm),
          Wrap(
            spacing: EventXSpacing.xs,
            runSpacing: EventXSpacing.xs,
            children: chips
                .map(
                  (item) => Chip(
                    label: Text(item),
                    visualDensity: VisualDensity.compact,
                  ),
                )
                .toList(),
          ),
          if (actions.isNotEmpty) ...[
            const SizedBox(height: EventXSpacing.sm),
            Wrap(
              spacing: EventXSpacing.xs,
              runSpacing: EventXSpacing.xs,
              children: actions,
            ),
          ],
        ],
      ),
    );
  }
}
