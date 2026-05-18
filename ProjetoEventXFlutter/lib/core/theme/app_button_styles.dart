import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';
import 'package:projeto_eventx_flutter/core/theme/app_radius.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';

class AppButtonStyles {
  AppButtonStyles._();

  static ButtonStyle primary() {
    return FilledButton.styleFrom(
      minimumSize: const Size(0, 48),
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.lg,
        vertical: AppSpacing.sm,
      ),
      backgroundColor: AppColors.primary,
      foregroundColor: Colors.white,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
      ),
    );
  }

  static ButtonStyle secondary() {
    return OutlinedButton.styleFrom(
      minimumSize: const Size(0, 46),
      foregroundColor: AppColors.textPrimary,
      side: const BorderSide(color: AppColors.border),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
      ),
    );
  }

  static ButtonStyle ghost() {
    return TextButton.styleFrom(
      minimumSize: const Size(0, 44),
      foregroundColor: AppColors.textSecondary,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
      ),
    );
  }

  static ButtonStyle danger() {
    return FilledButton.styleFrom(
      minimumSize: const Size(0, 46),
      backgroundColor: AppColors.error,
      foregroundColor: Colors.white,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
      ),
    );
  }
}
