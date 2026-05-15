import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/events/data/models/save_event_model.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_service.dart';
import 'package:projeto_eventx_flutter/models/event_details.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';

class EventsRepository {
  EventsRepository({
    required ApiClient apiClient,
    required EventsService eventsService,
  })  : _apiClient = apiClient,
        _eventsService = eventsService;

  final ApiClient _apiClient;
  final EventsService _eventsService;

  Future<List<EventListItem>> getEvents() async {
    final jsonList = await _eventsService.getEvents();
    if (jsonList.isEmpty) {
      return const <EventListItem>[];
    }

    try {
      return jsonList
          .whereType<Map>()
          .map((item) => EventListItem.fromJson(Map<String, dynamic>.from(item)))
          .toList(growable: false);
    } catch (error) {
      throw Exception('Falha ao interpretar os dados de /api/events: $error');
    }
  }

  Future<EventDetails> getEventDetails(int eventId) async {
    final json = await _apiClient.getJson(ApiEndpoints.eventById(eventId));
    return EventDetails.fromJson(json);
  }

  Future<EventDetails> createEvent(SaveEventModel model) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.events,
      body: model.toJson(),
    );
    return EventDetails.fromJson(json);
  }

  Future<EventDetails> updateEvent(int eventId, SaveEventModel model) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.eventById(eventId),
      body: model.toJson(),
    );
    return EventDetails.fromJson(json);
  }

  Future<void> deleteEvent(int eventId) async {
    await _apiClient.deleteVoid(ApiEndpoints.eventById(eventId));
  }
}
