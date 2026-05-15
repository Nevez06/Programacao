import 'package:projeto_eventx_flutter/features/feed/data/event_feed_remote_data_source.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/event_feed_model.dart';

class EventFeedRepository {
  EventFeedRepository({
    required EventFeedRemoteDataSource remoteDataSource,
  }) : _remoteDataSource = remoteDataSource;

  final EventFeedRemoteDataSource _remoteDataSource;

  Future<List<EventFeedModel>> getEvents() async {
    final rows = await _remoteDataSource.getEvents();
    return rows.map(EventFeedModel.fromJson).toList(growable: false);
  }
}
