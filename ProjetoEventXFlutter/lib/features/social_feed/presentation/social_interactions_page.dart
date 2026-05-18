import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/social_shell.dart';

class SocialInteractionsPage extends StatelessWidget {
  const SocialInteractionsPage({super.key});

  @override
  Widget build(BuildContext context) {
    final items = const [
      ('ana.eventos curtiu seu post', 'Ha 5 min', Icons.favorite_rounded),
      (
        'decorbysofi comentou: "Lindo convite!"',
        'Ha 22 min',
        Icons.mode_comment_rounded
      ),
      (
        'musicclub comecou a seguir voce',
        'Ha 1 h',
        Icons.person_add_alt_rounded
      ),
      (
        'Seu story recebeu 32 visualizacoes',
        'Ha 2 h',
        Icons.visibility_outlined
      ),
    ];

    return SocialShell(
      currentRoute: AppRoutes.socialFeed,
      title: 'Interacoes',
      child: ListView(
        padding: const EdgeInsets.all(EventXSpacing.md),
        children: [
          const Text(
            'Atividade social',
            style: TextStyle(
              color: EventXColors.socialText,
              fontWeight: FontWeight.w800,
              fontSize: 22,
            ),
          ),
          const SizedBox(height: EventXSpacing.xs),
          const Text(
            'Acompanhe quem interagiu com seu conteudo no EventX Social.',
            style: TextStyle(color: EventXColors.socialTextMuted),
          ),
          const SizedBox(height: EventXSpacing.md),
          ...items.map(
            (item) => Container(
              margin: const EdgeInsets.only(bottom: EventXSpacing.sm),
              decoration: BoxDecoration(
                color: EventXColors.socialSurface,
                borderRadius: BorderRadius.circular(EventXRadius.lg),
                border: Border.all(color: EventXColors.socialStroke),
              ),
              child: ListTile(
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: EventXSpacing.sm,
                  vertical: 4,
                ),
                leading: CircleAvatar(
                  backgroundColor: EventXColors.socialSurfaceAlt,
                  child: Icon(item.$3, color: EventXColors.socialAccent),
                ),
                title: Text(
                  item.$1,
                  style: const TextStyle(
                    color: EventXColors.socialText,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                subtitle: Text(
                  item.$2,
                  style: const TextStyle(color: EventXColors.socialTextMuted),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
