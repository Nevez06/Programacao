import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';

class AppShadows {
  AppShadows._();

  static const List<BoxShadow> light = [
    BoxShadow(
      color: Color(0x140E1320),
      blurRadius: 12,
      offset: Offset(0, 4),
    ),
  ];

  static const List<BoxShadow> medium = [
    BoxShadow(
      color: Color(0x1A0E1320),
      blurRadius: 18,
      offset: Offset(0, 8),
    ),
  ];

  static const List<BoxShadow> heavy = [
    BoxShadow(
      color: Color(0x240E1320),
      blurRadius: 28,
      offset: Offset(0, 12),
    ),
  ];

  static List<BoxShadow> socialGlow() => [
        BoxShadow(
          color: AppColors.socialAccent.withValues(alpha: 0.18),
          blurRadius: 28,
          offset: const Offset(0, 10),
        ),
      ];
}
