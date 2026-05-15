import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';

class HoverCard extends StatefulWidget {
  const HoverCard({
    required this.child,
    this.onTap,
    this.borderRadius = const BorderRadius.all(Radius.circular(16)),
    this.duration = AppDurations.fast,
    this.hoverTranslateY = -4,
    this.hoverScale = 1.01,
    this.baseShadow = const [],
    this.hoverShadow = const [],
    this.clipBehavior = Clip.none,
    super.key,
  });

  final Widget child;
  final VoidCallback? onTap;
  final BorderRadius borderRadius;
  final Duration duration;
  final double hoverTranslateY;
  final double hoverScale;
  final List<BoxShadow> baseShadow;
  final List<BoxShadow> hoverShadow;
  final Clip clipBehavior;

  @override
  State<HoverCard> createState() => _HoverCardState();
}

class _HoverCardState extends State<HoverCard> {
  bool _hovering = false;
  bool _pressed = false;

  @override
  Widget build(BuildContext context) {
    final active = _hovering || _pressed;
    final scale = _pressed
        ? 0.992
        : active
            ? widget.hoverScale
            : 1.0;
    final translateY = _pressed
        ? 0.0
        : active
            ? widget.hoverTranslateY
            : 0.0;

    return MouseRegion(
      onEnter: (_) => setState(() => _hovering = true),
      onExit: (_) => setState(() {
        _hovering = false;
        _pressed = false;
      }),
      child: GestureDetector(
        onTap: widget.onTap,
        onTapDown: (_) => setState(() => _pressed = true),
        onTapCancel: () => setState(() => _pressed = false),
        onTapUp: (_) => setState(() => _pressed = false),
        behavior: HitTestBehavior.opaque,
        child: AnimatedScale(
          duration: widget.duration,
          curve: AppCurves.easeInOut,
          scale: scale,
          child: AnimatedContainer(
            duration: widget.duration,
            curve: AppCurves.easeInOut,
            clipBehavior: widget.clipBehavior,
            transform: Matrix4.translationValues(0, translateY, 0),
            decoration: BoxDecoration(
              borderRadius: widget.borderRadius,
              boxShadow: active ? widget.hoverShadow : widget.baseShadow,
            ),
            child: widget.child,
          ),
        ),
      ),
    );
  }
}
