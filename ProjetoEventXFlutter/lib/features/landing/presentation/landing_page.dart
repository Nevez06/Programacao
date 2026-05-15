import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/secondary_button.dart';

class LandingPage extends StatelessWidget {
  const LandingPage({super.key});

  @override
  Widget build(BuildContext context) {
    final isWide = MediaQuery.of(context).size.width >= 980;
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: [Color(0xFFFDF8F8), Color(0xFFF7F8FB)],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
        ),
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 1200),
              child: Padding(
                padding: const EdgeInsets.all(EventXSpacing.lg),
                child: Flex(
                  direction: isWide ? Axis.horizontal : Axis.vertical,
                  children: [
                    Expanded(
                      child: _HeroBlock(isWide: isWide),
                    ),
                    const SizedBox(
                        width: EventXSpacing.xl, height: EventXSpacing.xl),
                    Expanded(
                      child: _EntryCard(),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _HeroBlock extends StatelessWidget {
  const _HeroBlock({required this.isWide});

  final bool isWide;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.xl),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(EventXRadius.xl),
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [EventXColors.brand, EventXColors.accent],
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Text(
            'EventX',
            style: TextStyle(
              color: Colors.white,
              fontSize: 44,
              fontWeight: FontWeight.w900,
            ),
          ),
          const SizedBox(height: EventXSpacing.md),
          Text(
            'Sistema operacional de eventos + universo social em um unico produto.',
            style: TextStyle(
              color: Colors.white.withValues(alpha: 0.92),
              fontSize: isWide ? 24 : 20,
              height: 1.32,
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: EventXSpacing.md),
          Text(
            'Gestao, convites, marketplace, financeiro, ranking e experiencias sociais premium em Flutter.',
            style: TextStyle(
              color: Colors.white.withValues(alpha: 0.88),
              fontSize: 15,
              height: 1.5,
            ),
          ),
        ],
      ),
    );
  }
}

class _EntryCard extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.lg),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.xl),
        border: Border.all(color: EventXColors.organizerStroke),
        boxShadow: EventXShadows.card,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        mainAxisSize: MainAxisSize.min,
        children: [
          Text('Acessar EventX',
              style: Theme.of(context).textTheme.headlineSmall),
          const SizedBox(height: EventXSpacing.xs),
          Text(
            'Escolha o perfil para entrar no ecossistema EventX.',
            style: Theme.of(context).textTheme.bodyMedium,
          ),
          const SizedBox(height: EventXSpacing.lg),
          PrimaryButton(
            label: 'Entrar como Organizador',
            icon: Icons.dashboard_outlined,
            expand: true,
            onPressed: () =>
                Navigator.of(context).pushNamed(AppRoutes.loginOrganizer),
          ),
          const SizedBox(height: EventXSpacing.sm),
          SecondaryButton(
            label: 'Entrar como Convidado',
            icon: Icons.people_outline_rounded,
            onPressed: () =>
                Navigator.of(context).pushNamed(AppRoutes.loginGuest),
          ),
          const SizedBox(height: EventXSpacing.sm),
          SecondaryButton(
            label: 'Entrar como Fornecedor',
            icon: Icons.storefront_outlined,
            onPressed: () =>
                Navigator.of(context).pushNamed(AppRoutes.loginSupplier),
          ),
          const SizedBox(height: EventXSpacing.sm),
          TextButton.icon(
            onPressed: () =>
                Navigator.of(context).pushNamed(AppRoutes.register),
            icon: const Icon(Icons.app_registration_rounded),
            label: const Text('Criar nova conta'),
          ),
          const SizedBox(height: EventXSpacing.lg),
          FilledButton.tonalIcon(
            onPressed: () =>
                Navigator.of(context).pushNamed(AppRoutes.socialFeed),
            icon: const Icon(Icons.dynamic_feed_outlined),
            label: const Text('Explorar Social'),
          ),
        ],
      ),
    );
  }
}
