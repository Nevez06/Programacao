import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/features/events/data/models/save_event_model.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_text_field.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/secondary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class CreateEventPage extends StatefulWidget {
  const CreateEventPage({this.eventId, super.key});

  final int? eventId;

  @override
  State<CreateEventPage> createState() => _CreateEventPageState();
}

class _CreateEventPageState extends State<CreateEventPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _typeController = TextEditingController();
  final _locationController = TextEditingController();
  final _dateController = TextEditingController();
  final _timeController = TextEditingController();
  final _coverController = TextEditingController();
  final _audienceController = TextEditingController();
  final _costController = TextEditingController();
  bool _isSubmitting = false;
  bool _loadingExisting = false;

  bool get _isEditing => widget.eventId != null;

  @override
  void initState() {
    super.initState();
    if (_isEditing) {
      _loadExistingEvent();
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descriptionController.dispose();
    _typeController.dispose();
    _locationController.dispose();
    _dateController.dispose();
    _timeController.dispose();
    _coverController.dispose();
    _audienceController.dispose();
    _costController.dispose();
    super.dispose();
  }

  Future<void> _loadExistingEvent() async {
    setState(() => _loadingExisting = true);
    try {
      final existing = await context
          .read<EventsRepository>()
          .getEventDetails(widget.eventId!);
      if (!mounted) {
        return;
      }

      _nameController.text = existing.nomeEvento;
      _descriptionController.text = existing.descricaoEvento;
      _typeController.text = existing.tipoEvento;
      _locationController.text = existing.localNome ?? '';
      final localDate = existing.dataEvento.toLocal();
      _dateController.text =
          '${localDate.day.toString().padLeft(2, '0')}/${localDate.month.toString().padLeft(2, '0')}/${localDate.year}';
      _timeController.text = existing.horaInicio;
      _coverController.text = existing.imagemCapa ?? '';
      _audienceController.text = existing.publicoEstimado.toString();
      _costController.text = existing.custoEstimado.toStringAsFixed(2);
    } catch (_) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Nao foi possivel carregar o evento.')),
      );
    } finally {
      if (mounted) {
        setState(() => _loadingExisting = false);
      }
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final parsedDate = _parseDate(_dateController.text.trim());
    if (parsedDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Data invalida. Use DD/MM/AAAA.')),
      );
      return;
    }

    final horaInicio = _parseTime(_timeController.text.trim());
    if (horaInicio == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Horario invalido. Use HH:mm.')),
      );
      return;
    }

    final publico =
        int.tryParse(_audienceController.text.replaceAll('.', '').trim()) ?? 0;
    final custo = _parseCurrency(_costController.text);
    final model = SaveEventModel(
      nomeEvento: _nameController.text.trim(),
      dataEvento: parsedDate,
      descricaoEvento: _descriptionController.text.trim(),
      tipoEvento: _typeController.text.trim(),
      statusEvento: 'Draft',
      horaInicio: horaInicio,
      horaFim: _addOneHour(horaInicio),
      publicoEstimado: publico,
      custoEstimado: custo,
      localNome: _locationController.text.trim(),
      imagemCapa: _coverController.text.trim(),
    );

    setState(() => _isSubmitting = true);
    try {
      final repository = context.read<EventsRepository>();
      final saved = _isEditing
          ? await repository.updateEvent(widget.eventId!, model)
          : await repository.createEvent(model);
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            _isEditing
                ? 'Evento atualizado com sucesso.'
                : 'Evento criado com sucesso.',
          ),
        ),
      );
      Navigator.of(context).pushNamedAndRemoveUntil(
        AppRoutes.eventDetails,
        (route) => route.settings.name == AppRoutes.organizerEvents,
        arguments: EventDetailsRouteArgs(saved.id),
      );
    } catch (_) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            _isEditing
                ? 'Nao foi possivel atualizar o evento.'
                : 'Nao foi possivel criar o evento.',
          ),
        ),
      );
    } finally {
      if (mounted) {
        setState(() => _isSubmitting = false);
      }
    }
  }

  DateTime? _parseDate(String value) {
    final split = value.split('/');
    if (split.length == 3) {
      final day = int.tryParse(split[0]);
      final month = int.tryParse(split[1]);
      final year = int.tryParse(split[2]);
      if (day != null && month != null && year != null) {
        return DateTime(year, month, day);
      }
    }
    return DateTime.tryParse(value);
  }

  String? _parseTime(String value) {
    final split = value.split(':');
    if (split.length != 2) {
      return null;
    }
    final hour = int.tryParse(split[0]);
    final minute = int.tryParse(split[1]);
    if (hour == null || minute == null) {
      return null;
    }
    if (hour < 0 || hour > 23 || minute < 0 || minute > 59) {
      return null;
    }
    final hh = hour.toString().padLeft(2, '0');
    final mm = minute.toString().padLeft(2, '0');
    return '$hh:$mm';
  }

  String _addOneHour(String value) {
    final split = value.split(':');
    final hour = int.tryParse(split.first) ?? 0;
    final minute = int.tryParse(split.last) ?? 0;
    final nextHour = (hour + 1) % 24;
    return '${nextHour.toString().padLeft(2, '0')}:${minute.toString().padLeft(2, '0')}';
  }

  double _parseCurrency(String value) {
    final normalized = value
        .replaceAll('R\$', '')
        .replaceAll('.', '')
        .replaceAll(',', '.')
        .trim();
    return double.tryParse(normalized) ?? 0;
  }

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerCreateEvent,
      title: _isEditing ? 'Editar Evento' : 'Criar Evento',
      subtitle: _isEditing
          ? 'Atualize os dados do evento com persistencia real'
          : 'Formulario premium com blocos claros para operacao',
      child: _loadingExisting
          ? const Center(child: CircularProgressIndicator())
          : ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                SectionHeader(
                  title: _isEditing ? 'Editar evento' : 'Novo evento',
                  subtitle:
                      'Preencha as secoes e publique seu planejamento com consistencia.',
                  trailing: SecondaryButton(
                    label: 'Voltar para eventos',
                    icon: Icons.arrow_back_rounded,
                    onPressed: () => Navigator.of(context)
                        .pushReplacementNamed(AppRoutes.organizerEvents),
                  ),
                ),
                const SizedBox(height: EventXSpacing.md),
                Form(
                  key: _formKey,
                  child: Column(
                    children: [
                      _SectionCard(
                        title: 'Dados principais',
                        subtitle: 'Nome, categoria e contexto inicial',
                        child: Column(
                          children: [
                            AppTextField(
                              controller: _nameController,
                              label: 'Nome do evento',
                              hint: 'Ex: Casamento Ana e Bruno',
                              prefixIcon: Icons.event_note_outlined,
                              validator: (value) => (value ?? '').trim().isEmpty
                                  ? 'Informe o nome.'
                                  : null,
                            ),
                            const SizedBox(height: EventXSpacing.sm),
                            AppTextField(
                              controller: _typeController,
                              label: 'Categoria / Tipo',
                              hint: 'Casamento, corporativo, aniversario...',
                              prefixIcon: Icons.category_outlined,
                              validator: (value) => (value ?? '').trim().isEmpty
                                  ? 'Informe a categoria.'
                                  : null,
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: EventXSpacing.md),
                      _SectionCard(
                        title: 'Data, horario e local',
                        subtitle: 'Defina agenda e localizacao principal',
                        child: Column(
                          children: [
                            Row(
                              children: [
                                Expanded(
                                  child: AppTextField(
                                    controller: _dateController,
                                    label: 'Data',
                                    hint: 'DD/MM/AAAA',
                                    prefixIcon: Icons.calendar_today_outlined,
                                    validator: (value) =>
                                        (value ?? '').trim().isEmpty
                                            ? 'Informe a data.'
                                            : null,
                                  ),
                                ),
                                const SizedBox(width: EventXSpacing.sm),
                                Expanded(
                                  child: AppTextField(
                                    controller: _timeController,
                                    label: 'Horario',
                                    hint: '19:30',
                                    prefixIcon: Icons.schedule_outlined,
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: EventXSpacing.sm),
                            AppTextField(
                              controller: _locationController,
                              label: 'Local',
                              hint: 'Cidade / Espaco do evento',
                              prefixIcon: Icons.location_on_outlined,
                              validator: (value) => (value ?? '').trim().isEmpty
                                  ? 'Informe o local.'
                                  : null,
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: EventXSpacing.md),
                      _SectionCard(
                        title: 'Descricao, capa e custos',
                        subtitle: 'Contexto visual e financeiro do evento',
                        child: Column(
                          children: [
                            AppTextField(
                              controller: _descriptionController,
                              label: 'Descricao do evento',
                              hint:
                                  'Objetivos, publico e experiencia esperada...',
                              prefixIcon: Icons.notes_outlined,
                              maxLines: 4,
                            ),
                            const SizedBox(height: EventXSpacing.sm),
                            AppTextField(
                              controller: _coverController,
                              label: 'URL da capa (opcional)',
                              hint: 'https://.../imagem.jpg',
                              prefixIcon: Icons.image_outlined,
                            ),
                            const SizedBox(height: EventXSpacing.sm),
                            Row(
                              children: [
                                Expanded(
                                  child: AppTextField(
                                    controller: _audienceController,
                                    label: 'Publico estimado',
                                    hint: '150',
                                    keyboardType: TextInputType.number,
                                    prefixIcon: Icons.group_outlined,
                                  ),
                                ),
                                const SizedBox(width: EventXSpacing.sm),
                                Expanded(
                                  child: AppTextField(
                                    controller: _costController,
                                    label: 'Custo previsto',
                                    hint: 'R\$ 25.000',
                                    keyboardType: TextInputType.number,
                                    prefixIcon: Icons.paid_outlined,
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: EventXSpacing.lg),
                      Row(
                        children: [
                          Expanded(
                            child: SecondaryButton(
                              label: _isEditing
                                  ? 'Salvar alteracoes'
                                  : 'Salvar rascunho',
                              icon: Icons.save_outlined,
                              onPressed: _isSubmitting ? null : _submit,
                            ),
                          ),
                          const SizedBox(width: EventXSpacing.sm),
                          Expanded(
                            child: PrimaryButton(
                              label: _isEditing
                                  ? 'Atualizar evento'
                                  : 'Publicar evento',
                              icon: Icons.check_rounded,
                              isLoading: _isSubmitting,
                              onPressed: _isSubmitting ? null : _submit,
                              expand: true,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
    );
  }
}

class _SectionCard extends StatelessWidget {
  const _SectionCard({
    required this.title,
    required this.subtitle,
    required this.child,
  });

  final String title;
  final String subtitle;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.lg),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
        boxShadow: EventXShadows.soft,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.w700,
                ),
          ),
          const SizedBox(height: 4),
          Text(subtitle, style: Theme.of(context).textTheme.bodyMedium),
          const SizedBox(height: EventXSpacing.md),
          child,
        ],
      ),
    );
  }
}
