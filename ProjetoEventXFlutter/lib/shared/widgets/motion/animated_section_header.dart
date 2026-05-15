import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/fade_slide_in.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class AnimatedSectionHeader extends StatelessWidget {
  const AnimatedSectionHeader({
    required this.title,
    this.subtitle,
    this.trailing,
    this.delay = Duration.zero,
    super.key,
  });

  final String title;
  final String? subtitle;
  final Widget? trailing;
  final Duration delay;

  @override
  Widget build(BuildContext context) {
    return FadeSlideIn(
      delay: delay,
      duration: AppDurations.normal,
      offset: const Offset(0, 0.025),
      child: SectionHeader(
        title: title,
        subtitle: subtitle,
        trailing: trailing,
      ),
    );
  }
}
