import 'package:projeto_eventx_flutter/features/budget/data/models/quote_model.dart';
import 'package:projeto_eventx_flutter/features/orders/data/models/order_model.dart';
import 'package:projeto_eventx_flutter/features/profile/data/models/user_profile_model.dart';
import 'package:projeto_eventx_flutter/models/event_list_item.dart';

class DashboardSummaryModel {
  const DashboardSummaryModel({
    required this.profile,
    required this.events,
    required this.orders,
    required this.quotes,
  });

  final UserProfileModel profile;
  final List<EventListItem> events;
  final List<OrderModel> orders;
  final List<QuoteModel> quotes;

  int get totalEvents => events.length;
  int get totalOrders => orders.length;
  int get totalQuotes => quotes.length;
  String get userName => profile.nome;
  List<EventListItem> get recentEvents {
    final sorted = [...events]
      ..sort((a, b) => b.dataEvento.compareTo(a.dataEvento));
    return sorted.take(4).toList(growable: false);
  }
}
