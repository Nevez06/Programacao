import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/story_highlight_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/social_story_repository.dart';
import 'package:projeto_eventx_flutter/features/social_story/presentation/social_stories_page.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/social_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/empty_social_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/highlight_card.dart';

class StoryHighlightsPage extends StatefulWidget {
  const StoryHighlightsPage({super.key});

  @override
  State<StoryHighlightsPage> createState() => _StoryHighlightsPageState();
}

class _StoryHighlightsPageState extends State<StoryHighlightsPage> {
  late Future<List<StoryHighlightModel>> _futureHighlights;

  @override
  void initState() {
    super.initState();
    _futureHighlights = _loadHighlights();
  }

  Future<List<StoryHighlightModel>> _loadHighlights() {
    return context.read<SocialStoryRepository>().getMyHighlights();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureHighlights = _loadHighlights();
    });
    await _futureHighlights;
  }

  Future<void> _openCreateStoryAndRefresh() async {
    final createdStoryId =
        await Navigator.of(context).pushNamed<int>(AppRoutes.socialCreateStory);
    if (!mounted || createdStoryId == null) {
      return;
    }

    await _refresh();
  }

  @override
  Widget build(BuildContext context) {
    return SocialShell(
      currentRoute: AppRoutes.socialStories,
      title: 'Destaques',
      child: FutureBuilder<List<StoryHighlightModel>>(
        future: _futureHighlights,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                EmptySocialState(
                  icon: Icons.cloud_off_outlined,
                  title: 'Falha ao carregar destaques',
                  message: 'Tente novamente para atualizar as colecoes.',
                  actionLabel: 'Recarregar',
                  onAction: _refresh,
                ),
              ],
            );
          }

          final highlights = snapshot.data ?? const <StoryHighlightModel>[];
          return ListView(
            padding: const EdgeInsets.all(EventXSpacing.md),
            children: [
              const Text(
                'Colecoes em destaque',
                style: TextStyle(
                  color: EventXColors.socialText,
                  fontSize: 22,
                  fontWeight: FontWeight.w800,
                ),
              ),
              const SizedBox(height: EventXSpacing.xs),
              const Text(
                'Organize stories por tema para fortalecer sua vitrine social.',
                style: TextStyle(color: EventXColors.socialTextMuted),
              ),
              const SizedBox(height: EventXSpacing.md),
              if (highlights.isEmpty)
                EmptySocialState(
                  title: 'Nenhum destaque criado',
                  message:
                      'Crie stories e depois agrupe os melhores momentos em colecoes.',
                  actionLabel: 'Criar story',
                  onAction: _openCreateStoryAndRefresh,
                )
              else
                SizedBox(
                  height: 210,
                  child: ListView.separated(
                    scrollDirection: Axis.horizontal,
                    itemCount: highlights.length + 1,
                    separatorBuilder: (_, __) =>
                        const SizedBox(width: EventXSpacing.sm),
                    itemBuilder: (_, index) {
                      if (index == 0) {
                        return HighlightCard(
                          title: 'Adicionar',
                          subtitle: 'Novo destaque',
                          isAdd: true,
                          onTap: _openCreateStoryAndRefresh,
                        );
                      }

                      final item = highlights[index - 1];
                      return HighlightCard(
                        title: item.nome,
                        subtitle: '${item.totalStories} stories',
                        imageUrl: item.capaUrl ??
                            (item.stories.isNotEmpty
                                ? item.stories.first.mediaUrl
                                : null),
                        onTap: () {
                          final firstStoryId = item.stories.isNotEmpty
                              ? item.stories.first.id
                              : null;
                          Navigator.of(context).pushNamed(
                            AppRoutes.socialStoryView,
                            arguments:
                                SocialStoryViewerArgs(storyId: firstStoryId),
                          );
                        },
                      );
                    },
                  ),
                ),
              const SizedBox(height: EventXSpacing.lg),
              FilledButton.tonalIcon(
                onPressed: _openCreateStoryAndRefresh,
                icon: const Icon(Icons.add_circle_outline_rounded),
                label: const Text('Criar novo story'),
              ),
            ],
          );
        },
      ),
    );
  }
}
