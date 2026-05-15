import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';

class AnimatedMetricCard extends StatelessWidget {
  const AnimatedMetricCard({
    required this.child,
    this.delay = Duration.zero,
    super.key,
  });

  final Widget child;
  final Duration delay;

  @override
  Widget build(BuildContext context) {
    return TweenAnimationBuilder<double>(
      duration: AppDurations.slow + delay,
      curve: AppCurves.easeInOut,
      tween: Tween(begin: 0, end: 1),
      builder: (context, value, widgetChild) {
        final opacity = value.clamp(0.0, 1.0).toDouble();
        final translateY = (1 - value) * 14;
        return Opacity(
          opacity: opacity,
          child: Transform.translate(
            offset: Offset(0, translateY),
            child: widgetChild,
          ),
        );
      },
      child: child,
    );
  }
}
