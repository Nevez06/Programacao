import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/fade_slide_in.dart';

class AnimatedEmptyState extends StatelessWidget {
  const AnimatedEmptyState({
    required this.child,
    this.delay = const Duration(milliseconds: 80),
    super.key,
  });

  final Widget child;
  final Duration delay;

  @override
  Widget build(BuildContext context) {
    return FadeSlideIn(
      delay: delay,
      duration: AppDurations.slow,
      offset: const Offset(0, 0.06),
      child: TweenAnimationBuilder<double>(
        duration: AppDurations.slow,
        curve: AppCurves.ease,
        tween: Tween(begin: 0.97, end: 1),
        builder: (context, value, widgetChild) {
          return Transform.scale(
            scale: value,
            child: widgetChild,
          );
        },
        child: child,
      ),
    );
  }
}
