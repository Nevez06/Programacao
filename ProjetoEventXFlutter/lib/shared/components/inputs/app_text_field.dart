import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';

class AppTextField extends StatelessWidget {
  const AppTextField({
    required this.controller,
    required this.label,
    this.hint,
    this.keyboardType,
    this.obscureText = false,
    this.autofillHints,
    this.validator,
    this.maxLines = 1,
    this.textInputAction,
    this.onSubmitted,
    this.onChanged,
    this.prefixIcon,
    this.suffixIcon,
    super.key,
  });

  final TextEditingController controller;
  final String label;
  final String? hint;
  final TextInputType? keyboardType;
  final bool obscureText;
  final Iterable<String>? autofillHints;
  final String? Function(String?)? validator;
  final int maxLines;
  final TextInputAction? textInputAction;
  final ValueChanged<String>? onSubmitted;
  final ValueChanged<String>? onChanged;
  final IconData? prefixIcon;
  final IconData? suffixIcon;

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      controller: controller,
      keyboardType: keyboardType,
      obscureText: obscureText,
      autofillHints: autofillHints,
      validator: validator,
      maxLines: maxLines,
      textInputAction: textInputAction,
      onFieldSubmitted: onSubmitted,
      onChanged: onChanged,
      scrollPadding: const EdgeInsets.all(AppSpacing.lg),
      decoration: InputDecoration(
        labelText: label,
        hintText: hint,
        prefixIcon: prefixIcon == null ? null : Icon(prefixIcon),
        suffixIcon: suffixIcon == null ? null : Icon(suffixIcon),
      ),
    );
  }
}
