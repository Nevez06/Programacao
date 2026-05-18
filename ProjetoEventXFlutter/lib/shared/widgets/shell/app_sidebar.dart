import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/shared/navigation/organizer_nav_item.dart';

class AppSidebar extends StatelessWidget {
  const AppSidebar({
    required this.items,
    required this.currentRoute,
    required this.onItemTap,
    super.key,
  });

  final List<OrganizerNavItem> items;
  final String currentRoute;
  final ValueChanged<String> onItemTap;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 268,
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        border: Border(
          right: BorderSide(color: EventXColors.organizerStroke),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: double.infinity,
            padding: const EdgeInsets.fromLTRB(
              EventXSpacing.md,
              EventXSpacing.xl,
              EventXSpacing.md,
              EventXSpacing.lg,
            ),
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                colors: [EventXColors.brand, EventXColors.accent],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
            ),
            child: const Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'EventX',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 24,
                    fontWeight: FontWeight.w800,
                  ),
                ),
                SizedBox(height: 4),
                Text(
                  'Organizer OS',
                  style: TextStyle(color: Color(0xFFEEDDE8), fontSize: 13),
                ),
              ],
            ),
          ),
          const SizedBox(height: EventXSpacing.sm),
          Expanded(
            child: ListView.separated(
              itemBuilder: (context, index) {
                final item = items[index];
                final isActive = currentRoute == item.route;

                return Padding(
                  padding:
                      const EdgeInsets.symmetric(horizontal: EventXSpacing.sm),
                  child: _SidebarTile(
                    item: item,
                    isActive: isActive,
                    onTap: () => onItemTap(item.route),
                  ),
                );
              },
              separatorBuilder: (_, __) => const SizedBox(height: 2),
              itemCount: items.length,
            ),
          ),
          const Padding(
            padding: EdgeInsets.all(EventXSpacing.md),
            child: Text(
              'Gestao + Social + IA',
              style: TextStyle(
                color: EventXColors.organizerTextMuted,
                fontSize: 12,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _SidebarTile extends StatefulWidget {
  const _SidebarTile({
    required this.item,
    required this.isActive,
    required this.onTap,
  });

  final OrganizerNavItem item;
  final bool isActive;
  final VoidCallback onTap;

  @override
  State<_SidebarTile> createState() => _SidebarTileState();
}

class _SidebarTileState extends State<_SidebarTile> {
  bool _hover = false;

  @override
  Widget build(BuildContext context) {
    final isHighlighted = widget.isActive || _hover;
    final scale = widget.isActive ? 1.0 : (_hover ? 1.008 : 1.0);
    final translate = _hover && !widget.isActive ? -2.0 : 0.0;

    return MouseRegion(
      onEnter: (_) => setState(() => _hover = true),
      onExit: (_) => setState(() => _hover = false),
      child: AnimatedScale(
        duration: AppDurations.fast,
        curve: AppCurves.easeInOut,
        scale: scale,
        child: AnimatedContainer(
          duration: AppDurations.fast,
          curve: AppCurves.easeInOut,
          transform: Matrix4.translationValues(0, translate, 0),
          decoration: BoxDecoration(
            color: isHighlighted ? const Color(0x14D93A3A) : Colors.transparent,
            borderRadius: BorderRadius.circular(EventXRadius.md),
            border: Border.all(
              color: widget.isActive
                  ? const Color(0x38D93A3A)
                  : Colors.transparent,
            ),
          ),
          child: ListTile(
            dense: true,
            horizontalTitleGap: 8,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(EventXRadius.md),
            ),
            leading: Icon(
              widget.item.icon,
              color: widget.isActive
                  ? EventXColors.brand
                  : EventXColors.organizerTextMuted,
            ),
            title: Text(
              widget.item.label,
              style: TextStyle(
                color: widget.isActive
                    ? EventXColors.organizerText
                    : EventXColors.organizerTextMuted,
                fontWeight: widget.isActive ? FontWeight.w700 : FontWeight.w500,
              ),
            ),
            onTap: widget.onTap,
          ),
        ),
      ),
    );
  }
}
