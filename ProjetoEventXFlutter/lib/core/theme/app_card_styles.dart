import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';
import 'package:projeto_eventx_flutter/core/theme/app_radius.dart';
import 'package:projeto_eventx_flutter/core/theme/app_shadows.dart';

class AppCardStyles {
  AppCardStyles._();

  static BoxDecoration organizer() {
    return BoxDecoration(
      color: AppColors.organizerCard,
      border: Border.all(color: AppColors.organizerBorder),
      borderRadius: BorderRadius.circular(AppRadius.lg),
      boxShadow: AppShadows.light,
    );
  }

  static BoxDecoration social() {
    return BoxDecoration(
      color: AppColors.socialCard,
      border: Border.all(color: AppColors.socialBorder),
      borderRadius: BorderRadius.circular(AppRadius.lg),
      boxShadow: AppShadows.medium,
    );
  }
}
