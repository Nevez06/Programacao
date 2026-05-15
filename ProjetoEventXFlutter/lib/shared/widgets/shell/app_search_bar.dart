import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class AppSearchBar extends StatelessWidget {
  const AppSearchBar({
    this.hintText = 'Buscar eventos, convites, fornecedores...',
    this.onSubmitted,
    this.controller,
    super.key,
  });

  final String hintText;
  final ValueChanged<String>? onSubmitted;
  final TextEditingController? controller;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 44,
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.md),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: TextField(
        controller: controller,
        onSubmitted: onSubmitted,
        decoration: InputDecoration(
          border: InputBorder.none,
          contentPadding: const EdgeInsets.symmetric(
            horizontal: EventXSpacing.md,
            vertical: EventXSpacing.sm,
          ),
          prefixIcon: const Icon(
            Icons.search_rounded,
            color: EventXColors.organizerTextMuted,
          ),
          hintText: hintText,
        ),
      ),
    );
  }
}
