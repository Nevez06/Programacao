import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';

class SupplierReviewPage extends StatelessWidget {
  const SupplierReviewPage({super.key});

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerSupplierReview,
      title: 'Avaliacao de Fornecedores',
      subtitle: 'Feedback que melhora qualidade e confianca do ecossistema',
      child: ListView(
        padding: const EdgeInsets.all(EventXSpacing.md),
        children: [
          DashboardHeroCard(
            title: 'Avaliacoes estrategicas',
            subtitle:
                'Compartilhe feedback apos cada entrega para fortalecer o ranking e a qualidade da rede.',
            primaryActionLabel: 'Ver ranking',
            onPrimaryAction: () =>
                Navigator.of(context).pushNamed(AppRoutes.organizerRanking),
            secondaryActionLabel: 'Marketplace',
            onSecondaryAction: () =>
                Navigator.of(context).pushNamed(AppRoutes.organizerMarketplace),
          ),
          const SizedBox(height: EventXSpacing.lg),
          EmptyStateCard(
            title: 'Nenhuma avaliacao pendente',
            message:
                'Assim que um evento for encerrado, os fornecedores pendentes aparecerao aqui com fluxo guiado de avaliacao.',
            icon: Icons.rate_review_outlined,
            action: FilledButton.tonalIcon(
              onPressed: () =>
                  Navigator.of(context).pushNamed(AppRoutes.organizerEvents),
              icon: const Icon(Icons.event_note_outlined),
              label: const Text('Voltar para eventos'),
            ),
          ),
        ],
      ),
    );
  }
}
