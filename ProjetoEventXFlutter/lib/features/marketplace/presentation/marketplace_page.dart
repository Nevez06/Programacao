import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/features/marketplace/presentation/marketplace_screen.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';

class MarketplacePage extends StatelessWidget {
  const MarketplacePage({super.key});

  @override
  Widget build(BuildContext context) {
    return const OrganizerShell(
      currentRoute: AppRoutes.organizerMarketplace,
      title: 'Marketplace',
      subtitle: 'Produtos, servicos e oportunidades para eventos',
      child: MarketplaceScreen(),
    );
  }
}
