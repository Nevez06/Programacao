import 'package:flutter/material.dart';

class SecondaryButton extends StatelessWidget {
  const SecondaryButton({
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
      child: OutlinedButton.icon(
        onPressed: onPressed,
        icon: Icon(icon ?? Icons.arrow_forward_rounded),
        label: Text(label),
      ),
    );

    if (!expand) {
      return button;
    }

    return SizedBox(width: double.infinity, child: button);
  }
}
