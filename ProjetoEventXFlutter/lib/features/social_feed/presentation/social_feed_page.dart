import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/social_story_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/presentation/social_stories_page.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/social_shell.dart';
import 'package:projeto_eventx_flutter/shared/models/content_intelligence_models.dart';
import 'package:projeto_eventx_flutter/shared/services/feed_ranking_service.dart';
import 'package:projeto_eventx_flutter/shared/services/social_repository.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/animated_empty_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/app_skeleton_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/fade_slide_in.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/empty_social_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/social_post_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/story_bubble.dart';

class SocialFeedPage extends StatefulWidget {
  const SocialFeedPage({super.key});

  @override
  State<SocialFeedPage> createState() => _SocialFeedPageState();
}

class _SocialFeedPageState extends State<SocialFeedPage> {
  late Future<_SocialFeedData> _futureData;
  FeedSortMode _feedMode = FeedSortMode.forYou;

  @override
  void initState() {
    super.initState();
    _futureData = _loadData();
  }

  Future<_SocialFeedData> _loadData() async {
    final socialRepository = context.read<SocialRepository>();
    final results = await Future.wait<Object>([
      socialRepository.getFeed(),
      socialRepository.getStories(take: 100),
    ]);

    return _SocialFeedData(
      posts: results[0] as List<PostModel>,
      stories: results[1] as List<SocialStoryModel>,
    );
  }

  Future<void> _refresh() async {
    setState(() {
      _futureData = _loadData();
    });
    await _futureData;
  }

  Future<void> _toggleLike(PostModel post) async {
    await context
        .read<SocialRepository>()
        .toggleLike(post.id, currentlyLiked: post.usuarioCurtiu);
    if (!mounted) {
      return;
    }
    await _refresh();
  }

  String _resolveErrorMessage(Object? error) {
    if (error is ApiException) {
      return error.message;
    }
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar feed social.';
    }
    return raw.replaceFirst('Exception: ', '');
  }

  Future<void> _openCreateStory() async {
    final createdStoryId =
        await Navigator.of(context).pushNamed<int>(AppRoutes.socialCreateStory);
    if (!mounted || createdStoryId == null) {
      return;
    }

    await _refresh();
    if (!mounted) {
      return;
    }

    Navigator.of(context).pushNamed(
      AppRoutes.socialStoryView,
      arguments: SocialStoryViewerArgs(storyId: createdStoryId),
    );
  }

  @override
  Widget build(BuildContext context) {
    return SocialShell(
      currentRoute: AppRoutes.socialFeed,
      title: 'EventX Social',
      actions: [
        IconButton(
          tooltip: 'Interacoes',
          onPressed: () =>
              Navigator.of(context).pushNamed(AppRoutes.socialInteractions),
          icon: const Icon(Icons.favorite_border_rounded),
        ),
      ],
      child: RefreshIndicator(
        onRefresh: _refresh,
        child: FutureBuilder<_SocialFeedData>(
          future: _futureData,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const _SocialFeedSkeleton();
            }

            if (snapshot.hasError) {
              return ListView(
                physics: const AlwaysScrollableScrollPhysics(),
                padding: const EdgeInsets.all(EventXSpacing.md),
                children: [
                  AnimatedEmptyState(
                    child: EmptySocialState(
                      icon: Icons.cloud_off_rounded,
                      title: 'Nao foi possivel carregar o feed',
                      message: _resolveErrorMessage(snapshot.error),
                      actionLabel: 'Tentar de novo',
                      onAction: _refresh,
                    ),
                  ),
                ],
              );
            }

            final data = snapshot.data ?? const _SocialFeedData.empty();
            final rankedPosts = _rankPosts(context, data.posts);
            final rankedStories = _rankStories(context, data.stories);

            return ListView(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.only(bottom: EventXSpacing.md),
              children: [
                const SizedBox(height: EventXSpacing.sm),
                FadeSlideIn(
                  delay: AppDurations.staggerStep,
                  child: _buildHero(context),
                ),
                const SizedBox(height: EventXSpacing.md),
                FadeSlideIn(
                  delay: AppDurations.staggerStep * 2,
                  child: _buildStories(context, rankedStories),
                ),
                const SizedBox(height: EventXSpacing.sm),
                FadeSlideIn(
                  delay: AppDurations.staggerStep * 3,
                  child: _buildFeedSortSwitcher(),
                ),
                const SizedBox(height: EventXSpacing.xs),
                if (rankedPosts.isEmpty)
                  Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: EventXSpacing.md,
                    ),
                    child: AnimatedEmptyState(
                      child: EmptySocialState(
                        title: 'Sua comunidade ainda esta silenciosa',
                        message:
                            'Crie um post para iniciar os bastidores do seu evento.',
                        actionLabel: 'Criar post',
                        onAction: () => Navigator.of(context)
                            .pushNamed(AppRoutes.postCreate),
                      ),
                    ),
                  )
                else
                  ...List.generate(
                    rankedPosts.length,
                    (index) {
                      final ranked = rankedPosts[index];
                      return FadeSlideIn(
                        delay: AppDurations.staggerStep * (index + 4),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            _RankingReasonBanner(
                              reason: ranked.reasons.first,
                              score: ranked.score,
                              mode: _feedMode,
                            ),
                            SocialPostCard(
                              post: ranked.post,
                              onLikeTap: () => _toggleLike(ranked.post),
                              onTap: () => Navigator.of(context).pushNamed(
                                AppRoutes.postDetails,
                                arguments: PostDetailsRouteArgs(ranked.post.id),
                              ),
                            ),
                          ],
                        ),
                      );
                    },
                  ),
              ],
            );
          },
        ),
      ),
    );
  }

  List<RankedPost> _rankPosts(BuildContext context, List<PostModel> posts) {
    final auth = context.read<AuthController>();
    final currentUserId = auth.session?.userId ?? auth.profile?.id;
    final preferredEvents = posts
        .where((post) => post.isOwner && post.eventoId != null)
        .map((post) => post.eventoId!)
        .toSet();

    final authorAffinity = <int, double>{};
    for (final post in posts) {
      final current = authorAffinity[post.autorUserId] ?? 0.35;
      final boosted = post.usuarioCurtiu
          ? (current + 0.28)
          : (current + ((post.totalCurtidas + post.totalComentarios) / 400));
      authorAffinity[post.autorUserId] = boosted.clamp(0.2, 1);
    }

    final rankingContext = FeedRankingContext(
      currentUserId: currentUserId,
      preferredEventIds: preferredEvents,
      preferredContentTypes: const {'video', 'story', 'foto'},
      authorAffinity: authorAffinity,
      manualPriorityPosts: posts
          .where((post) => post.isPinned || post.totalVisualizacoes > 500)
          .map((post) => post.id)
          .toSet(),
    );

    return context.read<FeedRankingService>().rankFeedPosts(
          posts,
          rankingContext,
          mode: _feedMode,
        );
  }

  List<SocialStoryModel> _rankStories(
    BuildContext context,
    List<SocialStoryModel> stories,
  ) {
    if (stories.isEmpty) {
      return const <SocialStoryModel>[];
    }

    final auth = context.read<AuthController>();
    final currentUserId = auth.session?.userId ?? auth.profile?.id;
    final candidates = stories
        .map(
          (story) => StoryCandidate(
            id: story.id.toString(),
            authorId: story.userId,
            authorName: story.authorName,
            imageUrl: story.mediaUrl,
            createdAt: story.createdAt,
            viewedByCurrentUser: story.viewedByCurrentUser,
            authorAffinity: story.viewedByCurrentUser ? 0.35 : 0.68,
            eventRelevance: story.eventId != null ? 0.74 : 0.45,
            isLive: DateTime.now().difference(story.createdAt).inMinutes < 20,
            eventId: story.eventId,
          ),
        )
        .toList(growable: false);

    final ranking = context.read<FeedRankingService>().rankStories(
          candidates,
          StoryRankingContext(
            currentUserId: currentUserId,
            relevantEventIds: stories
                .where((story) => story.eventId != null)
                .map((story) => story.eventId!)
                .toSet(),
            authorAffinity: {
              for (final story in stories)
                story.userId: story.viewedByCurrentUser ? 0.35 : 0.68,
            },
          ),
        );

    final byId = {for (final story in stories) story.id.toString(): story};
    return ranking
        .map((item) => byId[item.story.id])
        .whereType<SocialStoryModel>()
        .toList(growable: false);
  }

  Widget _buildFeedSortSwitcher() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: EventXSpacing.md),
      child: Wrap(
        spacing: EventXSpacing.xs,
        runSpacing: EventXSpacing.xs,
        children: [
          _ModeChip(
            label: 'Para voce',
            selected: _feedMode == FeedSortMode.forYou,
            onTap: () => setState(() => _feedMode = FeedSortMode.forYou),
          ),
          _ModeChip(
            label: 'Recentes',
            selected: _feedMode == FeedSortMode.recent,
            onTap: () => setState(() => _feedMode = FeedSortMode.recent),
          ),
          _ModeChip(
            label: 'Em alta',
            selected: _feedMode == FeedSortMode.trending,
            onTap: () => setState(() => _feedMode = FeedSortMode.trending),
          ),
        ],
      ),
    );
  }

  Widget _buildHero(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: EventXSpacing.md),
      child: Container(
        padding: const EdgeInsets.all(EventXSpacing.md),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(EventXRadius.lg),
          border: Border.all(color: EventXColors.socialStroke),
          gradient: LinearGradient(
            colors: [
              EventXColors.socialAccent.withValues(alpha: 0.22),
              EventXColors.socialAccentAlt.withValues(alpha: 0.18),
            ],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          boxShadow: [
            BoxShadow(
              color: EventXColors.socialAccent.withValues(alpha: 0.14),
              blurRadius: 28,
              offset: const Offset(0, 10),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Bastidores da comunidade EventX',
              style: TextStyle(
                color: EventXColors.socialText,
                fontWeight: FontWeight.w800,
                fontSize: 20,
              ),
            ),
            const SizedBox(height: EventXSpacing.xs),
            const Text(
              'Feed real com recencia, engajamento e afinidade contextual.',
              style: TextStyle(color: EventXColors.socialTextMuted),
            ),
            const SizedBox(height: EventXSpacing.sm),
            Row(
              children: [
                FilledButton.icon(
                  onPressed: _openCreateStory,
                  icon: const Icon(Icons.add_circle_outline_rounded),
                  label: const Text('Novo story'),
                ),
                const SizedBox(width: EventXSpacing.xs),
                OutlinedButton.icon(
                  onPressed: () =>
                      Navigator.of(context).pushNamed(AppRoutes.socialExplore),
                  icon: const Icon(Icons.explore_outlined),
                  label: const Text('Explorar'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStories(BuildContext context, List<SocialStoryModel> stories) {
    return SizedBox(
      height: 102,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: EventXSpacing.md),
        itemCount: stories.length + 1,
        separatorBuilder: (_, __) => const SizedBox(width: EventXSpacing.sm),
        itemBuilder: (_, index) {
          if (index == 0) {
            return FadeSlideIn(
              delay: AppDurations.staggerStep,
              offset: const Offset(0.06, 0),
              child: StoryBubble(
                name: 'Seu story',
                isViewed: true,
                isAddAction: true,
                onTap: _openCreateStory,
              ),
            );
          }

          final story = stories[index - 1];
          return FadeSlideIn(
            delay: AppDurations.staggerStep * index,
            offset: const Offset(0.06, 0),
            child: StoryBubble(
              name: story.authorName,
              imageUrl: story.authorAvatar,
              isViewed: story.viewedByCurrentUser,
              showLive:
                  DateTime.now().difference(story.createdAt).inMinutes < 20,
              onTap: () => Navigator.of(context).pushNamed(
                AppRoutes.socialStoryView,
                arguments: SocialStoryViewerArgs(
                  storyId: story.id,
                  initialIndex: index - 1,
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}

class _ModeChip extends StatelessWidget {
  const _ModeChip({
    required this.label,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return ChoiceChip(
      label: Text(label),
      selected: selected,
      onSelected: (_) => onTap(),
      selectedColor: EventXColors.socialAccent.withValues(alpha: 0.22),
      backgroundColor: EventXColors.socialSurface,
      side: const BorderSide(color: EventXColors.socialStroke),
      labelStyle: TextStyle(
        color:
            selected ? EventXColors.socialText : EventXColors.socialTextMuted,
        fontWeight: selected ? FontWeight.w700 : FontWeight.w500,
      ),
    );
  }
}

class _RankingReasonBanner extends StatelessWidget {
  const _RankingReasonBanner({
    required this.reason,
    required this.score,
    required this.mode,
  });

  final String reason;
  final double score;
  final FeedSortMode mode;

  @override
  Widget build(BuildContext context) {
    final modeLabel = switch (mode) {
      FeedSortMode.forYou => 'Para voce',
      FeedSortMode.recent => 'Recentes',
      FeedSortMode.trending => 'Em alta',
    };

    return Padding(
      padding: const EdgeInsets.fromLTRB(
        EventXSpacing.md,
        EventXSpacing.xs,
        EventXSpacing.md,
        0,
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.symmetric(
              horizontal: EventXSpacing.xs,
              vertical: 3,
            ),
            decoration: BoxDecoration(
              color: EventXColors.socialSurface,
              borderRadius: BorderRadius.circular(EventXRadius.pill),
              border: Border.all(color: EventXColors.socialStroke),
            ),
            child: Text(
              '$modeLabel • $reason',
              style: const TextStyle(
                color: EventXColors.socialTextMuted,
                fontSize: 11,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          const Spacer(),
          Text(
            'score ${score.toStringAsFixed(2)}',
            style: const TextStyle(
              color: EventXColors.socialTextMuted,
              fontSize: 10,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}

class _SocialFeedSkeleton extends StatelessWidget {
  const _SocialFeedSkeleton();

  @override
  Widget build(BuildContext context) {
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.only(top: EventXSpacing.sm),
      children: const [
        AppSkeletonCard(
          margin: EdgeInsets.symmetric(horizontal: EventXSpacing.md),
          height: 172,
        ),
        SizedBox(height: EventXSpacing.md),
        Padding(
          padding: EdgeInsets.symmetric(horizontal: EventXSpacing.md),
          child: Row(
            children: [
              AppSkeletonCard(
                width: 74,
                height: 74,
                borderRadius: EventXRadius.pill,
                margin: EdgeInsets.only(right: EventXSpacing.sm),
              ),
              AppSkeletonCard(
                width: 74,
                height: 74,
                borderRadius: EventXRadius.pill,
                margin: EdgeInsets.only(right: EventXSpacing.sm),
              ),
              AppSkeletonCard(
                width: 74,
                height: 74,
                borderRadius: EventXRadius.pill,
                margin: EdgeInsets.zero,
              ),
            ],
          ),
        ),
        SizedBox(height: EventXSpacing.sm),
        AppSkeletonCard(height: 360),
        AppSkeletonCard(height: 300),
      ],
    );
  }
}

class _SocialFeedData {
  const _SocialFeedData({
    required this.posts,
    required this.stories,
  });

  const _SocialFeedData.empty()
      : posts = const <PostModel>[],
        stories = const <SocialStoryModel>[];

  final List<PostModel> posts;
  final List<SocialStoryModel> stories;
}
