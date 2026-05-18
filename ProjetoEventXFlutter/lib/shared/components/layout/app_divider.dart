import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_colors.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';

class AppDivider extends StatelessWidget {
  const AppDivider({
    this.height = AppSpacing.md,
    super.key,
  });

  final double height;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.symmetric(vertical: height / 2),
      child: const Divider(
        height: 1,
        color: AppColors.border,
      ),
    );
  }
}
