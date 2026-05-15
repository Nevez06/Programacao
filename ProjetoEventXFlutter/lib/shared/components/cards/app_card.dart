import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_card_styles.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';

class AppCard extends StatelessWidget {
  const AppCard({
    required this.child,
    this.padding,
    this.margin,
    this.onTap,
    this.social = false,
    super.key,
  });

  final Widget child;
  final EdgeInsetsGeometry? padding;
  final EdgeInsetsGeometry? margin;
  final VoidCallback? onTap;
  final bool social;

  @override
  Widget build(BuildContext context) {
    final decoration =
        social ? AppCardStyles.social() : AppCardStyles.organizer();

    final content = Container(
      margin: margin,
      padding: padding ?? const EdgeInsets.all(AppSpacing.md),
      decoration: decoration,
      child: child,
    );

    if (onTap == null) {
      return content;
    }

    return InkWell(
      onTap: onTap,
      borderRadius: (decoration.borderRadius as BorderRadius?),
      child: content,
    );
  }
}
