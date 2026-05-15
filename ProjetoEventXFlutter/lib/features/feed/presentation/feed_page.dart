import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/features/feed/data/event_feed_repository.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/event_feed_model.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/empty_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class FeedPage extends StatefulWidget {
  const FeedPage({
    this.embedded = false,
    super.key,
  });

  final bool embedded;

  @override
  State<FeedPage> createState() => _FeedPageState();
}

class _FeedPageState extends State<FeedPage> {
  late Future<List<EventFeedModel>> _futureEvents;

  @override
  void initState() {
    super.initState();
    _futureEvents = _loadEvents();
  }

  Future<List<EventFeedModel>> _loadEvents() {
    return context.read<EventFeedRepository>().getEvents();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureEvents = _loadEvents();
    });
    await _futureEvents;
  }

  Future<void> _openEventDetails(EventFeedModel event) async {
    await Navigator.of(context).pushNamed(
      AppRoutes.eventDetails,
      arguments: EventDetailsRouteArgs(event.id),
    );
    if (!mounted) {
      return;
    }
    await _refresh();
  }

  Widget _buildBody() {
    return FutureBuilder<List<EventFeedModel>>(
      future: _futureEvents,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const LoadingView(message: 'Carregando feed de eventos...');
        }

        if (snapshot.hasError) {
          return ErrorView(
            message: 'Nao foi possivel carregar o feed de eventos.',
            onRetry: _refresh,
          );
        }

        final events = snapshot.data ?? const <EventFeedModel>[];
        if (events.isEmpty) {
          return RefreshIndicator(
            onRefresh: _refresh,
            child: ListView(
              padding: const EdgeInsets.only(top: 8, bottom: 16),
              children: const [
                Padding(
                  padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                  child: SectionTitle(
                    title: 'Feed de eventos',
                    subtitle: 'Lista real vinda de /api/events',
                  ),
                ),
                SizedBox(height: 64),
                EmptyState(
                  icon: Icons.event_busy_outlined,
                  message: 'Nenhum evento encontrado.',
                ),
              ],
            ),
          );
        }

        return RefreshIndicator(
          onRefresh: _refresh,
          child: ListView(
            padding: const EdgeInsets.only(top: 8, bottom: 16),
            children: [
              const Padding(
                padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                child: SectionTitle(
                  title: 'Feed de eventos',
                  subtitle: 'Lista real vinda de /api/events',
                ),
              ),
              ...events.map(
                (event) => _EventFeedCard(
                  event: event,
                  onTap: () => _openEventDetails(event),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    if (widget.embedded) {
      return _buildBody();
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Feed de eventos')),
      body: _buildBody(),
    );
  }
}

class _EventFeedCard extends StatelessWidget {
  const _EventFeedCard({
    required this.event,
    required this.onTap,
  });

  final EventFeedModel event;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final dateLabel = _formatDate(event.startDate);
    final typeLabel = event.type.trim().isEmpty ? 'Evento' : event.type;
    final locationLabel =
        event.location.trim().isEmpty ? 'Local nao informado' : event.location;

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _CoverImage(url: event.coverImageUrl),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      event.name,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.w700,
                          ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      '$typeLabel • $dateLabel',
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                            color: EventXColors.organizerTextMuted,
                          ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      locationLabel,
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                            color: EventXColors.organizerTextMuted,
                          ),
                    ),
                    if (event.description.trim().isNotEmpty) ...[
                      const SizedBox(height: 8),
                      Text(
                        event.description,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                    const SizedBox(height: 8),
                    _StatusBadge(status: event.status),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  static String _formatDate(DateTime? value) {
    if (value == null) {
      return 'Data nao informada';
    }
    final local = value.toLocal();
    final day = local.day.toString().padLeft(2, '0');
    final month = local.month.toString().padLeft(2, '0');
    final year = local.year.toString();
    return '$day/$month/$year';
  }
}

class _CoverImage extends StatelessWidget {
  const _CoverImage({required this.url});

  final String? url;

  @override
  Widget build(BuildContext context) {
    final imageUrl = url?.trim() ?? '';
    return ClipRRect(
      borderRadius: BorderRadius.circular(10),
      child: Container(
        width: 84,
        height: 84,
        color: EventXColors.organizerSurfaceAlt,
        child: imageUrl.isEmpty
            ? const Icon(
                Icons.photo_outlined,
                color: EventXColors.organizerTextMuted,
              )
            : Image.network(
                imageUrl,
                fit: BoxFit.cover,
                errorBuilder: (_, __, ___) => const Icon(
                  Icons.broken_image_outlined,
                  color: EventXColors.organizerTextMuted,
                ),
              ),
      ),
    );
  }
}

class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final normalized = status.trim().toLowerCase();
    final color = switch (normalized) {
      'published' || 'publicado' => EventXColors.success,
      'draft' || 'rascunho' => EventXColors.warning,
      'cancelled' || 'cancelado' => EventXColors.error,
      _ => EventXColors.brand,
    };

    final label = status.trim().isEmpty ? 'Sem status' : status;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.14),
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: color,
          fontWeight: FontWeight.w600,
          fontSize: 12,
        ),
      ),
    );
  }
}
