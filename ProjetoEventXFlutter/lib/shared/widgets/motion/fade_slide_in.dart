import 'dart:async';

import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';

class FadeSlideIn extends StatefulWidget {
  const FadeSlideIn({
    required this.child,
    this.delay = Duration.zero,
    this.duration = AppDurations.slow,
    this.offset = const Offset(0, 0.04),
    this.curve = AppCurves.ease,
    super.key,
  });

  final Widget child;
  final Duration delay;
  final Duration duration;
  final Offset offset;
  final Curve curve;

  @override
  State<FadeSlideIn> createState() => _FadeSlideInState();
}

class _FadeSlideInState extends State<FadeSlideIn>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: widget.duration,
    );

    if (widget.delay == Duration.zero) {
      _controller.forward();
    } else {
      _timer = Timer(widget.delay, _controller.forward);
    }
  }

  @override
  void dispose() {
    _timer?.cancel();
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      child: widget.child,
      builder: (context, child) {
        final animation = CurvedAnimation(
          parent: _controller,
          curve: widget.curve,
        );

        return AppMotion.fadeSlide(
          animation: animation,
          begin: widget.offset,
          curve: widget.curve,
          child: child ?? const SizedBox.shrink(),
        );
      },
    );
  }
}
