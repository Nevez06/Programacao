import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';

class PrimaryButton extends StatefulWidget {
  const PrimaryButton({
    required this.label,
    required this.onPressed,
    this.isLoading = false,
    this.icon,
    this.expand = false,
    super.key,
  });

  final String label;
  final VoidCallback? onPressed;
  final bool isLoading;
  final IconData? icon;
  final bool expand;

  @override
  State<PrimaryButton> createState() => _PrimaryButtonState();
}

class _PrimaryButtonState extends State<PrimaryButton> {
  bool _hovering = false;
  bool _pressed = false;

  @override
  Widget build(BuildContext context) {
    final enabled = !widget.isLoading && widget.onPressed != null;
    final scale = _pressed
        ? 0.985
        : _hovering
            ? 1.01
            : 1.0;

    Widget button = SizedBox(
      height: 50,
      child: MouseRegion(
        onEnter: (_) => setState(() => _hovering = true),
        onExit: (_) => setState(() {
          _hovering = false;
          _pressed = false;
        }),
        child: GestureDetector(
          onTapDown: enabled ? (_) => setState(() => _pressed = true) : null,
          onTapCancel: enabled ? () => setState(() => _pressed = false) : null,
          onTapUp: enabled ? (_) => setState(() => _pressed = false) : null,
          child: AnimatedScale(
            duration: AppDurations.fast,
            curve: AppCurves.easeInOut,
            scale: scale,
            child: FilledButton.icon(
              onPressed: widget.isLoading ? null : widget.onPressed,
              style: ButtonStyle(
                animationDuration: AppDurations.fast,
                elevation: WidgetStateProperty.resolveWith<double>(
                  (states) {
                    if (states.contains(WidgetState.disabled)) {
                      return 0;
                    }
                    if (states.contains(WidgetState.pressed)) {
                      return 0;
                    }
                    if (states.contains(WidgetState.hovered)) {
                      return 2;
                    }
                    return 1;
                  },
                ),
              ),
              icon: AnimatedSwitcher(
                duration: AppDurations.fast,
                switchInCurve: AppCurves.ease,
                switchOutCurve: AppCurves.easeIn,
                child: widget.isLoading
                    ? const SizedBox(
                        key: ValueKey('loading'),
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : Icon(
                        widget.icon ?? Icons.check,
                        key: const ValueKey('icon'),
                      ),
              ),
              label: AnimatedSwitcher(
                duration: AppDurations.fast,
                switchInCurve: AppCurves.ease,
                switchOutCurve: AppCurves.easeIn,
                child: Text(
                  widget.isLoading ? 'Carregando...' : widget.label,
                  key: ValueKey(widget.isLoading),
                ),
              ),
            ),
          ),
        ),
      ),
    );

    if (!widget.expand) {
      return button;
    }

    return SizedBox(width: double.infinity, child: button);
  }
}

class AnimatedPrimaryButton extends StatelessWidget {
  const AnimatedPrimaryButton({
    required this.label,
    required this.onPressed,
    this.isLoading = false,
    this.icon,
    this.expand = false,
    super.key,
  });

  final String label;
  final VoidCallback? onPressed;
  final bool isLoading;
  final IconData? icon;
  final bool expand;

  @override
  Widget build(BuildContext context) {
    return PrimaryButton(
      label: label,
      onPressed: onPressed,
      isLoading: isLoading,
      icon: icon,
      expand: expand,
    );
  }
}
