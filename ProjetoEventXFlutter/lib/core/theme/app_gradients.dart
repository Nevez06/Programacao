import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';

class AppGradients {
  AppGradients._();

  static const organizerHero = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [AppColors.primary, AppColors.secondary],
  );

  static const organizerSurface = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFFFDF8F8), AppColors.backgroundLight],
  );

  static const socialHero = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [AppColors.socialAccent, AppColors.socialAccentAlt],
  );

  static const socialBackground = LinearGradient(
    begin: Alignment.topCenter,
    end: Alignment.bottomCenter,
    colors: [Color(0xFF0D0F16), Color(0xFF121726)],
  );

  static const accentSoft = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0x22D93A3A), Color(0x22F28B54)],
  );
}
