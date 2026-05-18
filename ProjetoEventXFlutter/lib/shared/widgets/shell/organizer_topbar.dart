import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/shell/app_search_bar.dart';

class OrganizerTopbar extends StatelessWidget {
  const OrganizerTopbar({
    required this.title,
    this.subtitle,
    this.onMenuTap,
    this.trailing,
    this.currentRoute,
    super.key,
  });

  final String title;
  final String? subtitle;
  final VoidCallback? onMenuTap;
  final Widget? trailing;
  final String? currentRoute;

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthController>();
    final name = auth.profile?.nome.isNotEmpty == true
        ? auth.profile!.nome
        : (auth.session?.userName ?? 'Usuario');

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: EventXSpacing.md,
        vertical: EventXSpacing.md,
      ),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        border: Border(
          bottom: BorderSide(color: EventXColors.organizerStroke),
        ),
      ),
      child: Column(
        children: [
          Row(
            children: [
              if (onMenuTap != null) ...[
                IconButton(
                  onPressed: onMenuTap,
                  icon: const Icon(Icons.menu_rounded),
                ),
                const SizedBox(width: EventXSpacing.xs),
              ],
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(title, style: Theme.of(context).textTheme.titleLarge),
                    if (subtitle != null)
                      Text(
                        subtitle!,
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                  ],
                ),
              ),
              if (trailing != null) trailing!,
              IconButton(
                tooltip: 'Notificacoes',
                onPressed: currentRoute == AppRoutes.organizerNotifications
                    ? null
                    : () => Navigator.of(context).pushReplacementNamed(
                          AppRoutes.organizerNotifications,
                        ),
                icon: const Icon(Icons.notifications_none_rounded),
              ),
              const SizedBox(width: EventXSpacing.sm),
              PopupMenuButton<String>(
                tooltip: 'Conta',
                onSelected: (value) async {
                  if (value == 'profile') {
                    if (currentRoute != AppRoutes.organizerProfileAdmin) {
                      await Navigator.of(context).pushReplacementNamed(
                        AppRoutes.organizerProfileAdmin,
                      );
                    }
                    return;
                  }
                  if (value == 'logout') {
                    final authController = context.read<AuthController>();
                    await authController.logout();
                  }
                },
                itemBuilder: (context) => const [
                  PopupMenuItem<String>(
                    value: 'profile',
                    child: Text('Perfil administrativo'),
                  ),
                  PopupMenuItem<String>(
                    value: 'logout',
                    child: Text('Sair'),
                  ),
                ],
                child: Container(
                  padding: const EdgeInsets.all(2),
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(EventXRadius.pill),
                    border: Border.all(color: EventXColors.organizerStroke),
                  ),
                  child: CircleAvatar(
                    radius: 16,
                    backgroundColor: EventXColors.brand.withValues(alpha: 0.12),
                    child: Text(
                      name.trim().isEmpty ? 'U' : name.trim().characters.first,
                      style: const TextStyle(
                        color: EventXColors.brand,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: EventXSpacing.sm),
          const AppSearchBar(),
        ],
      ),
    );
  }
}
