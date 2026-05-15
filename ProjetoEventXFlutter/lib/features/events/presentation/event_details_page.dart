import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/utils/date_formatter.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/models/event_details.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_error_message.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_loader.dart';

class EventDetailsPage extends StatefulWidget {
  const EventDetailsPage({
    required this.eventId,
    super.key,
  });

  final int eventId;

  @override
  State<EventDetailsPage> createState() => _EventDetailsPageState();
}

class _EventDetailsPageState extends State<EventDetailsPage> {
  late Future<EventDetails> _futureEvent;

  @override
  void initState() {
    super.initState();
    _futureEvent = _load();
  }

  Future<EventDetails> _load() {
    return context.read<EventsRepository>().getEventDetails(widget.eventId);
  }

  Future<void> _refresh() async {
    setState(() {
      _futureEvent = _load();
    });
    await _futureEvent;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Detalhes do evento'),
        actions: [
          IconButton(
            onPressed: () => Navigator.of(context).pushNamed(
              AppRoutes.organizerCreateEvent,
              arguments: CreateEventRouteArgs(eventId: widget.eventId),
            ),
            icon: const Icon(Icons.edit_outlined),
            tooltip: 'Editar evento',
          ),
        ],
      ),
      body: FutureBuilder<EventDetails>(
        future: _futureEvent,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const AppLoader(message: 'Carregando detalhes...');
          }

          if (snapshot.hasError || snapshot.data == null) {
            return AppErrorMessage(
              message: 'Nao foi possivel carregar os detalhes do evento.',
              onRetry: _refresh,
            );
          }

          final event = snapshot.data!;
          return RefreshIndicator(
            onRefresh: _refresh,
            child: ListView(
              padding: const EdgeInsets.all(16),
              children: [
                Text(
                  event.nomeEvento,
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                const SizedBox(height: 8),
                Text(
                  event.descricaoEvento,
                  style: Theme.of(context).textTheme.bodyMedium,
                ),
                const SizedBox(height: 16),
                _InfoRow(
                  label: 'Data',
                  value: DateFormatter.formatDateTime(event.dataEvento),
                ),
                _InfoRow(label: 'Tipo', value: event.tipoEvento),
                _InfoRow(label: 'Status', value: event.statusEvento),
                _InfoRow(label: 'Hora inicio', value: event.horaInicio),
                _InfoRow(label: 'Hora fim', value: event.horaFim),
                _InfoRow(
                  label: 'Publico estimado',
                  value: event.publicoEstimado.toString(),
                ),
                _InfoRow(
                  label: 'Custo estimado',
                  value: event.custoEstimado.toStringAsFixed(2),
                ),
                _InfoRow(
                  label: 'Local',
                  value: event.localNome ?? 'Nao informado',
                ),
                if ((event.localEndereco ?? '').isNotEmpty)
                  _InfoRow(
                    label: 'Endereco',
                    value: event.localEndereco!,
                  ),
              ],
            ),
          );
        },
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({
    required this.label,
    required this.value,
  });

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 130,
            child: Text(
              '$label:',
              style: Theme.of(context).textTheme.titleSmall,
            ),
          ),
          Expanded(child: Text(value)),
        ],
      ),
    );
  }
}
