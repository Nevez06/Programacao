import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';

class AppSearchField extends StatelessWidget {
  const AppSearchField({
    required this.controller,
    this.hint = 'Buscar...',
    this.onChanged,
    this.onSubmitted,
    this.onFilterTap,
    super.key,
  });

  final TextEditingController controller;
  final String hint;
  final ValueChanged<String>? onChanged;
  final ValueChanged<String>? onSubmitted;
  final VoidCallback? onFilterTap;

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: controller,
      onChanged: onChanged,
      onSubmitted: onSubmitted,
      textInputAction: TextInputAction.search,
      decoration: InputDecoration(
        hintText: hint,
        prefixIcon: const Icon(Icons.search_rounded),
        suffixIcon: onFilterTap == null
            ? null
            : IconButton(
                onPressed: onFilterTap,
                icon: const Icon(Icons.tune_rounded),
              ),
        contentPadding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        ),
      ),
    );
  }
}
