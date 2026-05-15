import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/models/event_details.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';

class EventRepository {
  EventRepository(this._source);

  final EventsRepository _source;

  Future<List<EventListItem>> getEvents() => _source.getEvents();
  Future<EventDetails> getEventDetails(int eventId) =>
      _source.getEventDetails(eventId);
}
