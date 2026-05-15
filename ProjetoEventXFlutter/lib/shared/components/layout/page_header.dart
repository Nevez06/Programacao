import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';
import 'package:projeto_eventx_flutter/core/theme/app_text_styles.dart';

class PageHeader extends StatelessWidget {
  const PageHeader({
    required this.title,
    this.subtitle,
    this.trailing,
    super.key,
  });

  final String title;
  final String? subtitle;
  final Widget? trailing;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: AppTextStyles.headlineMedium,
                ),
                if ((subtitle ?? '').isNotEmpty) ...[
                  const SizedBox(height: AppSpacing.xs),
                  Text(
                    subtitle!,
                    style: AppTextStyles.bodyMedium.copyWith(
                      color: AppColors.textMuted,
                    ),
                  ),
                ],
              ],
            ),
          ),
          if (trailing != null) ...[
            const SizedBox(width: AppSpacing.sm),
            trailing!,
          ],
        ],
      ),
    );
  }
}
