import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/social_explore/data/models/social_explore_model.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/social_shell.dart';
import 'package:projeto_eventx_flutter/shared/services/social_repository.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/empty_social_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/story_bubble.dart';

class SocialExplorePage extends StatefulWidget {
  const SocialExplorePage({super.key});

  @override
  State<SocialExplorePage> createState() => _SocialExplorePageState();
}

class _SocialExplorePageState extends State<SocialExplorePage> {
  late Future<SocialExploreModel> _futureExplore;

  @override
  void initState() {
    super.initState();
    _futureExplore = _loadExplore();
  }

  Future<SocialExploreModel> _loadExplore() {
    return context.read<SocialRepository>().getExplore();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureExplore = _loadExplore();
    });
    await _futureExplore;
  }

  String _resolveErrorMessage(Object? error) {
    if (error is ApiException) {
      return error.message;
    }
    final raw = error?.toString().trim() ?? '';
    if (raw.isEmpty) {
      return 'Falha ao carregar explore social.';
    }
    return raw.replaceFirst('Exception: ', '');
  }

  @override
  Widget build(BuildContext context) {
    return SocialShell(
      currentRoute: AppRoutes.socialExplore,
      title: 'Explore',
      child: FutureBuilder<SocialExploreModel>(
        future: _futureExplore,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(message: 'Carregando explore...');
          }

          if (snapshot.hasError || snapshot.data == null) {
            return ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                EmptySocialState(
                  icon: Icons.cloud_off_outlined,
                  title: 'Nao foi possivel carregar o Explore',
                  message: _resolveErrorMessage(snapshot.error),
                  actionLabel: 'Recarregar',
                  onAction: _refresh,
                ),
              ],
            );
          }

          final explore = snapshot.data!;
          final categories = explore.categories;
          final trendingPosts = explore.trendingPosts;
          final profiles = explore.trendingProfiles;

          return RefreshIndicator(
            onRefresh: _refresh,
            child: ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                Container(
                  padding: const EdgeInsets.all(EventXSpacing.md),
                  decoration: BoxDecoration(
                    color: EventXColors.socialSurface,
                    borderRadius: BorderRadius.circular(EventXRadius.lg),
                    border: Border.all(color: EventXColors.socialStroke),
                  ),
                  child: const Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Descobertas da comunidade EventX',
                        style: TextStyle(
                          color: EventXColors.socialText,
                          fontWeight: FontWeight.w800,
                          fontSize: 22,
                        ),
                      ),
                      SizedBox(height: EventXSpacing.xs),
                      Text(
                        'Explore real baseado em tendencias e recomendacoes.',
                        style: TextStyle(color: EventXColors.socialTextMuted),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: EventXSpacing.md),
                TextField(
                  decoration: InputDecoration(
                    hintText: 'Busque tendencias, perfis e temas',
                    prefixIcon: const Icon(Icons.search_rounded),
                    suffixIcon: IconButton(
                      onPressed: () {},
                      icon: const Icon(Icons.tune_rounded),
                    ),
                  ),
                ),
                const SizedBox(height: EventXSpacing.md),
                if (categories.isNotEmpty)
                  Wrap(
                    spacing: EventXSpacing.xs,
                    runSpacing: EventXSpacing.xs,
                    children: categories
                        .map((category) => Chip(label: Text(category)))
                        .toList(growable: false),
                  ),
                const SizedBox(height: EventXSpacing.md),
                if (trendingPosts.isEmpty)
                  const EmptySocialState(
                    icon: Icons.local_fire_department_outlined,
                    title: 'Sem tendencias no momento',
                    message:
                        'Assim que houver movimentacao, os posts em alta aparecerao aqui.',
                  )
                else
                  GridView.builder(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: trendingPosts.length,
                    gridDelegate:
                        const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      childAspectRatio: 0.82,
                      crossAxisSpacing: EventXSpacing.sm,
                      mainAxisSpacing: EventXSpacing.sm,
                    ),
                    itemBuilder: (_, index) {
                      final post = trendingPosts[index];
                      return _ExploreCard(
                        title: post.categoria?.trim().isNotEmpty == true
                            ? post.categoria!
                            : 'Em alta',
                        subtitle: post.autorNome,
                        imageUrl: post.imagemUrl,
                        onTap: () => Navigator.of(context).pushNamed(
                          AppRoutes.postDetails,
                          arguments: PostDetailsRouteArgs(post.id),
                        ),
                      );
                    },
                  ),
                const SizedBox(height: EventXSpacing.md),
                const Text(
                  'Perfis em alta',
                  style: TextStyle(
                    color: EventXColors.socialText,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(height: EventXSpacing.sm),
                if (profiles.isEmpty)
                  const EmptySocialState(
                    icon: Icons.person_search_outlined,
                    title: 'Sem perfis em destaque',
                    message: 'Perfis recomendados aparecerao aqui.',
                  )
                else
                  Wrap(
                    spacing: EventXSpacing.sm,
                    runSpacing: EventXSpacing.sm,
                    children: profiles
                        .map(
                          (profile) => StoryBubble(
                            name: profile.username.replaceAll('@', ''),
                            imageUrl: profile.fotoPerfilUrl,
                            onTap: () => Navigator.of(context).pushNamed(
                              AppRoutes.socialProfileByUsername(
                                profile.id.toString(),
                              ),
                            ),
                          ),
                        )
                        .toList(growable: false),
                  ),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _ExploreCard extends StatelessWidget {
  const _ExploreCard({
    required this.title,
    required this.subtitle,
    required this.imageUrl,
    required this.onTap,
  });

  final String title;
  final String subtitle;
  final String? imageUrl;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(EventXRadius.lg),
          gradient: LinearGradient(
            colors: [
              EventXColors.socialAccent.withValues(alpha: 0.22),
              EventXColors.socialAccentAlt.withValues(alpha: 0.26),
            ],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          border: Border.all(color: EventXColors.socialStroke),
        ),
        child: Padding(
          padding: const EdgeInsets.all(EventXSpacing.sm),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Align(
                alignment: Alignment.topRight,
                child: Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.22),
                    borderRadius: BorderRadius.circular(EventXRadius.pill),
                  ),
                  child: const Text(
                    'Trend',
                    style: TextStyle(color: Colors.white, fontSize: 11),
                  ),
                ),
              ),
              const SizedBox(height: EventXSpacing.sm),
              Expanded(
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(EventXRadius.md),
                  child: (imageUrl ?? '').isNotEmpty
                      ? Image.network(imageUrl!,
                          fit: BoxFit.cover, width: double.infinity)
                      : Container(
                          color: EventXColors.socialSurfaceAlt,
                          alignment: Alignment.center,
                          child: const Icon(
                            Icons.photo_outlined,
                            color: EventXColors.socialTextMuted,
                          ),
                        ),
                ),
              ),
              const SizedBox(height: EventXSpacing.xs),
              Text(
                title,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.w800,
                  fontSize: 16,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                subtitle,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(color: Color(0xFFE9ECF7), fontSize: 12),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
