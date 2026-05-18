import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/navigation/social_nav_item.dart';
import 'package:projeto_eventx_flutter/shared/widgets/shell/social_topbar.dart';
import 'package:projeto_eventx_flutter/theme/app_theme.dart';

class SocialShell extends StatelessWidget {
  const SocialShell({
    required this.currentRoute,
    required this.title,
    required this.child,
    this.actions = const <Widget>[],
    this.showBackToPanel = true,
    super.key,
  });

  final String currentRoute;
  final String title;
  final Widget child;
  final List<Widget> actions;
  final bool showBackToPanel;

  static final List<SocialNavItem> _items = [
    const SocialNavItem(
      route: AppRoutes.socialFeed,
      label: 'Feed',
      icon: Icons.home_outlined,
    ),
    const SocialNavItem(
      route: AppRoutes.socialStories,
      label: 'Stories',
      icon: Icons.play_circle_outline_rounded,
    ),
    const SocialNavItem(
      route: AppRoutes.socialExplore,
      label: 'Explore',
      icon: Icons.explore_outlined,
    ),
    const SocialNavItem(
      route: AppRoutes.socialCreateStory,
      label: 'Criar',
      icon: Icons.add_circle_outline_rounded,
    ),
    const SocialNavItem(
      route: AppRoutes.socialProfile,
      label: 'Perfil',
      icon: Icons.person_outline_rounded,
    ),
  ];

  @override
  Widget build(BuildContext context) {
    final isDesktop = MediaQuery.of(context).size.width >= 1040;
    final selectedRoute = _normalizeSelectedRoute(currentRoute);
    final selectedIndex =
        _items.indexWhere((item) => item.route == selectedRoute);

    return Theme(
      data: AppTheme.socialDark(),
      child: Scaffold(
        appBar: SocialTopbar(
          title: title,
          currentRoute: selectedRoute,
          actions: actions,
          showBackToPanel: showBackToPanel,
        ),
        body: Stack(
          children: [
            const _SocialBackground(),
            Row(
              children: [
                if (isDesktop)
                  Container(
                    width: 112,
                    decoration: const BoxDecoration(
                      border: Border(
                        right: BorderSide(color: EventXColors.socialStroke),
                      ),
                    ),
                    child: Column(
                      children: [
                        const SizedBox(height: EventXSpacing.md),
                        ..._items.map((item) {
                          final selected = item.route == selectedRoute;
                          return Padding(
                            padding: const EdgeInsets.symmetric(
                              vertical: EventXSpacing.xs,
                              horizontal: EventXSpacing.xs,
                            ),
                            child: InkWell(
                              borderRadius:
                                  BorderRadius.circular(EventXRadius.md),
                              onTap: () => _go(context, item.route),
                              child: AnimatedContainer(
                                duration: AppDurations.fast,
                                curve: AppCurves.easeInOut,
                                width: double.infinity,
                                padding: const EdgeInsets.symmetric(
                                  vertical: EventXSpacing.sm,
                                  horizontal: EventXSpacing.xs,
                                ),
                                decoration: BoxDecoration(
                                  color: selected
                                      ? const Color(0x22FF4F70)
                                      : Colors.transparent,
                                  borderRadius:
                                      BorderRadius.circular(EventXRadius.md),
                                  border: Border.all(
                                    color: selected
                                        ? const Color(0x44FF4F70)
                                        : Colors.transparent,
                                  ),
                                ),
                                child: Column(
                                  children: [
                                    Icon(
                                      item.icon,
                                      color: selected
                                          ? EventXColors.socialAccent
                                          : EventXColors.socialTextMuted,
                                    ),
                                    const SizedBox(height: 4),
                                    Text(
                                      item.label,
                                      style: TextStyle(
                                        color: selected
                                            ? EventXColors.socialText
                                            : EventXColors.socialTextMuted,
                                        fontSize: 11,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ),
                          );
                        }),
                      ],
                    ),
                  ),
                Expanded(
                  child: AnimatedSwitcher(
                    duration: AppDurations.normal,
                    switchInCurve: AppCurves.ease,
                    switchOutCurve: AppCurves.easeIn,
                    transitionBuilder: (widget, animation) =>
                        AppMotion.fadeSlide(
                      animation: animation,
                      begin: const Offset(0.018, 0.02),
                      child: widget,
                    ),
                    child: KeyedSubtree(
                      key: ValueKey(currentRoute),
                      child: child,
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
        bottomNavigationBar: isDesktop
            ? null
            : BottomNavigationBar(
                currentIndex: selectedIndex < 0 ? 0 : selectedIndex,
                onTap: (index) => _go(context, _items[index].route),
                backgroundColor: EventXColors.socialSurfaceAlt,
                selectedItemColor: EventXColors.socialAccent,
                unselectedItemColor: EventXColors.socialTextMuted,
                type: BottomNavigationBarType.fixed,
                selectedLabelStyle:
                    const TextStyle(fontWeight: FontWeight.w700),
                items: _items
                    .map(
                      (item) => BottomNavigationBarItem(
                        icon: Icon(item.icon),
                        label: item.label,
                      ),
                    )
                    .toList(),
              ),
      ),
    );
  }

  Future<void> _go(BuildContext context, String route) async {
    if (route == currentRoute) {
      return;
    }
    await Navigator.of(context).pushReplacementNamed(route);
  }

  String _normalizeSelectedRoute(String route) {
    if (AppRoutes.isDynamicSocialProfileRoute(route)) {
      return AppRoutes.socialProfile;
    }
    return route;
  }
}

class _SocialBackground extends StatelessWidget {
  const _SocialBackground();

  @override
  Widget build(BuildContext context) {
    return IgnorePointer(
      ignoring: true,
      child: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: [Color(0xFF0D0F16), Color(0xFF121726)],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: Stack(
          children: [
            Positioned(
              top: -110,
              right: -80,
              child: Container(
                width: 320,
                height: 320,
                decoration: BoxDecoration(
                  gradient: RadialGradient(
                    colors: [
                      EventXColors.socialAccent.withValues(alpha: 0.18),
                      Colors.transparent,
                    ],
                  ),
                ),
              ),
            ),
            Positioned(
              bottom: -120,
              left: -100,
              child: Container(
                width: 360,
                height: 360,
                decoration: BoxDecoration(
                  gradient: RadialGradient(
                    colors: [
                      EventXColors.socialAccentAlt.withValues(alpha: 0.14),
                      Colors.transparent,
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
