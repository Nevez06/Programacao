import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/orders/data/models/order_model.dart';

class OrdersRepository {
  OrdersRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<OrderModel>> getOrders({int? eventId}) async {
    final list = await _apiClient.getList(
      ApiEndpoints.orders,
      queryParameters: {
        if (eventId != null) 'eventId': eventId,
      },
    );

    final orders = <OrderModel>[];
    for (final item in list) {
      if (item is Map<String, dynamic>) {
        orders.add(OrderModel.fromJson(item));
        continue;
      }
      if (item is Map) {
        orders.add(OrderModel.fromJson(Map<String, dynamic>.from(item)));
      }
    }
    return orders;
  }

  Future<OrderModel> getOrderById(String id) async {
    final json = await _apiClient.getJson(ApiEndpoints.orderById(id));
    return OrderModel.fromJson(json);
  }

  Future<OrderModel> createOrder(CreateOrderRequest request) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.orders,
      body: request.toJson(),
    );
    return OrderModel.fromJson(json);
  }

  Future<OrderModel> updateStatus(
    String id,
    UpdateOrderStatusRequest request,
  ) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.orderStatus(id),
      body: request.toJson(),
    );
    return OrderModel.fromJson(json);
  }
}
