import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';

class EmptySocialState extends StatelessWidget {
  const EmptySocialState({
    required this.title,
    required this.message,
    this.icon = Icons.celebration_outlined,
    this.actionLabel,
    this.onAction,
    super.key,
  });

  final String title;
  final String message;
  final IconData icon;
  final String? actionLabel;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(EventXSpacing.lg),
      decoration: BoxDecoration(
        color: EventXColors.socialSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.socialStroke),
      ),
      child: Column(
        children: [
          Container(
            width: 58,
            height: 58,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              gradient: LinearGradient(
                colors: [
                  EventXColors.socialAccent.withValues(alpha: 0.24),
                  EventXColors.socialAccentAlt.withValues(alpha: 0.24),
                ],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
            ),
            alignment: Alignment.center,
            child: Icon(icon, color: EventXColors.socialText),
          ),
          const SizedBox(height: EventXSpacing.md),
          Text(
            title,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: EventXColors.socialText,
              fontSize: 18,
              fontWeight: FontWeight.w800,
            ),
          ),
          const SizedBox(height: EventXSpacing.xs),
          Text(
            message,
            textAlign: TextAlign.center,
            style: const TextStyle(color: EventXColors.socialTextMuted),
          ),
          if (actionLabel != null && onAction != null) ...[
            const SizedBox(height: EventXSpacing.md),
            PrimaryButton(
              label: actionLabel!,
              icon: Icons.add_rounded,
              onPressed: onAction!,
            ),
          ],
        ],
      ),
    );
  }
}
