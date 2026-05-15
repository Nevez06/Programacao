import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/fade_slide_in.dart';

class StaggeredListReveal extends StatelessWidget {
  const StaggeredListReveal({
    required this.children,
    this.delay = Duration.zero,
    this.step = AppDurations.staggerStep,
    this.offset = const Offset(0, 0.03),
    super.key,
  });

  final List<Widget> children;
  final Duration delay;
  final Duration step;
  final Offset offset;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: List.generate(children.length, (index) {
        return FadeSlideIn(
          delay: delay + step * index,
          offset: offset,
          child: children[index],
        );
      }),
    );
  }
}
