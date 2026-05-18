import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class StoryViewerItem {
  const StoryViewerItem({
    required this.username,
    required this.title,
    this.imageUrl,
    this.caption,
  });

  final String username;
  final String title;
  final String? imageUrl;
  final String? caption;
}

class StoryViewer extends StatefulWidget {
  const StoryViewer({
    required this.items,
    this.initialIndex = 0,
    this.onIndexChanged,
    super.key,
  });

  final List<StoryViewerItem> items;
  final int initialIndex;
  final ValueChanged<int>? onIndexChanged;

  @override
  State<StoryViewer> createState() => _StoryViewerState();
}

class _StoryViewerState extends State<StoryViewer> {
  late final PageController _controller;
  late int _index;

  @override
  void initState() {
    super.initState();
    _index = widget.initialIndex.clamp(0, widget.items.length - 1);
    _controller = PageController(initialPage: _index);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (widget.items.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      children: [
        Container(
          margin: const EdgeInsets.all(EventXSpacing.md),
          padding: const EdgeInsets.all(EventXSpacing.md),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(EventXRadius.xl),
            border: Border.all(color: EventXColors.socialStroke),
            gradient: const LinearGradient(
              colors: [Color(0xFF23273B), Color(0xFF161A2A)],
              begin: Alignment.topCenter,
              end: Alignment.bottomCenter,
            ),
          ),
          child: Column(
            children: [
              Row(
                children: List.generate(widget.items.length, (i) {
                  final active = i == _index;
                  return Expanded(
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 180),
                      margin: const EdgeInsets.symmetric(horizontal: 2),
                      height: 3,
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(10),
                        color: active
                            ? EventXColors.socialAccent
                            : EventXColors.socialTextMuted
                                .withValues(alpha: 0.25),
                      ),
                    ),
                  );
                }),
              ),
              const SizedBox(height: EventXSpacing.sm),
              SizedBox(
                height: 300,
                child: PageView.builder(
                  controller: _controller,
                  itemCount: widget.items.length,
                  onPageChanged: (value) {
                    setState(() => _index = value);
                    widget.onIndexChanged?.call(value);
                  },
                  itemBuilder: (_, index) =>
                      _StorySlide(item: widget.items[index]),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _StorySlide extends StatelessWidget {
  const _StorySlide({required this.item});

  final StoryViewerItem item;

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(EventXRadius.lg),
      child: Stack(
        fit: StackFit.expand,
        children: [
          if ((item.imageUrl ?? '').isNotEmpty)
            Image.network(
              item.imageUrl!,
              fit: BoxFit.cover,
              errorBuilder: (_, __, ___) => _gradientFallback(),
            )
          else
            _gradientFallback(),
          Container(
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                colors: [Color(0x55000000), Color(0xB3000000)],
                begin: Alignment.topCenter,
                end: Alignment.bottomCenter,
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(EventXSpacing.md),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.username,
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const Spacer(),
                Text(
                  item.title,
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w800,
                    fontSize: 22,
                  ),
                ),
                if ((item.caption ?? '').isNotEmpty) ...[
                  const SizedBox(height: EventXSpacing.xs),
                  Text(
                    item.caption!,
                    style: const TextStyle(color: Color(0xFFE8EAF8)),
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _gradientFallback() {
    return Container(
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [
            EventXColors.socialAccent.withValues(alpha: 0.9),
            EventXColors.socialAccentAlt.withValues(alpha: 0.9),
          ],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
      ),
    );
  }
}
