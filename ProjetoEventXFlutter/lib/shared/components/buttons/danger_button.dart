import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_button_styles.dart';

class DangerButton extends StatelessWidget {
  const DangerButton({
    required this.label,
    required this.onPressed,
    this.icon,
    this.expand = false,
    super.key,
  });

  final String label;
  final VoidCallback? onPressed;
  final IconData? icon;
  final bool expand;

  @override
  Widget build(BuildContext context) {
    Widget button = SizedBox(
      height: 46,
      child: FilledButton.icon(
        style: AppButtonStyles.danger(),
        onPressed: onPressed,
        icon: Icon(icon ?? Icons.delete_outline_rounded),
        label: Text(label),
      ),
    );

    if (!expand) {
      return button;
    }

    return SizedBox(width: double.infinity, child: button);
  }
}
