import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';

class EventXColors {
  EventXColors._();

  // Brand / Organizer (compat)
  static const brand = AppColors.primary;
  static const brandDark = AppColors.primaryDark;
  static const accent = AppColors.secondary;
  static const success = AppColors.success;
  static const warning = AppColors.warning;
  static const error = AppColors.error;
  static const info = AppColors.info;

  static const organizerBackground = AppColors.organizerBackground;
  static const organizerSurface = AppColors.organizerSurface;
  static const organizerSurfaceAlt = AppColors.primarySoft;
  static const organizerStroke = AppColors.organizerBorder;
  static const organizerText = AppColors.organizerTextPrimary;
  static const organizerTextMuted = AppColors.organizerTextMuted;

  // Social
  static const socialBackground = AppColors.socialBackground;
  static const socialSurface = AppColors.socialSurface;
  static const socialSurfaceAlt = AppColors.socialCard;
  static const socialStroke = AppColors.socialBorder;
  static const socialText = AppColors.socialTextPrimary;
  static const socialTextMuted = AppColors.socialTextMuted;
  static const socialAccent = AppColors.socialAccent;
  static const socialAccentAlt = AppColors.socialAccentAlt;

  static Color withAlpha(Color color, double alpha) =>
      color.withValues(alpha: alpha.clamp(0, 1));
}
