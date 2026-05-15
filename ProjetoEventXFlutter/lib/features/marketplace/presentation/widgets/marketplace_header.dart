import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class MarketplaceHeader extends StatelessWidget {
  const MarketplaceHeader({
    required this.controller,
    required this.onChanged,
    required this.onMenuTap,
    required this.onProfileTap,
    super.key,
  });

  final TextEditingController controller;
  final ValueChanged<String> onChanged;
  final VoidCallback onMenuTap;
  final VoidCallback onProfileTap;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(
        EventXSpacing.md,
        EventXSpacing.md,
        EventXSpacing.md,
        EventXSpacing.sm,
      ),
      child: Column(
        children: [
          Row(
            children: [
              _HeaderIconButton(
                icon: Icons.menu_rounded,
                tooltip: 'Menu',
                onTap: onMenuTap,
              ),
              const SizedBox(width: EventXSpacing.sm),
              const Expanded(
                child: Text(
                  'Marketplace',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: EventXColors.socialText,
                    fontSize: 30,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
              const SizedBox(width: EventXSpacing.sm),
              _HeaderIconButton(
                icon: Icons.person_outline_rounded,
                tooltip: 'Conta',
                onTap: onProfileTap,
              ),
              const SizedBox(width: EventXSpacing.xs),
              _HeaderIconButton(
                icon: Icons.search_rounded,
                tooltip: 'Buscar',
                onTap: () {},
              ),
            ],
          ),
          const SizedBox(height: EventXSpacing.md),
          TextField(
            controller: controller,
            onChanged: onChanged,
            style: const TextStyle(color: EventXColors.socialText),
            cursorColor: EventXColors.socialAccent,
            decoration: InputDecoration(
              hintText: 'Pesquisar no Marketplace',
              hintStyle: const TextStyle(color: EventXColors.socialTextMuted),
              prefixIcon: const Icon(
                Icons.search_rounded,
                color: EventXColors.socialTextMuted,
              ),
              filled: true,
              fillColor: EventXColors.socialSurfaceAlt,
              contentPadding: const EdgeInsets.symmetric(
                horizontal: EventXSpacing.md,
                vertical: EventXSpacing.sm,
              ),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(EventXRadius.pill),
                borderSide: BorderSide.none,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _HeaderIconButton extends StatelessWidget {
  const _HeaderIconButton({
    required this.icon,
    required this.tooltip,
    required this.onTap,
  });

  final IconData icon;
  final String tooltip;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Tooltip(
      message: tooltip,
      child: InkResponse(
        onTap: onTap,
        radius: 24,
        child: SizedBox.square(
          dimension: 42,
          child: Icon(
            icon,
            color: EventXColors.socialText,
            size: 30,
          ),
        ),
      ),
    );
  }
}
