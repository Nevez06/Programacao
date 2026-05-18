import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';

class SocialTopbar extends StatelessWidget implements PreferredSizeWidget {
  const SocialTopbar({
    required this.title,
    required this.currentRoute,
    this.actions = const <Widget>[],
    this.showBackToPanel = true,
    this.onBackToPanel,
    super.key,
  });

  final String title;
  final String currentRoute;
  final List<Widget> actions;
  final bool showBackToPanel;
  final VoidCallback? onBackToPanel;

  @override
  Size get preferredSize => const Size.fromHeight(68);

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: EventXColors.socialBackground,
        border: const Border(
          bottom: BorderSide(color: EventXColors.socialStroke),
        ),
        boxShadow: EventXShadows.soft,
      ),
      child: AppBar(
        titleSpacing: EventXSpacing.md,
        title: Row(
          children: [
            Container(
              width: 32,
              height: 32,
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  colors: [
                    EventXColors.socialAccent,
                    EventXColors.socialAccentAlt
                  ],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                shape: BoxShape.circle,
              ),
              alignment: Alignment.center,
              child: const Icon(
                Icons.auto_awesome,
                size: 18,
                color: Colors.white,
              ),
            ),
            const SizedBox(width: EventXSpacing.sm),
            Expanded(
              child: Text(
                title,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  color: EventXColors.socialText,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 0.2,
                ),
              ),
            ),
          ],
        ),
        actions: [
          ...actions,
          IconButton(
            tooltip: 'Explorar',
            onPressed: currentRoute == AppRoutes.socialExplore
                ? null
                : () => Navigator.of(context)
                    .pushReplacementNamed(AppRoutes.socialExplore),
            icon: const Icon(Icons.explore_outlined),
          ),
          if (showBackToPanel)
            IconButton(
              tooltip: 'Voltar ao painel',
              onPressed: onBackToPanel ??
                  () => Navigator.of(context).pushReplacementNamed(
                        AppRoutes.organizerDashboard,
                      ),
              icon: const Icon(Icons.grid_view_rounded),
            ),
          const SizedBox(width: EventXSpacing.xxs),
        ],
      ),
    );
  }
}
