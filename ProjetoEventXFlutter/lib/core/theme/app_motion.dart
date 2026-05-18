import 'dart:ui';

import 'package:flutter/material.dart';

class AppDurations {
  AppDurations._();

  static const fast = Duration(milliseconds: 140);
  static const normal = Duration(milliseconds: 220);
  static const slow = Duration(milliseconds: 320);
  static const slower = Duration(milliseconds: 460);
  static const staggerStep = Duration(milliseconds: 42);
  static const skeletonPulse = Duration(milliseconds: 1200);
}

class AppCurves {
  AppCurves._();

  static const ease = Curves.easeOutCubic;
  static const easeIn = Curves.easeInCubic;
  static const easeInOut = Curves.easeInOutCubic;
  static const emphasis = Cubic(0.2, 0.0, 0.0, 1.0);
}

enum AppRouteTransitionStyle {
  organizer,
  social,
  auth,
}

class AppMotion {
  AppMotion._();

  static PageRoute<T> buildRoute<T>({
    required Widget page,
    required RouteSettings settings,
    AppRouteTransitionStyle style = AppRouteTransitionStyle.organizer,
  }) {
    final beginOffset = switch (style) {
      AppRouteTransitionStyle.organizer => const Offset(0, 0.04),
      AppRouteTransitionStyle.social => const Offset(0.04, 0),
      AppRouteTransitionStyle.auth => const Offset(0, 0.06),
    };

    final duration = switch (style) {
      AppRouteTransitionStyle.organizer => AppDurations.normal,
      AppRouteTransitionStyle.social => AppDurations.slow,
      AppRouteTransitionStyle.auth => AppDurations.slow,
    };

    return PageRouteBuilder<T>(
      settings: settings,
      transitionDuration: duration,
      reverseTransitionDuration: AppDurations.normal,
      pageBuilder: (_, __, ___) => page,
      transitionsBuilder: (_, animation, secondaryAnimation, child) {
        final fade = CurvedAnimation(
          parent: animation,
          curve: AppCurves.ease,
          reverseCurve: AppCurves.easeIn,
        );
        final slide = Tween<Offset>(
          begin: beginOffset,
          end: Offset.zero,
        ).animate(fade);

        final scale = Tween<double>(
          begin: style == AppRouteTransitionStyle.social ? 0.985 : 1,
          end: 1,
        ).animate(fade);

        return FadeTransition(
          opacity: fade,
          child: SlideTransition(
            position: slide,
            child: ScaleTransition(
              scale: scale,
              child: child,
            ),
          ),
        );
      },
    );
  }

  static Widget fadeSlide({
    required Animation<double> animation,
    required Widget child,
    Offset begin = const Offset(0, 0.04),
    Curve curve = AppCurves.ease,
  }) {
    final curved = CurvedAnimation(parent: animation, curve: curve);
    return FadeTransition(
      opacity: curved,
      child: SlideTransition(
        position: Tween<Offset>(
          begin: begin,
          end: Offset.zero,
        ).animate(curved),
        child: child,
      ),
    );
  }

  static Widget blurFade({
    required Animation<double> animation,
    required Widget child,
    double sigma = 4,
  }) {
    final curved = CurvedAnimation(parent: animation, curve: AppCurves.ease);
    return AnimatedBuilder(
      animation: curved,
      builder: (_, __) {
        final blur = lerpDouble(sigma, 0, curved.value) ?? 0;
        return Opacity(
          opacity: curved.value.clamp(0, 1),
          child: ImageFiltered(
            imageFilter: ImageFilter.blur(
              sigmaX: blur,
              sigmaY: blur,
            ),
            child: child,
          ),
        );
      },
    );
  }
}
