import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_button_styles.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';
import 'package:projeto_eventx_flutter/core/theme/app_input_styles.dart';
import 'package:projeto_eventx_flutter/core/theme/app_radius.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';
import 'package:projeto_eventx_flutter/core/theme/app_text_styles.dart';

class AppTheme {
  AppTheme._();

  static ThemeData light() {
    final scheme = ColorScheme.fromSeed(
      seedColor: AppColors.primary,
      brightness: Brightness.light,
      primary: AppColors.primary,
      secondary: AppColors.secondary,
      surface: AppColors.organizerSurface,
      error: AppColors.error,
    );

    return ThemeData(
      useMaterial3: true,
      visualDensity: VisualDensity.adaptivePlatformDensity,
      colorScheme: scheme,
      fontFamily: AppTextStyles.fontFamily,
      scaffoldBackgroundColor: AppColors.organizerBackground,
      textTheme: AppTextStyles.organizerTextTheme(),
      cardTheme: CardThemeData(
        color: AppColors.organizerCard,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.lg),
          side: const BorderSide(color: AppColors.organizerBorder),
        ),
      ),
      appBarTheme: const AppBarTheme(
        centerTitle: false,
        backgroundColor: Colors.transparent,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        titleTextStyle: AppTextStyles.titleLarge,
      ),
      inputDecorationTheme: AppInputStyles.organizer(),
      filledButtonTheme:
          FilledButtonThemeData(style: AppButtonStyles.primary()),
      outlinedButtonTheme:
          OutlinedButtonThemeData(style: AppButtonStyles.secondary()),
      textButtonTheme: TextButtonThemeData(style: AppButtonStyles.ghost()),
      dividerTheme: const DividerThemeData(color: AppColors.border, space: 1),
      chipTheme: ChipThemeData(
        padding: const EdgeInsets.symmetric(horizontal: AppSpacing.sm),
        side: const BorderSide(color: AppColors.border),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.pill),
        ),
      ),
      bottomNavigationBarTheme: const BottomNavigationBarThemeData(
        type: BottomNavigationBarType.fixed,
        selectedItemColor: AppColors.primary,
        unselectedItemColor: AppColors.textMuted,
      ),
    );
  }

  static ThemeData dark() {
    final scheme = ColorScheme.fromSeed(
      seedColor: AppColors.socialAccent,
      brightness: Brightness.dark,
      primary: AppColors.socialAccent,
      secondary: AppColors.socialAccentAlt,
      surface: AppColors.socialSurface,
      error: AppColors.error,
    );

    return ThemeData(
      useMaterial3: true,
      visualDensity: VisualDensity.adaptivePlatformDensity,
      colorScheme: scheme,
      brightness: Brightness.dark,
      fontFamily: AppTextStyles.fontFamily,
      scaffoldBackgroundColor: AppColors.socialBackground,
      textTheme: AppTextStyles.socialTextTheme(),
      appBarTheme: const AppBarTheme(
        centerTitle: false,
        backgroundColor: Colors.transparent,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        foregroundColor: AppColors.socialTextPrimary,
        titleTextStyle: AppTextStyles.titleLarge,
      ),
      cardTheme: CardThemeData(
        color: AppColors.socialCard,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.lg),
          side: const BorderSide(color: AppColors.socialBorder),
        ),
      ),
      inputDecorationTheme: AppInputStyles.social(),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size(0, 48),
          backgroundColor: AppColors.socialAccent,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppRadius.md),
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size(0, 46),
          foregroundColor: AppColors.socialTextPrimary,
          side: const BorderSide(color: AppColors.socialBorder),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppRadius.md),
          ),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          minimumSize: const Size(0, 44),
          foregroundColor: AppColors.socialTextMuted,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(AppRadius.md),
          ),
        ),
      ),
      dividerTheme:
          const DividerThemeData(color: AppColors.socialBorder, space: 1),
      bottomNavigationBarTheme: const BottomNavigationBarThemeData(
        type: BottomNavigationBarType.fixed,
        backgroundColor: AppColors.socialSurface,
        selectedItemColor: AppColors.socialAccent,
        unselectedItemColor: AppColors.socialTextMuted,
      ),
    );
  }

  static ThemeData socialDark() => dark();
}
