import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class MarketplaceActionButtons extends StatelessWidget {
  const MarketplaceActionButtons({super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: EventXSpacing.md),
      child: Row(
        children: [
          Expanded(
            child: _MarketplaceActionButton(
              icon: Icons.edit_square,
              label: 'Vender',
              onTap: () {},
            ),
          ),
          const SizedBox(width: EventXSpacing.sm),
          Expanded(
            child: _MarketplaceActionButton(
              icon: Icons.list_rounded,
              label: 'Categorias',
              onTap: () {},
            ),
          ),
        ],
      ),
    );
  }
}

class _MarketplaceActionButton extends StatelessWidget {
  const _MarketplaceActionButton({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: EventXColors.socialSurfaceAlt,
      borderRadius: BorderRadius.circular(EventXRadius.pill),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(EventXRadius.pill),
        child: Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: EventXSpacing.md,
            vertical: EventXSpacing.sm,
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(icon, color: EventXColors.socialText, size: 22),
              const SizedBox(width: EventXSpacing.xs),
              Flexible(
                child: Text(
                  label,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: EventXColors.socialText,
                    fontSize: 18,
                    fontWeight: FontWeight.w800,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
