import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/models/social_profile_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/social_story_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/presentation/social_stories_page.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/social_shell.dart';
import 'package:projeto_eventx_flutter/shared/services/social_repository.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/empty_social_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/highlight_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/social_profile_header.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/social_tab_bar.dart';

class SocialUserProfileArgs {
  const SocialUserProfileArgs(this.username);
  final String username;
}

class SocialUserProfilePage extends StatefulWidget {
  const SocialUserProfilePage({
    required this.username,
    super.key,
  });

  final String username;

  @override
  State<SocialUserProfilePage> createState() => _SocialUserProfilePageState();
}

class _SocialUserProfilePageState extends State<SocialUserProfilePage> {
  late Future<_SocialUserBundle> _futureBundle;
  bool _updatingFollow = false;

  @override
  void initState() {
    super.initState();
    _futureBundle = _loadBundle();
  }

  Future<_SocialUserBundle> _loadBundle() async {
    final repository = context.read<SocialRepository>();
    final normalized = widget.username.trim().replaceAll('@', '');
    final profileId = int.tryParse(normalized);
    final profile = profileId != null
        ? await repository.getProfileById(profileId)
        : await repository.getProfileByUsername(widget.username);
    final data = await Future.wait<Object>([
      repository.getPostsByProfileId(profile.id, take: 120),
      repository.getStoriesByProfileId(profile.id, take: 60),
    ]);
    return _SocialUserBundle(
      profile: profile,
      posts: data[0] as List<PostModel>,
      stories: data[1] as List<SocialStoryModel>,
    );
  }

  Future<void> _refresh() async {
    setState(() {
      _futureBundle = _loadBundle();
    });
    await _futureBundle;
  }

  Future<void> _toggleFollow(_SocialUserBundle bundle) async {
    if (_updatingFollow) {
      return;
    }

    setState(() => _updatingFollow = true);
    try {
      final repository = context.read<SocialRepository>();
      if (bundle.profile.isFollowing) {
        await repository.unfollowProfile(bundle.profile.id);
      } else {
        await repository.followProfile(bundle.profile.id);
      }
      await _refresh();
    } catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(_resolveErrorMessage(error))),
      );
    } finally {
      if (mounted) {
        setState(() => _updatingFollow = false);
      }
    }
  }

  String _resolveErrorMessage(Object? error) {
    if (error is ApiException) {
      return error.message;
    }
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar perfil social.';
    }
    return raw.replaceFirst('Exception: ', '');
  }

  @override
  Widget build(BuildContext context) {
    final handle = widget.username.startsWith('@')
        ? widget.username
        : '@${widget.username}';
    return SocialShell(
      currentRoute: AppRoutes.socialProfile,
      title: handle,
      child: FutureBuilder<_SocialUserBundle>(
        future: _futureBundle,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(message: 'Carregando perfil...');
          }

          if (snapshot.hasError || snapshot.data == null) {
            return ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                EmptySocialState(
                  icon: Icons.person_off_outlined,
                  title: 'Perfil social nao encontrado',
                  message: _resolveErrorMessage(snapshot.error),
                  actionLabel: 'Tentar novamente',
                  onAction: _refresh,
                ),
              ],
            );
          }

          final bundle = snapshot.data!;
          final stories = bundle.stories;

          return DefaultTabController(
            length: 3,
            child: ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                SocialProfileHeader(
                  displayName: bundle.profile.nomeExibicao,
                  username: bundle.profile.username,
                  role: bundle.profile.tipoPerfil,
                  posts: bundle.profile.postsCount,
                  followers: bundle.profile.followersCount,
                  following: bundle.profile.followingCount,
                  bio: bundle.profile.bio,
                  avatarUrl: bundle.profile.fotoPerfilUrl,
                  primaryActionLabel:
                      bundle.profile.isFollowing ? 'Seguindo' : 'Seguir',
                  secondaryActionLabel: 'Mensagem',
                  onPrimaryAction: () => _toggleFollow(bundle),
                  onSecondaryAction: () {},
                ),
                const SizedBox(height: EventXSpacing.md),
                if (stories.isNotEmpty)
                  SizedBox(
                    height: 180,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      itemCount: stories.length,
                      separatorBuilder: (_, __) =>
                          const SizedBox(width: EventXSpacing.sm),
                      itemBuilder: (_, index) {
                        final story = stories[index];
                        return HighlightCard(
                          title: story.eventName ?? 'Story',
                          subtitle: story.authorName,
                          imageUrl: story.mediaUrl,
                          onTap: () => Navigator.of(context).pushNamed(
                            AppRoutes.socialStoryView,
                            arguments: SocialStoryViewerArgs(
                              storyId: story.id,
                              initialIndex: index,
                            ),
                          ),
                        );
                      },
                    ),
                  )
                else
                  const EmptySocialState(
                    icon: Icons.play_circle_outline_rounded,
                    title: 'Sem stories ativos',
                    message:
                        'Este perfil nao possui stories ativos no momento.',
                  ),
                const SizedBox(height: EventXSpacing.md),
                const SocialTabBar(tabs: ['Posts', 'Stories', 'Salvos']),
                const SizedBox(height: EventXSpacing.md),
                SizedBox(
                  height: 430,
                  child: TabBarView(
                    children: [
                      _PostsGrid(posts: bundle.posts),
                      _StoriesGrid(stories: stories),
                      const _PlaceholderTab(
                        icon: Icons.bookmark_border_rounded,
                        text: 'Colecoes salvas deste perfil',
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _PostsGrid extends StatelessWidget {
  const _PostsGrid({required this.posts});

  final List<PostModel> posts;

  @override
  Widget build(BuildContext context) {
    if (posts.isEmpty) {
      return const EmptySocialState(
        icon: Icons.grid_view_rounded,
        title: 'Sem posts publicados',
        message: 'Este perfil ainda nao possui publicacoes no feed.',
      );
    }
    return GridView.builder(
      itemCount: posts.length,
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 3,
        crossAxisSpacing: EventXSpacing.xs,
        mainAxisSpacing: EventXSpacing.xs,
      ),
      itemBuilder: (_, index) {
        final post = posts[index];
        return GestureDetector(
          onTap: () => Navigator.of(context).pushNamed(
            AppRoutes.postDetails,
            arguments: PostDetailsRouteArgs(post.id),
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(EventXRadius.sm),
            child: (post.imagemUrl ?? '').isNotEmpty
                ? Image.network(post.imagemUrl!, fit: BoxFit.cover)
                : Container(
                    color: EventXColors.socialSurface,
                    alignment: Alignment.center,
                    child: const Icon(
                      Icons.photo_outlined,
                      color: EventXColors.socialTextMuted,
                    ),
                  ),
          ),
        );
      },
    );
  }
}

class _StoriesGrid extends StatelessWidget {
  const _StoriesGrid({required this.stories});

  final List<SocialStoryModel> stories;

  @override
  Widget build(BuildContext context) {
    if (stories.isEmpty) {
      return const EmptySocialState(
        icon: Icons.play_circle_outline_rounded,
        title: 'Nenhum story neste perfil',
        message: 'Quando houver stories ativos, eles aparecem aqui.',
      );
    }

    return GridView.builder(
      itemCount: stories.length,
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 3,
        crossAxisSpacing: EventXSpacing.xs,
        mainAxisSpacing: EventXSpacing.xs,
      ),
      itemBuilder: (_, index) {
        final story = stories[index];
        return GestureDetector(
          onTap: () => Navigator.of(context).pushNamed(
            AppRoutes.socialStoryView,
            arguments: SocialStoryViewerArgs(
              storyId: story.id,
              initialIndex: index,
            ),
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(EventXRadius.sm),
            child: Image.network(story.mediaUrl, fit: BoxFit.cover),
          ),
        );
      },
    );
  }
}

class _PlaceholderTab extends StatelessWidget {
  const _PlaceholderTab({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, color: EventXColors.socialTextMuted, size: 34),
          const SizedBox(height: EventXSpacing.xs),
          Text(text,
              style: const TextStyle(color: EventXColors.socialTextMuted)),
        ],
      ),
    );
  }
}

class _SocialUserBundle {
  const _SocialUserBundle({
    required this.profile,
    required this.posts,
    required this.stories,
  });

  final SocialProfileModel profile;
  final List<PostModel> posts;
  final List<SocialStoryModel> stories;
}
