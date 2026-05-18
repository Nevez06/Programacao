import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/social_story_model.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/services/social_repository.dart';
import 'package:projeto_eventx_flutter/shared/layouts/social_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_text_field.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/story_create_option_card.dart';

class CreateStoryPage extends StatefulWidget {
  const CreateStoryPage({super.key});

  @override
  State<CreateStoryPage> createState() => _CreateStoryPageState();
}

class _CreateStoryPageState extends State<CreateStoryPage> {
  final _captionController = TextEditingController();
  final _mediaUrlController = TextEditingController();
  final _eventIdController = TextEditingController();

  int _selectedVisual = 0;
  bool _submitting = false;

  static const _visuals = [
    ('Sunset', [Color(0xFFE9566B), Color(0xFF2A1638)]),
    ('Ocean', [Color(0xFF1D7FBF), Color(0xFF121A3A)]),
    ('Aurora', [Color(0xFF7B61FF), Color(0xFF1B2C42)]),
  ];

  static const _createOptions = [
    (
      'Upload rapido',
      'Use uma URL de imagem para publicar agora',
      Icons.upload_rounded,
      [Color(0xFF725BFF), Color(0xFF2C2F71)],
    ),
    (
      'Camera',
      'Fluxo mobile de camera fica pronto na proxima iteracao',
      Icons.camera_alt_outlined,
      [Color(0xFFFF5E84), Color(0xFF6C2745)],
    ),
    (
      'Musica',
      'Prepare trilha para compor o story',
      Icons.music_note_rounded,
      [Color(0xFFFFA64D), Color(0xFF6D3823)],
    ),
    (
      'Colagem',
      'Monte composicao com multiplos momentos',
      Icons.grid_view_rounded,
      [Color(0xFF1F9EE3), Color(0xFF1B335E)],
    ),
    (
      'Modelos',
      'Escolha templates premium EventX',
      Icons.auto_awesome_mosaic_outlined,
      [Color(0xFF55C58D), Color(0xFF1D4A4F)],
    ),
  ];

  @override
  void dispose() {
    _captionController.dispose();
    _mediaUrlController.dispose();
    _eventIdController.dispose();
    super.dispose();
  }

  Future<void> _publishStory() async {
    final mediaUrl = _mediaUrlController.text.trim();
    if (mediaUrl.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Informe a URL da imagem do story.')),
      );
      return;
    }

    final parsedUri = Uri.tryParse(mediaUrl);
    if (parsedUri == null ||
        (!parsedUri.hasScheme ||
            (parsedUri.scheme != 'http' && parsedUri.scheme != 'https'))) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('URL invalida. Use http(s).')),
      );
      return;
    }

    final eventIdText = _eventIdController.text.trim();
    final eventId = eventIdText.isEmpty ? null : int.tryParse(eventIdText);
    if (eventIdText.isNotEmpty && eventId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('EventId deve ser numerico.')),
      );
      return;
    }

    setState(() => _submitting = true);
    try {
      final createdStory = await context.read<SocialRepository>().createStory(
            CreateSocialStoryRequest(
              mediaUrl: mediaUrl,
              caption: _captionController.text.trim().isEmpty
                  ? null
                  : _captionController.text.trim(),
              theme: _visuals[_selectedVisual].$1,
              eventId: eventId,
            ),
          );

      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Story publicado com sucesso.')),
      );
      Navigator.of(context).pop<int>(createdStory.id);
    } catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(_resolveErrorMessage(error))),
      );
    } finally {
      if (mounted) {
        setState(() => _submitting = false);
      }
    }
  }

  String _resolveErrorMessage(Object? error) {
    if (error is ApiException) {
      return error.message;
    }
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Nao foi possivel publicar o story.';
    }
    return raw.replaceFirst('Exception: ', '');
  }

  @override
  Widget build(BuildContext context) {
    final visual = _visuals[_selectedVisual];

    return SocialShell(
      currentRoute: AppRoutes.socialCreateStory,
      title: 'Criar Story',
      child: ListView(
        padding: const EdgeInsets.all(EventXSpacing.md),
        children: [
          Container(
            height: 420,
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(EventXRadius.xl),
              gradient: LinearGradient(
                colors: visual.$2,
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              border: Border.all(color: EventXColors.socialStroke),
              boxShadow: EventXShadows.card,
            ),
            child: Padding(
              padding: const EdgeInsets.all(EventXSpacing.md),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Row(
                    children: [
                      Icon(Icons.radio_button_checked,
                          color: EventXColors.socialAccent, size: 10),
                      SizedBox(width: 6),
                      Text(
                        'PREVIEW STORY',
                        style: TextStyle(
                          color: EventXColors.socialTextMuted,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ],
                  ),
                  const Spacer(),
                  Text(
                    _captionController.text.isEmpty
                        ? 'Seu momento EventX\ncomeca aqui'
                        : _captionController.text,
                    style: const TextStyle(
                      color: EventXColors.socialText,
                      fontSize: 28,
                      fontWeight: FontWeight.w800,
                      height: 1.1,
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: EventXSpacing.md),
          AppTextField(
            controller: _mediaUrlController,
            label: 'URL da imagem',
            hint: 'https://.../story.jpg',
            prefixIcon: Icons.link_rounded,
          ),
          const SizedBox(height: EventXSpacing.sm),
          AppTextField(
            controller: _eventIdController,
            label: 'EventId (opcional)',
            hint: 'Ex: 12',
            prefixIcon: Icons.event_outlined,
            keyboardType: TextInputType.number,
          ),
          const SizedBox(height: EventXSpacing.sm),
          AppTextField(
            controller: _captionController,
            label: 'Legenda',
            hint: 'Compartilhe o momento do seu evento',
            maxLines: 2,
            onChanged: (_) => setState(() {}),
          ),
          const SizedBox(height: EventXSpacing.sm),
          const Text(
            'Visual',
            style: TextStyle(
              color: EventXColors.socialText,
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: EventXSpacing.xs),
          Wrap(
            spacing: EventXSpacing.xs,
            runSpacing: EventXSpacing.xs,
            children: List.generate(_visuals.length, (index) {
              final item = _visuals[index];
              final selected = index == _selectedVisual;
              return ChoiceChip(
                selected: selected,
                label: Text(item.$1),
                onSelected: (_) => setState(() => _selectedVisual = index),
              );
            }),
          ),
          const SizedBox(height: EventXSpacing.md),
          const Text(
            'Criacao rapida',
            style: TextStyle(
              color: EventXColors.socialText,
              fontWeight: FontWeight.w800,
              fontSize: 18,
            ),
          ),
          const SizedBox(height: EventXSpacing.xs),
          ..._createOptions.map(
            (option) => Padding(
              padding: const EdgeInsets.only(bottom: EventXSpacing.sm),
              child: StoryCreateOptionCard(
                title: option.$1,
                subtitle: option.$2,
                icon: option.$3,
                gradient: option.$4,
                onTap: () => ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text(option.$2)),
                ),
              ),
            ),
          ),
          const SizedBox(height: EventXSpacing.sm),
          PrimaryButton(
            label: 'Publicar Story',
            icon: Icons.send_rounded,
            expand: true,
            isLoading: _submitting,
            onPressed: _submitting ? null : _publishStory,
          ),
        ],
      ),
    );
  }
}
