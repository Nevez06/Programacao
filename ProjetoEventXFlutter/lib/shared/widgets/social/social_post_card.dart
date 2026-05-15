import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/hover_card.dart';

class SocialPostCard extends StatefulWidget {
  const SocialPostCard({
    required this.post,
    this.onTap,
    this.onLikeTap,
    this.onCommentTap,
    super.key,
  });

  final PostModel post;
  final VoidCallback? onTap;
  final VoidCallback? onLikeTap;
  final VoidCallback? onCommentTap;

  @override
  State<SocialPostCard> createState() => _SocialPostCardState();
}

class _SocialPostCardState extends State<SocialPostCard> {
  @override
  Widget build(BuildContext context) {
    final title = (widget.post.titulo ?? '').isNotEmpty
        ? widget.post.titulo!
        : widget.post.conteudo;
    final showImage = (widget.post.imagemUrl ?? '').isNotEmpty;

    return Container(
      margin: const EdgeInsets.symmetric(
        horizontal: EventXSpacing.md,
        vertical: EventXSpacing.xs,
      ),
      child: HoverCard(
        onTap: widget.onTap,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        hoverTranslateY: -3,
        hoverScale: 1.006,
        baseShadow: EventXShadows.soft,
        hoverShadow: [
          BoxShadow(
            color: EventXColors.socialAccent.withValues(alpha: 0.16),
            blurRadius: 20,
            offset: const Offset(0, 8),
          ),
        ],
        child: Material(
          color: EventXColors.socialSurface,
          borderRadius: BorderRadius.circular(EventXRadius.lg),
          child: InkWell(
            borderRadius: BorderRadius.circular(EventXRadius.lg),
            onTap: widget.onTap,
            child: Container(
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(EventXRadius.lg),
                border: Border.all(color: EventXColors.socialStroke),
              ),
              padding: const EdgeInsets.all(EventXSpacing.md),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      _Avatar(
                        name: widget.post.autorNome,
                        avatarUrl: widget.post.autorFotoUrl,
                      ),
                      const SizedBox(width: EventXSpacing.sm),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              widget.post.autorNome,
                              style: const TextStyle(
                                color: EventXColors.socialText,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                            const SizedBox(height: 2),
                            Text(
                              _postMeta(widget.post),
                              style: const TextStyle(
                                color: EventXColors.socialTextMuted,
                                fontSize: 12,
                              ),
                            ),
                          ],
                        ),
                      ),
                      if ((widget.post.categoria ?? '').isNotEmpty)
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: EventXSpacing.xs,
                            vertical: 4,
                          ),
                          decoration: BoxDecoration(
                            color: EventXColors.socialSurfaceAlt,
                            borderRadius:
                                BorderRadius.circular(EventXRadius.pill),
                          ),
                          child: Text(
                            widget.post.categoria!,
                            style: const TextStyle(
                              color: EventXColors.socialTextMuted,
                              fontSize: 11,
                            ),
                          ),
                        ),
                      IconButton(
                        onPressed: widget.onTap,
                        icon: const Icon(
                          Icons.more_horiz_rounded,
                          color: EventXColors.socialTextMuted,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: EventXSpacing.sm),
                  if (showImage)
                    Hero(
                      tag: 'social-post-media-${widget.post.id}',
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(EventXRadius.md),
                        child: AspectRatio(
                          aspectRatio: 16 / 11,
                          child: Image.network(
                            widget.post.imagemUrl!,
                            fit: BoxFit.cover,
                            errorBuilder: (_, __, ___) => Container(
                              color: EventXColors.socialSurfaceAlt,
                              alignment: Alignment.center,
                              child: const Icon(
                                Icons.image_not_supported_outlined,
                                color: EventXColors.socialTextMuted,
                              ),
                            ),
                          ),
                        ),
                      ),
                    )
                  else
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(EventXSpacing.md),
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(EventXRadius.md),
                        gradient: LinearGradient(
                          colors: [
                            EventXColors.socialAccent.withValues(alpha: 0.22),
                            EventXColors.socialAccentAlt
                                .withValues(alpha: 0.22),
                          ],
                          begin: Alignment.topLeft,
                          end: Alignment.bottomRight,
                        ),
                      ),
                      child: const Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(
                            Icons.photo_filter_outlined,
                            color: EventXColors.socialTextMuted,
                          ),
                          SizedBox(width: EventXSpacing.xs),
                          Text(
                            'Post sem midia',
                            style:
                                TextStyle(color: EventXColors.socialTextMuted),
                          ),
                        ],
                      ),
                    ),
                  const SizedBox(height: EventXSpacing.sm),
                  Text(
                    title,
                    maxLines: 3,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: EventXColors.socialText,
                      fontSize: 15,
                      height: 1.32,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  if ((widget.post.nomeEvento ?? '').isNotEmpty) ...[
                    const SizedBox(height: EventXSpacing.xs),
                    Row(
                      children: [
                        const Icon(
                          Icons.location_on_outlined,
                          size: 14,
                          color: EventXColors.socialTextMuted,
                        ),
                        const SizedBox(width: 4),
                        Expanded(
                          child: Text(
                            widget.post.nomeEvento!,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: const TextStyle(
                              color: EventXColors.socialTextMuted,
                              fontSize: 12,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                  const SizedBox(height: EventXSpacing.sm),
                  Row(
                    children: [
                      _ActionButton(
                        icon: widget.post.usuarioCurtiu
                            ? Icons.favorite
                            : Icons.favorite_border,
                        active: widget.post.usuarioCurtiu,
                        label: widget.post.hideLikesCount
                            ? 'Curtido'
                            : widget.post.totalCurtidas.toString(),
                        onTap: widget.onLikeTap,
                      ),
                      const SizedBox(width: EventXSpacing.xs),
                      _ActionButton(
                        icon: Icons.mode_comment_outlined,
                        label: widget.post.totalComentarios.toString(),
                        onTap: widget.onCommentTap ?? widget.onTap,
                      ),
                      const Spacer(),
                      TextButton.icon(
                        onPressed: widget.onTap,
                        icon: const Icon(Icons.open_in_new_rounded, size: 16),
                        label: const Text('Detalhes'),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  String _postMeta(PostModel post) {
    final from = _relative(post.dataCriacao);
    if ((post.localizacao ?? '').isNotEmpty) {
      return '$from • ${post.localizacao!}';
    }
    return from;
  }

  String _relative(DateTime date) {
    final now = DateTime.now();
    final diff = now.difference(date);
    if (diff.inMinutes < 1) {
      return 'agora';
    }
    if (diff.inMinutes < 60) {
      return 'ha ${diff.inMinutes} min';
    }
    if (diff.inHours < 24) {
      return 'ha ${diff.inHours} h';
    }
    if (diff.inDays < 7) {
      return 'ha ${diff.inDays} d';
    }
    return '${date.day}/${date.month}/${date.year}';
  }
}

class _ActionButton extends StatefulWidget {
  const _ActionButton({
    required this.icon,
    required this.label,
    this.active = false,
    this.onTap,
  });

  final IconData icon;
  final String label;
  final bool active;
  final VoidCallback? onTap;

  @override
  State<_ActionButton> createState() => _ActionButtonState();
}

class _ActionButtonState extends State<_ActionButton> {
  bool _hovering = false;
  bool _pressed = false;

  @override
  Widget build(BuildContext context) {
    final color = widget.active
        ? EventXColors.socialAccent
        : EventXColors.socialTextMuted;
    final scale = _pressed
        ? 0.96
        : _hovering
            ? 1.03
            : 1.0;

    return MouseRegion(
      onEnter: (_) => setState(() => _hovering = true),
      onExit: (_) => setState(() {
        _hovering = false;
        _pressed = false;
      }),
      child: GestureDetector(
        onTap: widget.onTap,
        onTapDown: (_) => setState(() => _pressed = true),
        onTapCancel: () => setState(() => _pressed = false),
        onTapUp: (_) => setState(() => _pressed = false),
        child: AnimatedScale(
          duration: AppDurations.fast,
          curve: AppCurves.easeInOut,
          scale: scale,
          child: AnimatedContainer(
            duration: AppDurations.fast,
            curve: AppCurves.easeInOut,
            padding: const EdgeInsets.symmetric(
              horizontal: EventXSpacing.sm,
              vertical: 6,
            ),
            decoration: BoxDecoration(
              color: _hovering
                  ? EventXColors.socialSurfaceAlt.withValues(alpha: 0.95)
                  : EventXColors.socialSurfaceAlt,
              borderRadius: BorderRadius.circular(EventXRadius.pill),
            ),
            child: Row(
              children: [
                AnimatedSwitcher(
                  duration: AppDurations.fast,
                  child: Icon(
                    widget.icon,
                    key: ValueKey(widget.icon),
                    size: 18,
                    color: color,
                  ),
                ),
                const SizedBox(width: 4),
                Text(
                  widget.label,
                  style: TextStyle(color: color, fontWeight: FontWeight.w600),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _Avatar extends StatelessWidget {
  const _Avatar({required this.name, this.avatarUrl});

  final String name;
  final String? avatarUrl;

  @override
  Widget build(BuildContext context) {
    final hasAvatar = (avatarUrl ?? '').isNotEmpty;
    return Container(
      padding: const EdgeInsets.all(1.5),
      decoration: const BoxDecoration(
        shape: BoxShape.circle,
        gradient: LinearGradient(
          colors: [EventXColors.socialAccent, EventXColors.socialAccentAlt],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
      ),
      child: CircleAvatar(
        radius: 20,
        backgroundColor: EventXColors.socialSurface,
        backgroundImage: hasAvatar ? NetworkImage(avatarUrl!) : null,
        child: hasAvatar
            ? null
            : Text(
                name.trim().isEmpty ? 'U' : name.trim().characters.first,
                style: const TextStyle(
                  color: EventXColors.socialText,
                  fontWeight: FontWeight.w700,
                ),
              ),
      ),
    );
  }
}
