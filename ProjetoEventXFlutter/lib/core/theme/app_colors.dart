import 'package:flutter/material.dart';

class AppColors {
  AppColors._();

  // Core brand
  static const primary = Color(0xFFD93A3A);
  static const primaryLight = Color(0xFFF2694A);
  static const primaryDark = Color(0xFFAA2029);
  static const primarySoft = Color(0xFFFBE8E7);
  static const secondary = Color(0xFFF28B54);

  // Neutral foundations
  static const backgroundLight = Color(0xFFF7F8FB);
  static const backgroundDark = Color(0xFF0D0F16);
  static const surface = Color(0xFFFFFFFF);
  static const card = Color(0xFFFFFFFF);
  static const border = Color(0xFFE4E7EF);

  static const textPrimary = Color(0xFF171C2A);
  static const textSecondary = Color(0xFF424A63);
  static const textMuted = Color(0xFF66708A);

  static const success = Color(0xFF1D8E5A);
  static const warning = Color(0xFFF5A623);
  static const error = Color(0xFFD93A3A);
  static const info = Color(0xFF2676FF);

  // Organizer (light SaaS)
  static const organizerBackground = backgroundLight;
  static const organizerSurface = surface;
  static const organizerCard = card;
  static const organizerBorder = border;
  static const organizerTextPrimary = textPrimary;
  static const organizerTextSecondary = textSecondary;
  static const organizerTextMuted = textMuted;

  // Social (dark premium)
  static const socialBackground = Color(0xFF0D0F16);
  static const socialSurface = Color(0xFF171A24);
  static const socialCard = Color(0xFF1A1F2C);
  static const socialBorder = Color(0xFF2C3243);
  static const socialTextPrimary = Color(0xFFF4F6FC);
  static const socialTextSecondary = Color(0xFFD9DDF1);
  static const socialTextMuted = Color(0xFFB8C0D8);
  static const socialAccent = Color(0xFFFF4F70);
  static const socialAccentAlt = Color(0xFFFF8A5B);

  static Color withAlpha(Color color, double alpha) =>
      color.withValues(alpha: alpha.clamp(0, 1));
}
