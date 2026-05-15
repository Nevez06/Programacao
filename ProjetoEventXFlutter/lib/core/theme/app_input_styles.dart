import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';
import 'package:projeto_eventx_flutter/core/theme/app_radius.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';

class AppInputStyles {
  AppInputStyles._();

  static InputDecorationTheme organizer() {
    return InputDecorationTheme(
      filled: true,
      fillColor: AppColors.organizerSurface,
      isDense: true,
      contentPadding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.md,
        vertical: AppSpacing.md,
      ),
      labelStyle: const TextStyle(color: AppColors.textMuted),
      hintStyle: const TextStyle(color: AppColors.textMuted),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
        borderSide: const BorderSide(color: AppColors.border),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
        borderSide: const BorderSide(color: AppColors.border),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
        borderSide: const BorderSide(color: AppColors.primary, width: 1.4),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
        borderSide: const BorderSide(color: AppColors.error),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
        borderSide: const BorderSide(color: AppColors.error, width: 1.4),
      ),
    );
  }

  static InputDecorationTheme social() {
    return InputDecorationTheme(
      filled: true,
      fillColor: AppColors.socialSurface,
      isDense: true,
      contentPadding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.md,
        vertical: AppSpacing.md,
      ),
      labelStyle: const TextStyle(color: AppColors.socialTextMuted),
      hintStyle: const TextStyle(color: AppColors.socialTextMuted),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
        borderSide: const BorderSide(color: AppColors.socialBorder),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
        borderSide: const BorderSide(color: AppColors.socialBorder),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(AppRadius.md),
        borderSide: const BorderSide(color: AppColors.socialAccent, width: 1.4),
      ),
    );
  }
}
