import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';

class StoryBubble extends StatefulWidget {
  const StoryBubble({
    required this.name,
    this.imageUrl,
    this.onTap,
    this.isViewed = false,
    this.isAddAction = false,
    this.showLive = false,
    super.key,
  });

  final String name;
  final String? imageUrl;
  final VoidCallback? onTap;
  final bool isViewed;
  final bool isAddAction;
  final bool showLive;

  @override
  State<StoryBubble> createState() => _StoryBubbleState();
}

class _StoryBubbleState extends State<StoryBubble> {
  bool _hovering = false;
  bool _pressed = false;

  @override
  Widget build(BuildContext context) {
    final hasImage = (widget.imageUrl ?? '').isNotEmpty;
    final scale = _pressed
        ? 0.97
        : _hovering
            ? 1.04
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
        behavior: HitTestBehavior.opaque,
        child: AnimatedScale(
          duration: AppDurations.fast,
          curve: AppCurves.easeInOut,
          scale: scale,
          child: SizedBox(
            width: 82,
            child: Column(
              children: [
                Stack(
                  clipBehavior: Clip.none,
                  children: [
                    AnimatedContainer(
                      duration: AppDurations.fast,
                      curve: AppCurves.easeInOut,
                      padding: const EdgeInsets.all(2.5),
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        gradient: LinearGradient(
                          colors: widget.isViewed
                              ? [
                                  EventXColors.socialTextMuted
                                      .withValues(alpha: 0.22),
                                  EventXColors.socialTextMuted
                                      .withValues(alpha: 0.15),
                                ]
                              : const [
                                  EventXColors.socialAccent,
                                  EventXColors.socialAccentAlt,
                                ],
                          begin: Alignment.topLeft,
                          end: Alignment.bottomRight,
                        ),
                        boxShadow: _hovering
                            ? [
                                BoxShadow(
                                  color: EventXColors.socialAccent
                                      .withValues(alpha: 0.25),
                                  blurRadius: 18,
                                  offset: const Offset(0, 6),
                                ),
                              ]
                            : const [],
                      ),
                      child: CircleAvatar(
                        radius: 28,
                        backgroundColor: EventXColors.socialSurface,
                        backgroundImage:
                            hasImage ? NetworkImage(widget.imageUrl!) : null,
                        child: hasImage
                            ? null
                            : widget.isAddAction
                                ? const Icon(
                                    Icons.add,
                                    color: EventXColors.socialText,
                                  )
                                : Text(
                                    widget.name.trim().isEmpty
                                        ? 'U'
                                        : widget.name.trim().characters.first,
                                    style: const TextStyle(
                                      color: EventXColors.socialText,
                                      fontWeight: FontWeight.w700,
                                    ),
                                  ),
                      ),
                    ),
                    if (widget.showLive)
                      Positioned(
                        right: -2,
                        bottom: -2,
                        child: AnimatedOpacity(
                          duration: AppDurations.fast,
                          opacity: _hovering ? 1 : 0.92,
                          child: Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 6,
                              vertical: 2,
                            ),
                            decoration: BoxDecoration(
                              color: EventXColors.socialAccent,
                              borderRadius:
                                  BorderRadius.circular(EventXRadius.pill),
                              border: Border.all(
                                color: EventXColors.socialBackground,
                              ),
                            ),
                            child: const Text(
                              'LIVE',
                              style: TextStyle(
                                color: Colors.white,
                                fontSize: 9,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                          ),
                        ),
                      ),
                  ],
                ),
                const SizedBox(height: 6),
                Text(
                  widget.name,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: EventXColors.socialTextMuted,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
