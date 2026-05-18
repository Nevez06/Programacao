import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/invitation_templates_repository.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/models/invitation_draft_model.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/models/invitation_template_model.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/filter_panel.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/template_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/animated_section_header.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/fade_slide_in.dart';

class TemplateGalleryPage extends StatefulWidget {
  const TemplateGalleryPage({super.key});

  @override
  State<TemplateGalleryPage> createState() => _TemplateGalleryPageState();
}

class _TemplateGalleryPageState extends State<TemplateGalleryPage> {
  late Future<List<InvitationTemplateModel>> _futureTemplates;
  String? _selectedStyle;

  @override
  void initState() {
    super.initState();
    _futureTemplates = _loadTemplates();
  }

  Future<List<InvitationTemplateModel>> _loadTemplates() {
    return context.read<InvitationTemplatesRepository>().getTemplates();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureTemplates = _loadTemplates();
    });
    await _futureTemplates;
  }

  Future<void> _createDraftFromTemplate(
    InvitationTemplateModel template,
  ) async {
    List<EventListItem> events;
    try {
      events = await context.read<EventsRepository>().getEvents();
    } catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(_resolveErrorMessage(error))),
      );
      return;
    }

    if (!mounted) {
      return;
    }

    if (events.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
            content: Text('Crie um evento antes de escolher template.')),
      );
      return;
    }

    int selectedEventId = events.first.id;
    final draftNameController = TextEditingController(text: template.name);
    bool creating = false;

    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return StatefulBuilder(
          builder: (context, setStateDialog) {
            return AlertDialog(
              title: const Text('Criar rascunho do convite'),
              content: SizedBox(
                width: 480,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    DropdownButtonFormField<int>(
                      initialValue: selectedEventId,
                      decoration: const InputDecoration(labelText: 'Evento'),
                      items: events
                          .map(
                            (event) => DropdownMenuItem<int>(
                              value: event.id,
                              child: Text(event.nomeEvento),
                            ),
                          )
                          .toList(growable: false),
                      onChanged: (value) {
                        if (value == null) {
                          return;
                        }
                        setStateDialog(() => selectedEventId = value);
                      },
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: draftNameController,
                      decoration:
                          const InputDecoration(labelText: 'Nome do rascunho'),
                    ),
                  ],
                ),
              ),
              actions: [
                TextButton(
                  onPressed:
                      creating ? null : () => Navigator.of(dialogContext).pop(),
                  child: const Text('Cancelar'),
                ),
                FilledButton(
                  onPressed: creating
                      ? null
                      : () async {
                          setStateDialog(() => creating = true);
                          try {
                            final draft = await context
                                .read<InvitationTemplatesRepository>()
                                .createFromTemplate(
                                  CreateInvitationFromTemplateRequest(
                                    eventId: selectedEventId,
                                    templateId: template.id,
                                    name: draftNameController.text.trim(),
                                  ),
                                );

                            if (!mounted || !dialogContext.mounted) {
                              return;
                            }
                            Navigator.of(dialogContext).pop();
                            ScaffoldMessenger.of(this.context).showSnackBar(
                              const SnackBar(
                                content: Text('Rascunho criado com sucesso.'),
                              ),
                            );
                            Navigator.of(this.context).pushNamed(
                              AppRoutes.organizerEditor,
                              arguments: EventXEditorRouteArgs(draft: draft),
                            );
                          } catch (error) {
                            if (!mounted) {
                              return;
                            }
                            setStateDialog(() => creating = false);
                            ScaffoldMessenger.of(this.context).showSnackBar(
                              SnackBar(
                                content: Text(_resolveErrorMessage(error)),
                              ),
                            );
                          }
                        },
                  child: Text(creating ? 'Criando...' : 'Criar rascunho'),
                ),
              ],
            );
          },
        );
      },
    );

    draftNameController.dispose();
  }

  String _resolveErrorMessage(Object? error) {
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar templates.';
    }
    return raw
        .replaceFirst('Exception: ', '')
        .replaceFirst('DioException [connection error]: ', '')
        .replaceFirst('DioException [unknown]: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerTemplateGallery,
      title: 'Galeria de Templates',
      subtitle: 'Curadoria visual estilo Canva/Pinterest para convites',
      child: FutureBuilder<List<InvitationTemplateModel>>(
        future: _futureTemplates,
        builder: (context, eventsSnapshot) {
          if (eventsSnapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(message: 'Carregando templates...');
          }

          if (eventsSnapshot.hasError) {
            return ErrorView(
              message: _resolveErrorMessage(eventsSnapshot.error),
              onRetry: _refresh,
            );
          }

          final templates =
              eventsSnapshot.data ?? const <InvitationTemplateModel>[];
          final styles = templates
              .map((template) => (template.style ?? '').trim())
              .where((style) => style.isNotEmpty)
              .toSet()
              .toList(growable: false)
            ..sort();
          final visibleTemplates = _selectedStyle == null
              ? templates
              : templates
                  .where((template) =>
                      (template.style ?? '').toLowerCase() ==
                      _selectedStyle!.toLowerCase())
                  .toList(growable: false);

          return ListView(
            padding: const EdgeInsets.all(EventXSpacing.md),
            children: [
              FadeSlideIn(
                child: DashboardHeroCard(
                  title: 'Escolha o template ideal',
                  subtitle:
                      'Templates reais vindos da API de convites, prontos para gerar rascunho.',
                  primaryActionLabel: 'Abrir EventX Editor',
                  onPrimaryAction: () => Navigator.of(context)
                      .pushNamed(AppRoutes.organizerEditor),
                  secondaryActionLabel: 'Central de convites',
                  onSecondaryAction: () => Navigator.of(context)
                      .pushNamed(AppRoutes.organizerInvitations),
                ),
              ),
              const SizedBox(height: EventXSpacing.lg),
              const AnimatedSectionHeader(
                title: 'Biblioteca visual',
                subtitle: 'Navegue por estilo, ocasiao e linguagem de design',
                delay: AppDurations.staggerStep,
              ),
              const SizedBox(height: EventXSpacing.sm),
              FadeSlideIn(
                delay: AppDurations.staggerStep * 2,
                child: LayoutBuilder(
                  builder: (context, constraints) {
                    final isWide = constraints.maxWidth >= 1040;
                    final columns = constraints.maxWidth >= 1200
                        ? 4
                        : constraints.maxWidth >= 820
                            ? 3
                            : 2;

                    final panel = FilterPanel(
                      title: 'Filtros criativos',
                      subtitle: 'Refine por estilo de template',
                      chips: ['Todos', ...styles],
                      actions: [
                        FilledButton.tonal(
                          onPressed: _refresh,
                          child: const Text('Atualizar'),
                        ),
                        OutlinedButton(
                          onPressed: () =>
                              setState(() => _selectedStyle = null),
                          child: const Text('Limpar'),
                        ),
                      ],
                    );

                    final Widget grid;
                    if (templates.isEmpty) {
                      grid = EmptyStateCard(
                        title: 'Nenhum template encontrado',
                        message:
                            'Nao ha templates disponiveis no momento. Tente atualizar.',
                        icon: Icons.auto_awesome_mosaic_outlined,
                        action: FilledButton.tonalIcon(
                          onPressed: _refresh,
                          icon: const Icon(Icons.refresh_rounded),
                          label: const Text('Atualizar'),
                        ),
                      );
                    } else if (visibleTemplates.isEmpty) {
                      grid = const EmptyStateCard(
                        title: 'Sem resultados para este filtro',
                        message:
                            'Nenhum template corresponde ao filtro selecionado.',
                        icon: Icons.filter_alt_off_outlined,
                      );
                    } else {
                      grid = GridView.builder(
                        shrinkWrap: true,
                        itemCount: visibleTemplates.length,
                        physics: const NeverScrollableScrollPhysics(),
                        gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                          crossAxisCount: columns,
                          crossAxisSpacing: EventXSpacing.sm,
                          mainAxisSpacing: EventXSpacing.sm,
                          childAspectRatio: 0.74,
                        ),
                        itemBuilder: (context, index) {
                          final item = visibleTemplates[index];
                          return FadeSlideIn(
                            delay: AppDurations.staggerStep * index,
                            child: TemplateCard(
                              name: item.name,
                              category: item.style ?? 'Template',
                              previewLabel: item.defaultSystem
                                  ? 'Sistema'
                                  : 'Personalizado',
                              previewGradient: _templateGradient(item),
                              onTap: () => _createDraftFromTemplate(item),
                            ),
                          );
                        },
                      );
                    }

                    if (isWide) {
                      return Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          SizedBox(width: 280, child: panel),
                          const SizedBox(width: EventXSpacing.md),
                          Expanded(child: grid),
                        ],
                      );
                    }

                    return Column(
                      children: [
                        panel,
                        const SizedBox(height: EventXSpacing.sm),
                        grid,
                      ],
                    );
                  },
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  List<Color> _templateGradient(InvitationTemplateModel template) {
    final background =
        _tryParseColor(template.backgroundColor) ?? const Color(0xFFFEE8E2);
    final primary =
        _tryParseColor(template.primaryColor) ?? const Color(0xFFF7D8D0);
    return [background, primary];
  }

  Color? _tryParseColor(String? colorHex) {
    final raw = (colorHex ?? '').trim();
    if (raw.isEmpty) {
      return null;
    }
    final normalized = raw.replaceAll('#', '');
    if (normalized.length != 6 && normalized.length != 8) {
      return null;
    }
    final buffer = StringBuffer();
    if (normalized.length == 6) {
      buffer.write('ff');
    }
    buffer.write(normalized);
    return Color(int.tryParse(buffer.toString(), radix: 16) ?? 0xFFFEE8E2);
  }
}
