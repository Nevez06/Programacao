import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/social_story_model.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/social_shell.dart';
import 'package:projeto_eventx_flutter/shared/services/social_repository.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/empty_social_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/story_bubble.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/story_viewer.dart';

class SocialStoryViewerArgs {
  const SocialStoryViewerArgs({
    this.initialIndex = 0,
    this.storyId,
  });

  final int initialIndex;
  final int? storyId;
}

class SocialStoriesPage extends StatefulWidget {
  const SocialStoriesPage({
    this.initialIndex = 0,
    this.storyId,
    super.key,
  });

  final int initialIndex;
  final int? storyId;

  @override
  State<SocialStoriesPage> createState() => _SocialStoriesPageState();
}

class _SocialStoriesPageState extends State<SocialStoriesPage> {
  late Future<List<SocialStoryModel>> _futureStories;
  final Set<int> _markedAsViewed = <int>{};

  @override
  void initState() {
    super.initState();
    _futureStories = _loadStories();
  }

  Future<List<SocialStoryModel>> _loadStories() {
    return context.read<SocialRepository>().getStories(take: 120);
  }

  Future<void> _refresh() async {
    setState(() {
      _futureStories = _loadStories();
    });
    await _futureStories;
  }

  Future<void> _openCreateStoryAndRefresh() async {
    final createdStoryId =
        await Navigator.of(context).pushNamed<int>(AppRoutes.socialCreateStory);
    if (!mounted || createdStoryId == null) {
      return;
    }

    await _refresh();
    if (!mounted) {
      return;
    }

    Navigator.of(context).pushReplacementNamed(
      AppRoutes.socialStoryView,
      arguments: SocialStoryViewerArgs(storyId: createdStoryId),
    );
  }

  int _resolveInitialIndex(List<SocialStoryModel> stories) {
    if (stories.isEmpty) {
      return 0;
    }
    if (widget.storyId != null) {
      final byId = stories.indexWhere((story) => story.id == widget.storyId);
      if (byId >= 0) {
        return byId;
      }
    }
    return widget.initialIndex.clamp(0, stories.length - 1);
  }

  Future<void> _markViewed(SocialStoryModel story) async {
    if (story.viewedByCurrentUser || _markedAsViewed.contains(story.id)) {
      return;
    }

    _markedAsViewed.add(story.id);
    try {
      await context.read<SocialRepository>().markStoryAsViewed(story.id);
    } catch (_) {
      _markedAsViewed.remove(story.id);
    }
  }

  String _resolveErrorMessage(Object? error) {
    if (error is ApiException) {
      return error.message;
    }
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar stories.';
    }
    return raw.replaceFirst('Exception: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return SocialShell(
      currentRoute: AppRoutes.socialStories,
      title: 'Stories',
      actions: [
        IconButton(
          onPressed: () =>
              Navigator.of(context).pushNamed(AppRoutes.socialHighlights),
          icon: const Icon(Icons.star_border_rounded),
          tooltip: 'Destaques',
        ),
      ],
      child: FutureBuilder<List<SocialStoryModel>>(
        future: _futureStories,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(message: 'Carregando stories...');
          }

          if (snapshot.hasError) {
            return ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                EmptySocialState(
                  icon: Icons.cloud_off_outlined,
                  title: 'Nao foi possivel carregar os stories',
                  message: _resolveErrorMessage(snapshot.error),
                  actionLabel: 'Recarregar',
                  onAction: _refresh,
                ),
              ],
            );
          }

          final stories = snapshot.data ?? const <SocialStoryModel>[];
          if (stories.isEmpty) {
            return ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                EmptySocialState(
                  icon: Icons.bolt_outlined,
                  title: 'Nenhum story ativo',
                  message:
                      'Publique o primeiro story para movimentar os bastidores do evento.',
                  actionLabel: 'Criar story',
                  onAction: _openCreateStoryAndRefresh,
                ),
              ],
            );
          }

          final initialIndex = _resolveInitialIndex(stories);
          final initialStory = stories[initialIndex];
          _markViewed(initialStory);

          final viewerItems = stories
              .map(
                (story) => StoryViewerItem(
                  username: story.authorName.startsWith('@')
                      ? story.authorName
                      : '@${story.authorName}',
                  title: story.eventName?.trim().isNotEmpty == true
                      ? story.eventName!
                      : 'Story de ${story.authorName}',
                  imageUrl: story.mediaUrl,
                  caption: story.caption,
                ),
              )
              .toList(growable: false);

          return RefreshIndicator(
            onRefresh: _refresh,
            child: ListView(
              children: [
                const SizedBox(height: EventXSpacing.xs),
                SizedBox(
                  height: 94,
                  child: ListView.separated(
                    scrollDirection: Axis.horizontal,
                    padding: const EdgeInsets.symmetric(
                        horizontal: EventXSpacing.md),
                    separatorBuilder: (_, __) =>
                        const SizedBox(width: EventXSpacing.sm),
                    itemCount: stories.length,
                    itemBuilder: (_, index) {
                      final story = stories[index];
                      final viewed = story.viewedByCurrentUser ||
                          _markedAsViewed.contains(story.id);
                      final isLive =
                          DateTime.now().difference(story.createdAt).inMinutes <
                              20;
                      return StoryBubble(
                        name: story.authorName,
                        imageUrl: story.authorAvatar,
                        isViewed: viewed,
                        showLive: isLive,
                        onTap: () => Navigator.of(context).pushReplacementNamed(
                          AppRoutes.socialStoryView,
                          arguments: SocialStoryViewerArgs(
                            initialIndex: index,
                            storyId: story.id,
                          ),
                        ),
                      );
                    },
                  ),
                ),
                StoryViewer(
                  items: viewerItems,
                  initialIndex: initialIndex,
                  onIndexChanged: (value) {
                    if (value >= 0 && value < stories.length) {
                      _markViewed(stories[value]);
                    }
                  },
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
