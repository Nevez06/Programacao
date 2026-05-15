import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/models/social_profile_model.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/models/update_social_profile_model.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/models/story_highlight_model.dart';
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

class SocialProfilePage extends StatefulWidget {
  const SocialProfilePage({super.key});

  @override
  State<SocialProfilePage> createState() => _SocialProfilePageState();
}

class _SocialProfilePageState extends State<SocialProfilePage> {
  late Future<_SocialProfileBundle> _futureBundle;

  @override
  void initState() {
    super.initState();
    _futureBundle = _loadBundle();
  }

  Future<_SocialProfileBundle> _loadBundle() async {
    final socialRepository = context.read<SocialRepository>();
    final profile = await socialRepository.getMyProfile();
    final data = await Future.wait<Object>([
      socialRepository.getPostsByProfileId(profile.id, take: 120),
      socialRepository.getMyHighlights(),
    ]);

    return _SocialProfileBundle(
      profile: profile,
      posts: data[0] as List<PostModel>,
      highlights: data[1] as List<StoryHighlightModel>,
    );
  }

  Future<void> _refresh() async {
    setState(() {
      _futureBundle = _loadBundle();
    });
    await _futureBundle;
  }

  Future<void> _openCreateStoryAndRefresh() async {
    final createdStoryId =
        await Navigator.of(context).pushNamed<int>(AppRoutes.socialCreateStory);
    if (!mounted || createdStoryId == null) {
      return;
    }

    await _refresh();
  }

  Future<void> _editProfile(_SocialProfileBundle bundle) async {
    final nomeController =
        TextEditingController(text: bundle.profile.nomeExibicao);
    final usernameController = TextEditingController(
        text: bundle.profile.username.replaceAll('@', ''));
    final bioController = TextEditingController(text: bundle.profile.bio ?? '');
    final avatarController =
        TextEditingController(text: bundle.profile.fotoPerfilUrl ?? '');
    final cityController =
        TextEditingController(text: bundle.profile.cidade ?? '');
    final instagramController =
        TextEditingController(text: bundle.profile.instagram ?? '');
    final siteController =
        TextEditingController(text: bundle.profile.site ?? '');

    final formKey = GlobalKey<FormState>();
    bool saving = false;

    await showDialog<void>(
      context: context,
      builder: (dialogContext) {
        return StatefulBuilder(
          builder: (context, setStateDialog) {
            return AlertDialog(
              title: const Text('Editar perfil social'),
              content: SizedBox(
                width: 500,
                child: Form(
                  key: formKey,
                  child: SingleChildScrollView(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        TextFormField(
                          controller: nomeController,
                          decoration: const InputDecoration(
                            labelText: 'Nome de exibicao',
                          ),
                          validator: (value) => (value ?? '').trim().isEmpty
                              ? 'Informe o nome.'
                              : null,
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: usernameController,
                          decoration: const InputDecoration(
                            labelText: 'Username',
                            prefixText: '@',
                          ),
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: bioController,
                          decoration: const InputDecoration(labelText: 'Bio'),
                          minLines: 2,
                          maxLines: 4,
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: avatarController,
                          decoration: const InputDecoration(
                            labelText: 'URL do avatar',
                          ),
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: cityController,
                          decoration:
                              const InputDecoration(labelText: 'Cidade'),
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: instagramController,
                          decoration:
                              const InputDecoration(labelText: 'Instagram'),
                        ),
                        const SizedBox(height: 10),
                        TextFormField(
                          controller: siteController,
                          decoration: const InputDecoration(labelText: 'Site'),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed:
                      saving ? null : () => Navigator.of(dialogContext).pop(),
                  child: const Text('Cancelar'),
                ),
                FilledButton(
                  onPressed: saving
                      ? null
                      : () async {
                          if (!formKey.currentState!.validate()) {
                            return;
                          }
                          setStateDialog(() => saving = true);
                          try {
                            await context
                                .read<SocialRepository>()
                                .updateMyProfile(
                                  UpdateSocialProfileModel(
                                    nomeExibicao: nomeController.text.trim(),
                                    username: usernameController.text.trim(),
                                    bio: bioController.text.trim(),
                                    fotoPerfilUrl: avatarController.text.trim(),
                                    cidade: cityController.text.trim(),
                                    instagram: instagramController.text.trim(),
                                    site: siteController.text.trim(),
                                  ),
                                );
                            if (!mounted || !dialogContext.mounted) {
                              return;
                            }
                            Navigator.of(dialogContext).pop();
                            await _refresh();
                            if (!mounted) {
                              return;
                            }
                            ScaffoldMessenger.of(this.context).showSnackBar(
                              const SnackBar(
                                content: Text('Perfil social atualizado.'),
                              ),
                            );
                          } catch (error) {
                            if (!mounted) {
                              return;
                            }
                            setStateDialog(() => saving = false);
                            ScaffoldMessenger.of(this.context).showSnackBar(
                              SnackBar(
                                content: Text(_resolveErrorMessage(error)),
                              ),
                            );
                          }
                        },
                  child: Text(saving ? 'Salvando...' : 'Salvar'),
                ),
              ],
            );
          },
        );
      },
    );

    nomeController.dispose();
    usernameController.dispose();
    bioController.dispose();
    avatarController.dispose();
    cityController.dispose();
    instagramController.dispose();
    siteController.dispose();
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
    return SocialShell(
      currentRoute: AppRoutes.socialProfile,
      title: 'Perfil Social',
      child: FutureBuilder<_SocialProfileBundle>(
        future: _futureBundle,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const LoadingView(message: 'Carregando perfil social...');
          }

          if (snapshot.hasError || snapshot.data == null) {
            return ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                EmptySocialState(
                  icon: Icons.cloud_off_outlined,
                  title: 'Falha ao carregar perfil social',
                  message: _resolveErrorMessage(snapshot.error),
                  actionLabel: 'Recarregar',
                  onAction: _refresh,
                ),
              ],
            );
          }

          final bundle = snapshot.data!;

          return DefaultTabController(
            length: 3,
            child: ListView(
              padding: const EdgeInsets.all(EventXSpacing.md),
              children: [
                SocialProfileHeader(
                  displayName: bundle.profile.nomeExibicao,
                  username: bundle.profile.username,
                  role: bundle.profile.tipoPerfil,
                  bio: bundle.profile.bio,
                  avatarUrl: bundle.profile.fotoPerfilUrl,
                  posts: bundle.profile.postsCount,
                  followers: bundle.profile.followersCount,
                  following: bundle.profile.followingCount,
                  primaryActionLabel: 'Editar perfil',
                  secondaryActionLabel: 'Compartilhar',
                  onPrimaryAction: () => _editProfile(bundle),
                  onSecondaryAction: () {},
                ),
                const SizedBox(height: EventXSpacing.md),
                _sectionHeader(
                  'Destaques',
                  onTap: () => Navigator.of(context)
                      .pushNamed(AppRoutes.socialHighlights),
                ),
                const SizedBox(height: EventXSpacing.sm),
                SizedBox(
                  height: 200,
                  child: ListView(
                    scrollDirection: Axis.horizontal,
                    children: [
                      HighlightCard(
                        title: 'Adicionar',
                        subtitle: 'Novo destaque',
                        isAdd: true,
                        onTap: _openCreateStoryAndRefresh,
                      ),
                      if (bundle.highlights.isNotEmpty)
                        ...bundle.highlights.map(
                          (item) => Padding(
                            padding:
                                const EdgeInsets.only(left: EventXSpacing.sm),
                            child: HighlightCard(
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
                                  arguments: SocialStoryViewerArgs(
                                      storyId: firstStoryId),
                                );
                              },
                            ),
                          ),
                        ),
                    ],
                  ),
                ),
                const SizedBox(height: EventXSpacing.md),
                const SocialTabBar(tabs: ['Posts', 'Stories', 'Marcados']),
                const SizedBox(height: EventXSpacing.md),
                SizedBox(
                  height: 460,
                  child: TabBarView(
                    children: [
                      _PostGrid(posts: bundle.posts),
                      _StoryTab(
                        onCreateStory: _openCreateStoryAndRefresh,
                      ),
                      const _TaggedTab(),
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

  Widget _sectionHeader(String title, {VoidCallback? onTap}) {
    return Row(
      children: [
        Text(
          title,
          style: const TextStyle(
            color: EventXColors.socialText,
            fontWeight: FontWeight.w800,
            fontSize: 18,
          ),
        ),
        const Spacer(),
        TextButton(
          onPressed: onTap,
          child: const Text('Ver todos'),
        ),
      ],
    );
  }
}

class _PostGrid extends StatelessWidget {
  const _PostGrid({required this.posts});

  final List<PostModel> posts;

  @override
  Widget build(BuildContext context) {
    if (posts.isEmpty) {
      return const EmptySocialState(
        icon: Icons.grid_view_rounded,
        title: 'Sem posts por enquanto',
        message: 'Publique no feed para preencher sua grade social.',
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
        final imageUrl = post.imagemUrl;
        return GestureDetector(
          onTap: () => Navigator.of(context).pushNamed(
            AppRoutes.postDetails,
            arguments: PostDetailsRouteArgs(post.id),
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(EventXRadius.sm),
            child: imageUrl != null && imageUrl.isNotEmpty
                ? Image.network(imageUrl, fit: BoxFit.cover)
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

class _StoryTab extends StatelessWidget {
  const _StoryTab({required this.onCreateStory});

  final VoidCallback onCreateStory;

  @override
  Widget build(BuildContext context) {
    return EmptySocialState(
      icon: Icons.bolt_outlined,
      title: 'Stories ativos por 24h',
      message:
          'Publique bastidores e mantenha sua audiencia engajada durante o evento.',
      actionLabel: 'Criar story',
      onAction: onCreateStory,
    );
  }
}

class _TaggedTab extends StatelessWidget {
  const _TaggedTab();

  @override
  Widget build(BuildContext context) {
    return const EmptySocialState(
      icon: Icons.loyalty_outlined,
      title: 'Sem marcacoes ainda',
      message: 'Quando voce for marcado em posts, eles aparecem aqui.',
    );
  }
}

class _SocialProfileBundle {
  const _SocialProfileBundle({
    required this.profile,
    required this.posts,
    required this.highlights,
  });

  final SocialProfileModel profile;
  final List<PostModel> posts;
  final List<StoryHighlightModel> highlights;
}
