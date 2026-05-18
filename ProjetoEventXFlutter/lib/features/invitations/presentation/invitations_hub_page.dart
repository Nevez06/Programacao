import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/invitation_templates_repository.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/models/invitation_draft_model.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/models/invitation_template_model.dart';
import 'package:projeto_eventx_flutter/features/invites/presentation/invites_page.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/action_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/template_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class InvitationsHubPage extends StatefulWidget {
  const InvitationsHubPage({super.key});

  @override
  State<InvitationsHubPage> createState() => _InvitationsHubPageState();
}

class _InvitationsHubPageState extends State<InvitationsHubPage> {
  late Future<_InvitationsHubData> _futureData;

  @override
  void initState() {
    super.initState();
    _futureData = _loadData();
  }

  Future<_InvitationsHubData> _loadData() async {
    final repository = context.read<InvitationTemplatesRepository>();
    final result = await Future.wait<Object>([
      repository.getTemplates(),
      repository.getDrafts(),
    ]);

    return _InvitationsHubData(
      templates: result[0] as List<InvitationTemplateModel>,
      drafts: result[1] as List<InvitationDraftModel>,
    );
  }

  Future<void> _refresh() async {
    setState(() {
      _futureData = _loadData();
    });
    await _futureData;
  }

  String _resolveErrorMessage(Object? error) {
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar dados da Central de Convites.';
    }
    return raw
        .replaceFirst('Exception: ', '')
        .replaceFirst('DioException [connection error]: ', '')
        .replaceFirst('DioException [unknown]: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerInvitations,
      title: 'Central de Convites',
      subtitle: 'Galeria, criação visual e envio em fluxo guiado',
      child: FutureBuilder<_InvitationsHubData>(
        future: _futureData,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(
                message: 'Carregando central de convites...');
          }

          if (snapshot.hasError || snapshot.data == null) {
            return ErrorView(
              message: _resolveErrorMessage(snapshot.error),
              onRetry: _refresh,
            );
          }

          final templates =
              snapshot.data?.templates ?? const <InvitationTemplateModel>[];
          final drafts =
              snapshot.data?.drafts ?? const <InvitationDraftModel>[];

          return ListView(
            padding: const EdgeInsets.all(EventXSpacing.md),
            children: [
              DashboardHeroCard(
                title: 'Convites com experiência premium',
                subtitle:
                    'Templates e rascunhos reais da API para acelerar criação e envio com RSVP.',
                primaryActionLabel: 'Escolher template',
                onPrimaryAction: () => Navigator.of(context)
                    .pushNamed(AppRoutes.organizerTemplateGallery),
                secondaryActionLabel: 'Criar do zero',
                onSecondaryAction: () =>
                    Navigator.of(context).pushNamed(AppRoutes.organizerEditor),
              ),
              const SizedBox(height: EventXSpacing.md),
              _HubMetrics(
                templatesCount: templates.length,
                draftsCount: drafts.length,
                loading: snapshot.connectionState == ConnectionState.waiting,
              ),
              const SizedBox(height: EventXSpacing.md),
              const SectionHeader(
                title: 'Fluxo criativo',
                subtitle: 'Da ideia ao envio em passos claros',
              ),
              const SizedBox(height: EventXSpacing.sm),
              _CreativeFlow(
                onRefresh: _refresh,
              ),
              const SizedBox(height: EventXSpacing.lg),
              SectionHeader(
                title: 'Templates em destaque',
                subtitle: 'Templates reais para uso imediato',
                trailing: FilledButton.tonalIcon(
                  onPressed: () => Navigator.of(context)
                      .pushNamed(AppRoutes.organizerTemplateGallery),
                  icon: const Icon(Icons.grid_view_rounded),
                  label: const Text('Ver galeria completa'),
                ),
              ),
              const SizedBox(height: EventXSpacing.sm),
              if (templates.isEmpty)
                EmptyStateCard(
                  title: 'Nenhum template encontrado',
                  message:
                      'Nenhum template disponível no momento. Tente atualizar a central.',
                  icon: Icons.auto_awesome_mosaic_outlined,
                  action: FilledButton.tonalIcon(
                    onPressed: _refresh,
                    icon: const Icon(Icons.refresh_rounded),
                    label: const Text('Atualizar'),
                  ),
                )
              else
                SizedBox(
                  height: 258,
                  child: ListView(
                    scrollDirection: Axis.horizontal,
                    children: templates.take(6).map((template) {
                      return _SizedTemplate(
                        template: template,
                        onTap: () => Navigator.of(context)
                            .pushNamed(AppRoutes.organizerTemplateGallery),
                      );
                    }).toList(growable: false),
                  ),
                ),
              const SizedBox(height: EventXSpacing.lg),
              const SectionHeader(
                title: 'Rascunhos recentes',
                subtitle: 'Continue de onde parou com persistencia real',
              ),
              const SizedBox(height: EventXSpacing.sm),
              if (drafts.isEmpty)
                EmptyStateCard(
                  title: 'Nenhum rascunho salvo',
                  message:
                      'Comece por um template para abrir o editor e salvar seu primeiro rascunho.',
                  icon: Icons.description_outlined,
                  action: FilledButton.icon(
                    onPressed: () => Navigator.of(context)
                        .pushNamed(AppRoutes.organizerTemplateGallery),
                    icon: const Icon(Icons.auto_awesome_mosaic_outlined),
                    label: const Text('Escolher template'),
                  ),
                )
              else
                ...drafts.take(4).map(
                      (draft) => _DraftCard(
                        draft: draft,
                        onContinue: () => Navigator.of(context).pushNamed(
                          AppRoutes.organizerEditor,
                          arguments: EventXEditorRouteArgs(draft: draft),
                        ),
                        onDelete: () async {
                          final confirmed = await showDialog<bool>(
                            context: context,
                            builder: (dialogContext) => AlertDialog(
                              title: const Text('Excluir rascunho'),
                              content: Text(
                                'Deseja excluir o rascunho "${draft.name}"?',
                              ),
                              actions: [
                                TextButton(
                                  onPressed: () =>
                                      Navigator.of(dialogContext).pop(false),
                                  child: const Text('Cancelar'),
                                ),
                                FilledButton(
                                  onPressed: () =>
                                      Navigator.of(dialogContext).pop(true),
                                  child: const Text('Excluir'),
                                ),
                              ],
                            ),
                          );

                          if (confirmed != true) {
                            return;
                          }
                          if (!context.mounted) {
                            return;
                          }

                          try {
                            await context
                                .read<InvitationTemplatesRepository>()
                                .deleteDraft(draft.id);
                            if (!context.mounted) {
                              return;
                            }
                            ScaffoldMessenger.of(context).showSnackBar(
                              const SnackBar(
                                content: Text('Rascunho excluido com sucesso.'),
                              ),
                            );
                            await _refresh();
                          } catch (error) {
                            if (!context.mounted) {
                              return;
                            }
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                content: Text(_resolveErrorMessage(error)),
                              ),
                            );
                          }
                        },
                      ),
                    ),
              const SizedBox(height: EventXSpacing.lg),
              const SectionHeader(
                title: 'Convites e RSVP',
                subtitle: 'Acompanhamento de envio, abertura e confirmação',
              ),
              const SizedBox(height: EventXSpacing.xs),
              const SizedBox(
                height: 560,
                child: InvitesPage(embedded: true),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _HubMetrics extends StatelessWidget {
  const _HubMetrics({
    required this.templatesCount,
    required this.draftsCount,
    required this.loading,
  });

  final int templatesCount;
  final int draftsCount;
  final bool loading;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Row(
        children: [
          _MetricTile(
            label: 'Templates',
            value: loading ? '...' : '$templatesCount',
          ),
          const SizedBox(width: EventXSpacing.md),
          _MetricTile(
            label: 'Rascunhos',
            value: loading ? '...' : '$draftsCount',
          ),
        ],
      ),
    );
  }
}

class _MetricTile extends StatelessWidget {
  const _MetricTile({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(EventXSpacing.sm),
        decoration: BoxDecoration(
          color: EventXColors.organizerSurfaceAlt,
          borderRadius: BorderRadius.circular(EventXRadius.md),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              value,
              style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 22),
            ),
            Text(
              label,
              style: const TextStyle(color: EventXColors.organizerTextMuted),
            ),
          ],
        ),
      ),
    );
  }
}

class _CreativeFlow extends StatelessWidget {
  const _CreativeFlow({required this.onRefresh});

  final Future<void> Function() onRefresh;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final columns = constraints.maxWidth >= 1180
            ? 4
            : constraints.maxWidth >= 760
                ? 2
                : 1;

        return GridView.count(
          shrinkWrap: true,
          crossAxisCount: columns,
          crossAxisSpacing: EventXSpacing.sm,
          mainAxisSpacing: EventXSpacing.sm,
          childAspectRatio: 1.1,
          physics: const NeverScrollableScrollPhysics(),
          children: [
            ActionCard(
              title: '1. Escolher template',
              description:
                  'Use categorias e estilos visuais para acelerar sua criação.',
              icon: Icons.auto_awesome_mosaic_outlined,
              onTap: () => Navigator.of(context)
                  .pushNamed(AppRoutes.organizerTemplateGallery),
              buttonLabel: 'Escolher',
            ),
            ActionCard(
              title: '2. Editar convite',
              description:
                  'Personalize textos, paleta, blocos e call-to-action de RSVP.',
              icon: Icons.draw_outlined,
              onTap: () =>
                  Navigator.of(context).pushNamed(AppRoutes.organizerEditor),
              buttonLabel: 'Editar',
              highlightColor: const Color(0xFF6C63FF),
            ),
            ActionCard(
              title: '3. Salvar rascunho',
              description: 'Continue depois sem perder progresso criativo.',
              icon: Icons.save_outlined,
              onTap: onRefresh,
              buttonLabel: 'Atualizar',
              highlightColor: const Color(0xFF2D7DF6),
            ),
            ActionCard(
              title: '4. Enviar convite',
              description:
                  'Dispare para sua lista e monitore abertura e confirmações.',
              icon: Icons.send_outlined,
              onTap: () => Navigator.of(context)
                  .pushNamed(AppRoutes.organizerInvitations),
              buttonLabel: 'Enviar',
              highlightColor: const Color(0xFF008C6E),
            ),
          ],
        );
      },
    );
  }
}

class _SizedTemplate extends StatelessWidget {
  const _SizedTemplate({
    required this.template,
    required this.onTap,
  });

  final InvitationTemplateModel template;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 240,
      child: Padding(
        padding: const EdgeInsets.only(right: EventXSpacing.sm),
        child: TemplateCard(
          name: template.name,
          category: template.style ?? 'Template',
          previewLabel: template.defaultSystem ? 'Sistema' : 'Personalizado',
          previewGradient: _templateGradient(template),
          onTap: onTap,
        ),
      ),
    );
  }

  List<Color> _templateGradient(InvitationTemplateModel template) {
    final background =
        _parseColor(template.backgroundColor) ?? const Color(0xFFFEE8E2);
    final primary =
        _parseColor(template.primaryColor) ?? const Color(0xFFF7D8D0);
    return [background, primary];
  }

  Color? _parseColor(String? colorHex) {
    final raw = (colorHex ?? '').trim().replaceAll('#', '');
    if (raw.length != 6 && raw.length != 8) {
      return null;
    }
    final withAlpha = raw.length == 6 ? 'ff$raw' : raw;
    final parsed = int.tryParse(withAlpha, radix: 16);
    if (parsed == null) {
      return null;
    }
    return Color(parsed);
  }
}

class _InvitationsHubData {
  const _InvitationsHubData({
    required this.templates,
    required this.drafts,
  });

  final List<InvitationTemplateModel> templates;
  final List<InvitationDraftModel> drafts;
}

class _DraftCard extends StatelessWidget {
  const _DraftCard({
    required this.draft,
    required this.onContinue,
    required this.onDelete,
  });

  final InvitationDraftModel draft;
  final VoidCallback onContinue;
  final VoidCallback onDelete;

  @override
  Widget build(BuildContext context) {
    final updatedAt = draft.updatedAt;
    final updatedLabel =
        '${updatedAt.day.toString().padLeft(2, '0')}/${updatedAt.month.toString().padLeft(2, '0')}/${updatedAt.year} ${updatedAt.hour.toString().padLeft(2, '0')}:${updatedAt.minute.toString().padLeft(2, '0')}';

    return Container(
      margin: const EdgeInsets.only(bottom: EventXSpacing.sm),
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Row(
        children: [
          const Icon(Icons.description_outlined, color: EventXColors.brand),
          const SizedBox(width: EventXSpacing.sm),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  draft.name,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.w700,
                      ),
                ),
                const SizedBox(height: 2),
                Text(
                  'Evento #${draft.eventId} • Atualizado em $updatedLabel',
                  style: const TextStyle(
                    color: EventXColors.organizerTextMuted,
                  ),
                ),
              ],
            ),
          ),
          FilledButton.tonalIcon(
            onPressed: onContinue,
            icon: const Icon(Icons.edit_outlined),
            label: const Text('Continuar'),
          ),
          const SizedBox(width: EventXSpacing.xs),
          IconButton(
            tooltip: 'Excluir rascunho',
            onPressed: onDelete,
            icon: const Icon(Icons.delete_outline_rounded),
          ),
        ],
      ),
    );
  }
}
