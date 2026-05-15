import 'package:projeto_eventx_flutter/features/budget/data/quotes_repository.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/features/orders/data/orders_repository.dart';
import 'package:projeto_eventx_flutter/features/organizer_dashboard/data/models/dashboard_summary_model.dart';
import 'package:projeto_eventx_flutter/features/profile/data/profile_repository.dart';

class DashboardRepository {
  DashboardRepository({
    required EventsRepository eventsRepository,
    required OrdersRepository ordersRepository,
    required QuotesRepository quotesRepository,
    required ProfileRepository profileRepository,
  })  : _eventsRepository = eventsRepository,
        _ordersRepository = ordersRepository,
        _quotesRepository = quotesRepository,
        _profileRepository = profileRepository;

  final EventsRepository _eventsRepository;
  final OrdersRepository _ordersRepository;
  final QuotesRepository _quotesRepository;
  final ProfileRepository _profileRepository;

  Future<DashboardSummaryModel> fetchSummary() async {
    final eventsFuture = _eventsRepository.getEvents();
    final ordersFuture = _ordersRepository.getOrders();
    final quotesFuture = _quotesRepository.getQuotes();
    final profileFuture = _profileRepository.fetchMe();

    final events = await eventsFuture;
    final orders = await ordersFuture;
    final quotes = await quotesFuture;
    final profile = await profileFuture;

    return DashboardSummaryModel(
      profile: profile,
      events: events,
      orders: orders,
      quotes: quotes,
    );
  }
}
