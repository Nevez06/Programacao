import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';

class AppSkeletonCard extends StatefulWidget {
  const AppSkeletonCard({
    this.height = 120,
    this.width,
    this.borderRadius = EventXRadius.lg,
    this.margin = const EdgeInsets.symmetric(
      horizontal: EventXSpacing.md,
      vertical: EventXSpacing.xs,
    ),
    super.key,
  });

  final double height;
  final double? width;
  final double borderRadius;
  final EdgeInsetsGeometry margin;

  @override
  State<AppSkeletonCard> createState() => _AppSkeletonCardState();
}

class _AppSkeletonCardState extends State<AppSkeletonCard>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: AppDurations.skeletonPulse,
    )..repeat(reverse: true);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final base = isDark
        ? EventXColors.socialSurface
        : EventXColors.organizerSurfaceAlt.withValues(alpha: 0.45);

    return Container(
      width: widget.width,
      height: widget.height,
      margin: widget.margin,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(widget.borderRadius),
        border: Border.all(
          color:
              isDark ? EventXColors.socialStroke : EventXColors.organizerStroke,
        ),
      ),
      clipBehavior: Clip.antiAlias,
      child: AnimatedBuilder(
        animation: _controller,
        builder: (context, child) {
          return DecoratedBox(
            decoration: BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment(-1 + (_controller.value * 2), -0.2),
                end: Alignment(1 + (_controller.value * 2), 0.2),
                colors: [
                  base.withValues(alpha: isDark ? 0.65 : 0.6),
                  base.withValues(alpha: isDark ? 0.95 : 0.85),
                  base.withValues(alpha: isDark ? 0.65 : 0.6),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
