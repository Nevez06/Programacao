import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/events/presentation/events_page.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/action_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';

class OrganizerEventsPage extends StatelessWidget {
  const OrganizerEventsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerEvents,
      title: 'Eventos',
      subtitle: 'Planejamento, status e operacao por evento',
      child: Container(
        color: EventXColors.organizerBackground,
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(EventXSpacing.md),
              child: DashboardHeroCard(
                title: 'Gestao de Eventos',
                subtitle:
                    'Acompanhe status, local, publico e custo de todos os eventos em andamento.',
                primaryActionLabel: 'Criar evento',
                onPrimaryAction: () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerCreateEvent),
                secondaryActionLabel: 'Central de convites',
                onSecondaryAction: () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerInvitations),
              ),
            ),
            Expanded(
              child: EventsPage(
                embedded: true,
                footer: ActionCard(
                  title: 'Planejamento rapido',
                  description:
                      'Use o EventX Editor para transformar templates em convites prontos.',
                  icon: Icons.auto_awesome_mosaic_outlined,
                  buttonLabel: 'Abrir Editor',
                  onTap: () =>
                      Navigator.of(context).pushNamed(AppRoutes.organizerEditor),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
