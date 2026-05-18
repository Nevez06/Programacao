import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/utils/date_formatter.dart';
import 'package:projeto_eventx_flutter/features/feed/data/feed_repository.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/feed/presentation/create_post_page.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';

class PostDetailsPage extends StatefulWidget {
  const PostDetailsPage({
    required this.postId,
    super.key,
  });

  final int postId;

  @override
  State<PostDetailsPage> createState() => _PostDetailsPageState();
}

class _PostDetailsPageState extends State<PostDetailsPage> {
  late Future<PostModel> _futurePost;
  PostModel? _post;
  final _commentController = TextEditingController();
  bool _sendingComment = false;
  bool _processingLike = false;

  @override
  void initState() {
    super.initState();
    _futurePost = _loadPost();
  }

  @override
  void dispose() {
    _commentController.dispose();
    super.dispose();
  }

  Future<PostModel> _loadPost() async {
    final post =
        await context.read<FeedRepository>().getPostDetails(widget.postId);
    if (!mounted) {
      return post;
    }
    setState(() {
      _post = post;
    });
    return post;
  }

  Future<void> _refresh() async {
    setState(() {
      _futurePost = _loadPost();
    });
    await _futurePost;
  }

  Future<void> _toggleLike() async {
    if (_post == null || _processingLike) {
      return;
    }

    setState(() {
      _processingLike = true;
    });

    try {
      final current = _post!;
      final result = await context.read<FeedRepository>().toggleLike(
            current.id,
            curtir: !current.usuarioCurtiu,
          );
      if (!mounted) {
        return;
      }
      setState(() {
        _post = _post?.copyWith(
          usuarioCurtiu: result.usuarioCurtiu,
          totalCurtidas: result.totalCurtidas,
        );
      });
    } on ApiException catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.message)),
      );
    } finally {
      if (mounted) {
        setState(() {
          _processingLike = false;
        });
      }
    }
  }

  Future<void> _submitComment() async {
    final post = _post;
    if (post == null || _sendingComment) {
      return;
    }

    final texto = _commentController.text.trim();
    if (texto.isEmpty) {
      return;
    }

    setState(() {
      _sendingComment = true;
    });

    try {
      final comment = await context.read<FeedRepository>().addComment(
            post.id,
            texto: texto,
          );
      if (!mounted) {
        return;
      }
      _commentController.clear();
      final nextComments = [comment, ...post.comentarios];
      setState(() {
        _post = post.copyWith(
          totalComentarios: post.totalComentarios + 1,
          comentarios: nextComments,
        );
      });
    } on ApiException catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.message)),
      );
    } finally {
      if (mounted) {
        setState(() {
          _sendingComment = false;
        });
      }
    }
  }

  Future<void> _deletePost() async {
    final post = _post;
    if (post == null) {
      return;
    }
    final repository = context.read<FeedRepository>();

    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Excluir post'),
        content: const Text('Deseja realmente excluir esta publicacao?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: const Text('Excluir'),
          ),
        ],
      ),
    );

    if (confirm != true) {
      return;
    }

    try {
      await repository.deletePost(post.id);
      if (!mounted) {
        return;
      }
      Navigator.of(context).pop(true);
    } on ApiException catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.message)),
      );
    }
  }

  Future<void> _editPost() async {
    final post = _post;
    if (post == null) {
      return;
    }

    final result = await Navigator.of(context).push<PostModel>(
      MaterialPageRoute<PostModel>(
        builder: (_) => CreatePostPage(initialPost: post),
      ),
    );

    if (!mounted || result == null) {
      return;
    }

    setState(() {
      _post = result;
    });
  }

  Widget _buildLoaded(PostModel post) {
    return RefreshIndicator(
      onRefresh: _refresh,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Row(
            children: [
              CircleAvatar(
                backgroundImage: (post.autorFotoUrl ?? '').isNotEmpty
                    ? NetworkImage(post.autorFotoUrl!)
                    : null,
                child: (post.autorFotoUrl ?? '').isEmpty
                    ? const Icon(Icons.person)
                    : null,
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      post.autorNome,
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    Text(
                      DateFormatter.formatDateTime(post.dataCriacao),
                      style: Theme.of(context).textTheme.bodySmall,
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          if ((post.titulo ?? '').isNotEmpty) ...[
            Text(
              post.titulo!,
              style: Theme.of(context).textTheme.titleLarge,
            ),
            const SizedBox(height: 8),
          ],
          Text(post.conteudo),
          if ((post.imagemUrl ?? '').isNotEmpty) ...[
            const SizedBox(height: 12),
            ClipRRect(
              borderRadius: BorderRadius.circular(12),
              child: Image.network(
                post.imagemUrl!,
                fit: BoxFit.cover,
                errorBuilder: (_, __, ___) => const SizedBox.shrink(),
              ),
            ),
          ],
          const SizedBox(height: 16),
          Row(
            children: [
              FilledButton.tonalIcon(
                onPressed: _processingLike ? null : _toggleLike,
                icon: Icon(
                  post.usuarioCurtiu ? Icons.favorite : Icons.favorite_outline,
                ),
                label: Text('${post.totalCurtidas} curtidas'),
              ),
              const SizedBox(width: 8),
              Text('${post.totalComentarios} comentarios'),
            ],
          ),
          if (post.isOwner) ...[
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              children: [
                OutlinedButton.icon(
                  onPressed: _editPost,
                  icon: const Icon(Icons.edit_outlined),
                  label: const Text('Editar'),
                ),
                OutlinedButton.icon(
                  onPressed: _deletePost,
                  icon: const Icon(Icons.delete_outline),
                  label: const Text('Excluir'),
                ),
              ],
            ),
          ],
          const Divider(height: 32),
          Text(
            'Comentarios',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          ...post.comentarios.map(
            (comment) => ListTile(
              contentPadding: EdgeInsets.zero,
              title: Text(comment.nomeAutor),
              subtitle: Text(comment.texto),
              trailing: Text(
                DateFormatter.formatDateTime(comment.criadoEm),
                style: Theme.of(context).textTheme.bodySmall,
              ),
            ),
          ),
          if (post.commentsEnabled) ...[
            const SizedBox(height: 8),
            TextField(
              controller: _commentController,
              minLines: 1,
              maxLines: 3,
              decoration: const InputDecoration(
                hintText: 'Escreva um comentario...',
              ),
            ),
            const SizedBox(height: 8),
            Align(
              alignment: Alignment.centerRight,
              child: FilledButton.icon(
                onPressed: _sendingComment ? null : _submitComment,
                icon: _sendingComment
                    ? const SizedBox(
                        width: 14,
                        height: 14,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.send),
                label: const Text('Comentar'),
              ),
            ),
          ] else ...[
            const SizedBox(height: 8),
            const Text('Comentarios desativados para este post.'),
          ],
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Detalhes do post')),
      body: FutureBuilder<PostModel>(
        future: _futurePost,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting &&
              _post == null) {
            return const LoadingView(message: 'Carregando post...');
          }

          if (snapshot.hasError && _post == null) {
            return ErrorView(
              message: 'Nao foi possivel carregar o post.',
              onRetry: _refresh,
            );
          }

          final post = _post ?? snapshot.data;
          if (post == null) {
            return const ErrorView(
              message: 'Post nao encontrado.',
            );
          }

          return _buildLoaded(post);
        },
      ),
    );
  }
}
