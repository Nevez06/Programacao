import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/empty_state_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/status_badge.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/event_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class EventsPage extends StatefulWidget {
  const EventsPage({
    this.embedded = false,
    this.footer,
    super.key,
  });

  final bool embedded;
  final Widget? footer;

  @override
  State<EventsPage> createState() => _EventsPageState();
}

class _EventsPageState extends State<EventsPage> {
  List<EventListItem> _events = const <EventListItem>[];
  String? _errorMessage;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadEvents();
  }

  Future<void> _loadEvents() async {
    if (!mounted) {
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final events = await context.read<EventsRepository>().getEvents();
      if (!mounted) {
        return;
      }
      setState(() {
        _events = events;
        _isLoading = false;
        _errorMessage = null;
      });
    } on ApiException catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _events = const <EventListItem>[];
        _isLoading = false;
        _errorMessage = error.message;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }
      setState(() {
        _events = const <EventListItem>[];
        _isLoading = false;
        _errorMessage = error.toString().replaceFirst('Exception: ', '');
      });
    }
  }

  Future<void> _refresh() async {
    await _loadEvents();
  }

  Future<void> _deleteEvent(EventListItem event) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Excluir evento'),
        content: Text('Deseja excluir o evento "${event.nomeEvento}"?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: const Text('Excluir'),
          ),
        ],
      ),
    );

    if (confirm != true) {
      return;
    }
    if (!mounted) {
      return;
    }

    try {
      await context.read<EventsRepository>().deleteEvent(event.id);
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Evento excluido com sucesso.')),
      );
      await _refresh();
    } catch (_) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Nao foi possivel excluir o evento.')),
      );
    }
  }

  Widget _buildBody(BuildContext context) {
    if (_isLoading) {
      return const Center(
        child: CircularProgressIndicator(),
      );
    }

    if (_errorMessage != null && _events.isEmpty) {
      return ErrorView(
        message: _errorMessage!,
        onRetry: _refresh,
      );
    }

    if (_events.isEmpty) {
      return RefreshIndicator(
        onRefresh: _refresh,
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16),
          children: [
            EmptyStateCard(
              title: 'Nenhum evento encontrado',
              message: 'Nenhum evento encontrado',
              icon: Icons.event_busy_outlined,
              action: FilledButton.icon(
                onPressed: () =>
                    Navigator.of(context).pushNamed(AppRoutes.organizerCreateEvent),
                icon: const Icon(Icons.add_rounded),
                label: const Text('Criar evento'),
              ),
            ),
            if (widget.footer != null) ...[
              const SizedBox(height: 16),
              widget.footer!,
            ],
          ],
        ),
      );
    }

    final showInlineError = _errorMessage != null && _errorMessage!.isNotEmpty;
    final headerItems = 3;
    final totalItems = headerItems +
        _events.length +
        (showInlineError ? 1 : 0) +
        (widget.footer != null ? 1 : 0);

    return RefreshIndicator(
      onRefresh: _refresh,
      child: ListView.builder(
        padding: const EdgeInsets.only(top: 8, bottom: 16),
        itemCount: totalItems,
        itemBuilder: (context, index) {
          if (index == 0) {
            return Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              child: SectionHeader(
                title: 'Eventos do organizador',
                subtitle:
                    'Visual mais rico para status, categoria e operacao por evento.',
                trailing: FilledButton.tonalIcon(
                  onPressed: () => Navigator.of(context)
                      .pushNamed(AppRoutes.organizerCreateEvent),
                  icon: const Icon(Icons.add_rounded),
                  label: const Text('Novo evento'),
                ),
              ),
            );
          }

          if (index == 1) {
            return const Padding(
              padding: EdgeInsets.symmetric(horizontal: 16),
              child: Wrap(
                spacing: 8,
                runSpacing: 8,
                children: [
                  StatusBadge(label: 'Todos'),
                  StatusBadge(label: 'Confirmado'),
                  StatusBadge(label: 'Planejamento'),
                  StatusBadge(label: 'Finalizado'),
                ],
              ),
            );
          }

          if (index == 2) {
            return const SizedBox(height: 6);
          }

          final eventsStartIndex = headerItems;
          final eventsEndIndexExclusive = eventsStartIndex + _events.length;

          if (index >= eventsStartIndex && index < eventsEndIndexExclusive) {
            final event = _events[index - eventsStartIndex];
            return EventCard(
              event: event,
              onTap: () {
                Navigator.of(context).pushNamed(
                  AppRoutes.eventDetails,
                  arguments: EventDetailsRouteArgs(event.id),
                );
              },
              onEdit: () => Navigator.of(context).pushNamed(
                AppRoutes.organizerCreateEvent,
                arguments: CreateEventRouteArgs(eventId: event.id),
              ),
              onDelete: () => _deleteEvent(event),
            );
          }

          final errorIndex = eventsEndIndexExclusive;
          if (showInlineError && index == errorIndex) {
            return Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
              child: Text(
                _errorMessage!,
                style: Theme.of(context).textTheme.bodySmall?.copyWith(
                      color: Theme.of(context).colorScheme.error,
                    ),
              ),
            );
          }

          if (widget.footer != null) {
            return Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
              child: widget.footer!,
            );
          }

          return Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
            child: const SizedBox.shrink(),
          );
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (widget.embedded) {
      return _buildBody(context);
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Eventos')),
      body: _buildBody(context),
    );
  }
}
