import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/invitation_templates_repository.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/models/invitation_draft_model.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class EventXEditorPage extends StatefulWidget {
  const EventXEditorPage({
    this.initialDraft,
    super.key,
  });

  final InvitationDraftModel? initialDraft;

  @override
  State<EventXEditorPage> createState() => _EventXEditorPageState();
}

class _EventXEditorPageState extends State<EventXEditorPage> {
  final _draftNameController = TextEditingController();
  final _titleController = TextEditingController();
  final _messageController = TextEditingController();

  int? _draftId;
  int? _eventId;
  int? _templateId;
  String? _previewUrl;
  bool _isSaving = false;
  bool _isDeleting = false;
  String? _saveError;

  @override
  void initState() {
    super.initState();
    _hydrateFromInitialDraft(widget.initialDraft);
  }

  @override
  void dispose() {
    _draftNameController.dispose();
    _titleController.dispose();
    _messageController.dispose();
    super.dispose();
  }

  void _hydrateFromInitialDraft(InvitationDraftModel? draft) {
    final data = _decodeLayout(draft?.layoutJson);
    _draftId = draft?.id;
    _eventId = draft?.eventId;
    _templateId = draft?.templateId;
    _previewUrl = draft?.previewUrl;

    _draftNameController.text =
        (draft?.name ?? '').trim().isEmpty ? 'Novo convite' : draft!.name;
    _titleController.text = (data['title'] ?? '').toString().trim().isEmpty
        ? 'Ana & Bruno'
        : data['title'].toString();
    _messageController.text = (data['message'] ?? '').toString().trim().isEmpty
        ? 'Convidam para celebrar um momento inesquecivel'
        : data['message'].toString();
  }

  Map<String, dynamic> _decodeLayout(String? rawLayoutJson) {
    if ((rawLayoutJson ?? '').trim().isEmpty) {
      return <String, dynamic>{};
    }
    try {
      final decoded = jsonDecode(rawLayoutJson!);
      if (decoded is Map<String, dynamic>) {
        return decoded;
      }
      if (decoded is Map) {
        return decoded.map((key, value) => MapEntry('$key', value));
      }
    } catch (_) {
      return <String, dynamic>{};
    }
    return <String, dynamic>{};
  }

  Future<int?> _pickEventId() async {
    final events = await context.read<EventsRepository>().getEvents();
    if (!mounted) {
      return null;
    }
    if (events.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Crie um evento antes de salvar o rascunho.'),
        ),
      );
      return null;
    }

    int selected = _eventId ?? events.first.id;
    final picked = await showDialog<int>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Associar evento'),
        content: StatefulBuilder(
          builder: (context, setStateDialog) {
            return DropdownButtonFormField<int>(
              initialValue: selected,
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
                setStateDialog(() => selected = value);
              },
            );
          },
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(selected),
            child: const Text('Confirmar'),
          ),
        ],
      ),
    );

    return picked;
  }

  Future<void> _saveDraft() async {
    final invitationsRepository = context.read<InvitationTemplatesRepository>();
    var eventId = _eventId;
    if (eventId == null || eventId <= 0) {
      eventId = await _pickEventId();
      if (eventId == null || eventId <= 0) {
        return;
      }
    }

    setState(() {
      _isSaving = true;
      _saveError = null;
    });

    final layout = jsonEncode({
      'title': _titleController.text.trim(),
      'message': _messageController.text.trim(),
      'eventId': eventId,
      'templateId': _templateId,
    });

    try {
      final request = SaveInvitationDraftRequest(
        id: _draftId,
        eventId: eventId,
        templateId: _templateId,
        name: _draftNameController.text.trim(),
        layoutJson: layout,
        previewUrl: _previewUrl,
      );
      final draft = _draftId == null
          ? await invitationsRepository.saveDraft(request)
          : await invitationsRepository.updateDraft(_draftId!, request);

      if (!mounted) {
        return;
      }

      setState(() {
        _draftId = draft.id;
        _eventId = draft.eventId;
        _templateId = draft.templateId;
      });

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Rascunho salvo com sucesso.')),
      );
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _saveError = _resolveErrorMessage(error);
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(_resolveErrorMessage(error))),
      );
    } finally {
      if (mounted) {
        setState(() {
          _isSaving = false;
        });
      }
    }
  }

  Future<void> _deleteDraft() async {
    if (_draftId == null || _isDeleting) {
      return;
    }

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Excluir rascunho'),
        content: Text(
          'Deseja excluir o rascunho "${_draftNameController.text.trim()}"?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: const Text('Excluir'),
          ),
        ],
      ),
    );

    if (confirmed != true) {
      return;
    }

    if (!mounted) {
      return;
    }

    final invitationsRepository = context.read<InvitationTemplatesRepository>();

    setState(() {
      _isDeleting = true;
      _saveError = null;
    });

    try {
      await invitationsRepository.deleteDraft(_draftId!);
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Rascunho excluido com sucesso.')),
      );
      Navigator.of(context)
          .pushReplacementNamed(AppRoutes.organizerInvitations);
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _saveError = _resolveErrorMessage(error);
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(_resolveErrorMessage(error))),
      );
    } finally {
      if (mounted) {
        setState(() {
          _isDeleting = false;
        });
      }
    }
  }

  String _resolveErrorMessage(Object? error) {
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao processar rascunho.';
    }
    return raw
        .replaceFirst('Exception: ', '')
        .replaceFirst('DioException [connection error]: ', '')
        .replaceFirst('DioException [unknown]: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerEditor,
      title: 'EventX Editor',
      subtitle: 'Estrutura base visual para criacao de convites e templates',
      child: ListView(
        padding: const EdgeInsets.all(EventXSpacing.md),
        children: [
          DashboardHeroCard(
            title: 'Area criativa EventX',
            subtitle:
                'Edicao visual com biblioteca, canvas central e painel de propriedades pronto para evolucao.',
            primaryActionLabel: _isSaving ? 'Salvando...' : 'Salvar rascunho',
            onPrimaryAction: () {
              if (_isSaving) {
                return;
              }
              _saveDraft();
            },
            secondaryActionLabel: 'Voltar templates',
            onSecondaryAction: () => Navigator.of(context)
                .pushNamed(AppRoutes.organizerTemplateGallery),
          ),
          const SizedBox(height: EventXSpacing.sm),
          Wrap(
            spacing: EventXSpacing.xs,
            runSpacing: EventXSpacing.xs,
            children: [
              StatusBadge(
                  label:
                      _draftId == null ? 'Novo rascunho' : 'Draft #$_draftId'),
              StatusBadge(
                label: _eventId == null
                    ? 'Evento nao associado'
                    : 'Evento #$_eventId',
              ),
            ],
          ),
          if (_saveError != null) ...[
            const SizedBox(height: EventXSpacing.sm),
            Text(
              _saveError!,
              style: const TextStyle(color: EventXColors.error),
            ),
          ],
          if (_draftId != null) ...[
            const SizedBox(height: EventXSpacing.sm),
            Align(
              alignment: Alignment.centerLeft,
              child: OutlinedButton.icon(
                onPressed: _isDeleting ? null : _deleteDraft,
                icon: const Icon(Icons.delete_outline_rounded),
                label: Text(_isDeleting ? 'Excluindo...' : 'Excluir rascunho'),
              ),
            ),
          ],
          const SizedBox(height: EventXSpacing.lg),
          const SectionHeader(
            title: 'Workspace de edicao',
            subtitle: 'Fluxo: biblioteca -> canvas -> propriedades',
          ),
          const SizedBox(height: EventXSpacing.sm),
          LayoutBuilder(
            builder: (context, constraints) {
              final wide = constraints.maxWidth >= 1180;
              final medium = constraints.maxWidth >= 860;

              if (wide) {
                return Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Expanded(flex: 2, child: _LibraryPanel()),
                    const SizedBox(width: EventXSpacing.md),
                    Expanded(
                      flex: 5,
                      child: _EditorCanvas(
                        draftNameController: _draftNameController,
                        titleController: _titleController,
                        messageController: _messageController,
                        isSaving: _isSaving,
                        onSave: _saveDraft,
                      ),
                    ),
                    const SizedBox(width: EventXSpacing.md),
                    const Expanded(flex: 2, child: _InspectorPanel()),
                  ],
                );
              }

              if (medium) {
                return Column(
                  children: [
                    _EditorCanvas(
                      draftNameController: _draftNameController,
                      titleController: _titleController,
                      messageController: _messageController,
                      isSaving: _isSaving,
                      onSave: _saveDraft,
                    ),
                    const SizedBox(height: EventXSpacing.sm),
                    const Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(child: _LibraryPanel()),
                        SizedBox(width: EventXSpacing.sm),
                        Expanded(child: _InspectorPanel()),
                      ],
                    ),
                  ],
                );
              }

              return Column(
                children: [
                  _EditorCanvas(
                    draftNameController: _draftNameController,
                    titleController: _titleController,
                    messageController: _messageController,
                    isSaving: _isSaving,
                    onSave: _saveDraft,
                  ),
                  const SizedBox(height: EventXSpacing.sm),
                  const _LibraryPanel(),
                  const SizedBox(height: EventXSpacing.sm),
                  const _InspectorPanel(),
                ],
              );
            },
          ),
        ],
      ),
    );
  }
}

class _LibraryPanel extends StatelessWidget {
  const _LibraryPanel();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Biblioteca',
              style: Theme.of(context)
                  .textTheme
                  .titleMedium
                  ?.copyWith(fontWeight: FontWeight.w700)),
          const SizedBox(height: EventXSpacing.sm),
          const Wrap(
            spacing: EventXSpacing.xs,
            runSpacing: EventXSpacing.xs,
            children: [
              StatusBadge(label: 'Texto'),
              StatusBadge(label: 'Imagem'),
              StatusBadge(label: 'Icones'),
              StatusBadge(label: 'Formas'),
              StatusBadge(label: 'Paleta'),
              StatusBadge(label: 'RSVP CTA'),
            ],
          ),
          const SizedBox(height: EventXSpacing.md),
          const _LibraryItem(
              title: 'Cabecalho Luxo', subtitle: 'Tipografia forte + selo'),
          const _LibraryItem(
              title: 'Hero Floral', subtitle: 'Imagem central + overlay'),
          const _LibraryItem(
              title: 'Bloco RSVP', subtitle: 'Botao + link confirmacao'),
        ],
      ),
    );
  }
}

class _InspectorPanel extends StatelessWidget {
  const _InspectorPanel();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Propriedades',
              style: Theme.of(context)
                  .textTheme
                  .titleMedium
                  ?.copyWith(fontWeight: FontWeight.w700)),
          const SizedBox(height: EventXSpacing.sm),
          const _PropertyRow(label: 'Fonte', value: 'Playfair Display'),
          const _PropertyRow(label: 'Tamanho', value: '32 px'),
          const _PropertyRow(label: 'Cor principal', value: '#D93A3A'),
          const _PropertyRow(label: 'Espacamento', value: '12 px'),
          const _PropertyRow(label: 'Alinhamento', value: 'Centralizado'),
          const SizedBox(height: EventXSpacing.md),
          FilledButton.icon(
            onPressed: () {},
            icon: const Icon(Icons.preview_outlined),
            label: const Text('Preview final'),
          ),
        ],
      ),
    );
  }
}

class _EditorCanvas extends StatelessWidget {
  const _EditorCanvas({
    required this.draftNameController,
    required this.titleController,
    required this.messageController,
    required this.isSaving,
    required this.onSave,
  });

  final TextEditingController draftNameController;
  final TextEditingController titleController;
  final TextEditingController messageController;
  final bool isSaving;
  final Future<void> Function() onSave;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(
                'Canvas principal',
                style: Theme.of(context)
                    .textTheme
                    .titleMedium
                    ?.copyWith(fontWeight: FontWeight.w700),
              ),
              const Spacer(),
              OutlinedButton.icon(
                onPressed: () {},
                icon: const Icon(Icons.undo_rounded),
                label: const Text('Desfazer'),
              ),
              const SizedBox(width: EventXSpacing.xs),
              FilledButton.icon(
                onPressed: isSaving ? null : () => onSave(),
                icon: const Icon(Icons.save_outlined),
                label: Text(isSaving ? 'Salvando...' : 'Salvar'),
              ),
            ],
          ),
          const SizedBox(height: EventXSpacing.md),
          TextField(
            controller: draftNameController,
            decoration: const InputDecoration(
              labelText: 'Nome do rascunho',
              border: OutlineInputBorder(),
            ),
          ),
          const SizedBox(height: EventXSpacing.sm),
          AspectRatio(
            aspectRatio: 16 / 10,
            child: Container(
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(EventXRadius.md),
                gradient: const LinearGradient(
                  colors: [Color(0xFFFCE1DD), Color(0xFFF6CFC8)],
                ),
              ),
              child: Stack(
                children: [
                  const Positioned(
                    top: 18,
                    right: 18,
                    child: StatusBadge(label: 'Template base'),
                  ),
                  Center(
                    child: Container(
                      width: 380,
                      padding: const EdgeInsets.all(EventXSpacing.md),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.82),
                        borderRadius: BorderRadius.circular(EventXRadius.lg),
                      ),
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          TextField(
                            controller: titleController,
                            textAlign: TextAlign.center,
                            style: const TextStyle(
                              fontWeight: FontWeight.w800,
                              fontSize: 28,
                            ),
                            decoration: const InputDecoration(
                              border: InputBorder.none,
                              isDense: true,
                            ),
                          ),
                          const SizedBox(height: 6),
                          TextField(
                            controller: messageController,
                            textAlign: TextAlign.center,
                            maxLines: 2,
                            decoration: const InputDecoration(
                              border: InputBorder.none,
                              isDense: true,
                            ),
                          ),
                          const SizedBox(height: 12),
                          const StatusBadge(label: 'Confirmar presenca'),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _LibraryItem extends StatelessWidget {
  const _LibraryItem({
    required this.title,
    required this.subtitle,
  });

  final String title;
  final String subtitle;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: EventXSpacing.xs),
      padding: const EdgeInsets.all(EventXSpacing.sm),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurfaceAlt,
        borderRadius: BorderRadius.circular(EventXRadius.md),
      ),
      child: Row(
        children: [
          const Icon(Icons.drag_indicator_rounded,
              color: EventXColors.organizerTextMuted),
          const SizedBox(width: EventXSpacing.xs),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title,
                    style: const TextStyle(fontWeight: FontWeight.w600)),
                Text(subtitle,
                    style: const TextStyle(
                        fontSize: 12, color: EventXColors.organizerTextMuted)),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _PropertyRow extends StatelessWidget {
  const _PropertyRow({
    required this.label,
    required this.value,
  });

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: EventXSpacing.xs),
      child: Row(
        children: [
          Expanded(
            child: Text(
              label,
              style: const TextStyle(
                fontWeight: FontWeight.w600,
                color: EventXColors.organizerTextMuted,
              ),
            ),
          ),
          Text(value, style: const TextStyle(fontWeight: FontWeight.w700)),
        ],
      ),
    );
  }
}
