import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/utils/date_formatter.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';

class PostCard extends StatelessWidget {
  const PostCard({
    required this.post,
    this.onTap,
    this.onLikeTap,
    super.key,
  });

  final PostModel post;
  final VoidCallback? onTap;
  final VoidCallback? onLikeTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      child: InkWell(
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                post.autorNome,
                style: Theme.of(context).textTheme.titleSmall,
              ),
              const SizedBox(height: 4),
              Text(
                DateFormatter.formatDateTime(post.dataCriacao),
                style: Theme.of(context).textTheme.bodySmall,
              ),
              if ((post.titulo ?? '').isNotEmpty) ...[
                const SizedBox(height: 8),
                Text(
                  post.titulo!,
                  style: Theme.of(context).textTheme.titleMedium,
                ),
              ],
              const SizedBox(height: 8),
              Text(post.conteudo),
              if ((post.imagemUrl ?? '').isNotEmpty) ...[
                const SizedBox(height: 8),
                ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: Image.network(
                    post.imagemUrl!,
                    height: 180,
                    width: double.infinity,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => const SizedBox.shrink(),
                  ),
                ),
              ],
              const SizedBox(height: 10),
              Row(
                children: [
                  InkWell(
                    onTap: onLikeTap,
                    child: Row(
                      children: [
                        Icon(
                          post.usuarioCurtiu
                              ? Icons.favorite
                              : Icons.favorite_outline,
                          size: 18,
                        ),
                        const SizedBox(width: 4),
                        Text('${post.totalCurtidas}'),
                      ],
                    ),
                  ),
                  const SizedBox(width: 16),
                  Row(
                    children: [
                      const Icon(Icons.comment_outlined, size: 18),
                      const SizedBox(width: 4),
                      Text('${post.totalComentarios}'),
                    ],
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
