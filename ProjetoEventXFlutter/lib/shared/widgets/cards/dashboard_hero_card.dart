import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';

class DashboardHeroCard extends StatelessWidget {
  const DashboardHeroCard({
    required this.title,
    required this.subtitle,
    required this.primaryActionLabel,
    required this.onPrimaryAction,
    this.secondaryActionLabel,
    this.onSecondaryAction,
    super.key,
  });

  final String title;
  final String subtitle;
  final String primaryActionLabel;
  final VoidCallback onPrimaryAction;
  final String? secondaryActionLabel;
  final VoidCallback? onSecondaryAction;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(EventXSpacing.lg),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(EventXRadius.xl),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [EventXColors.brand, EventXColors.accent],
        ),
        boxShadow: EventXShadows.card,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.w800,
                ),
          ),
          const SizedBox(height: EventXSpacing.xs),
          Text(
            subtitle,
            style: const TextStyle(
              color: Color(0xFFF6E8ED),
              fontSize: 14,
              height: 1.5,
            ),
          ),
          const SizedBox(height: EventXSpacing.lg),
          Wrap(
            spacing: EventXSpacing.sm,
            runSpacing: EventXSpacing.sm,
            children: [
              PrimaryButton(
                label: primaryActionLabel,
                icon: Icons.flash_on_rounded,
                onPressed: onPrimaryAction,
              ),
              if (secondaryActionLabel != null && onSecondaryAction != null)
                OutlinedButton.icon(
                  onPressed: onSecondaryAction,
                  icon: const Icon(Icons.open_in_new_rounded),
                  label: Text(secondaryActionLabel!),
                  style: OutlinedButton.styleFrom(
                    side: const BorderSide(color: Color(0x7AFFFFFF)),
                    foregroundColor: Colors.white,
                    backgroundColor: const Color(0x10FFFFFF),
                  ),
                ),
            ],
          ),
        ],
      ),
    );
  }
}
