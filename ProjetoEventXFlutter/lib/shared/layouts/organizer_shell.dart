import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/navigation/organizer_nav_item.dart';
import 'package:projeto_eventx_flutter/shared/widgets/shell/app_sidebar.dart';
import 'package:projeto_eventx_flutter/shared/widgets/shell/organizer_topbar.dart';

class OrganizerShell extends StatelessWidget {
  const OrganizerShell({
    required this.currentRoute,
    required this.title,
    required this.child,
    this.subtitle,
    this.topbarTrailing,
    this.floatingActionButton,
    super.key,
  });

  final String currentRoute;
  final String title;
  final String? subtitle;
  final Widget child;
  final Widget? topbarTrailing;
  final Widget? floatingActionButton;

  static final List<OrganizerNavItem> _items = [
    const OrganizerNavItem(
      route: AppRoutes.organizerDashboard,
      label: 'Dashboard',
      icon: Icons.dashboard_outlined,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerEvents,
      label: 'Eventos',
      icon: Icons.event_outlined,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerCreateEvent,
      label: 'Criar Evento',
      icon: Icons.add_circle_outline_rounded,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerInvitations,
      label: 'Central de Convites',
      icon: Icons.mail_outline_rounded,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerTemplateGallery,
      label: 'Galeria de Templates',
      icon: Icons.auto_awesome_mosaic_outlined,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerEditor,
      label: 'EventX Editor',
      icon: Icons.draw_outlined,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerBudget,
      label: 'Financeiro',
      icon: Icons.pie_chart_outline_rounded,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerOrders,
      label: 'Pedidos',
      icon: Icons.receipt_long_outlined,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerMarketplace,
      label: 'Marketplace',
      icon: Icons.storefront_outlined,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerRanking,
      label: 'Ranking',
      icon: Icons.emoji_events_outlined,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerSupplierReview,
      label: 'Avaliacoes',
      icon: Icons.star_border_rounded,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerAiAssistant,
      label: 'Assistente IA',
      icon: Icons.auto_awesome_outlined,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerNotifications,
      label: 'Notificacoes',
      icon: Icons.notifications_none_rounded,
    ),
    const OrganizerNavItem(
      route: AppRoutes.organizerProfileAdmin,
      label: 'Perfil Admin',
      icon: Icons.person_outline_rounded,
    ),
    const OrganizerNavItem(
      route: AppRoutes.socialFeed,
      label: 'EventX Social',
      icon: Icons.dynamic_feed_outlined,
    ),
  ];

  @override
  Widget build(BuildContext context) {
    final isDesktop = MediaQuery.of(context).size.width >= 1024;

    return Scaffold(
      backgroundColor: EventXColors.organizerBackground,
      drawer: isDesktop
          ? null
          : Drawer(
              child: AppSidebar(
                items: _items,
                currentRoute: currentRoute,
                onItemTap: (route) => _navigate(context, route),
              ),
            ),
      floatingActionButton: floatingActionButton,
      body: SafeArea(
        child: Row(
          children: [
            if (isDesktop)
              AnimatedSwitcher(
                duration: AppDurations.normal,
                switchInCurve: AppCurves.ease,
                switchOutCurve: AppCurves.easeIn,
                transitionBuilder: (child, animation) => AppMotion.fadeSlide(
                  animation: animation,
                  begin: const Offset(-0.05, 0),
                  child: child,
                ),
                child: AppSidebar(
                  key: const ValueKey('organizer-sidebar'),
                  items: _items,
                  currentRoute: currentRoute,
                  onItemTap: (route) => _navigate(context, route),
                ),
              ),
            Expanded(
              child: Column(
                children: [
                  Builder(
                    builder: (innerContext) => OrganizerTopbar(
                      title: title,
                      subtitle: subtitle,
                      currentRoute: currentRoute,
                      trailing: topbarTrailing,
                      onMenuTap: isDesktop
                          ? null
                          : () => Scaffold.of(innerContext).openDrawer(),
                    ),
                  ),
                  Expanded(
                    child: AnimatedSwitcher(
                      duration: AppDurations.normal,
                      switchInCurve: AppCurves.ease,
                      switchOutCurve: AppCurves.easeIn,
                      transitionBuilder: (child, animation) =>
                          AppMotion.fadeSlide(
                        animation: animation,
                        begin: const Offset(0.018, 0),
                        child: child,
                      ),
                      child: KeyedSubtree(
                        key: ValueKey(currentRoute),
                        child: child,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _navigate(BuildContext context, String route) async {
    if (route == currentRoute) {
      Navigator.of(context).maybePop();
      return;
    }

    await Navigator.of(context).pushReplacementNamed(route);
  }

  static Future<void> logoutAndGoLogin(BuildContext context) async {
    final auth = context.read<AuthController>();
    await auth.logout();
  }
}
