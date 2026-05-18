class OrderModel {
  const OrderModel({
    required this.id,
    required this.eventId,
    required this.eventName,
    required this.productId,
    required this.productName,
    required this.quantity,
    required this.totalPrice,
    required this.status,
    required this.orderedAt,
    required this.title,
    required this.total,
    required this.createdAt,
    required this.expenseGenerated,
    this.supplierId,
    this.supplierName,
  });

  final String id;
  final int eventId;
  final String eventName;
  final String productId;
  final String productName;
  final int quantity;
  final double totalPrice;
  final String status;
  final DateTime orderedAt;
  final String title;
  final double total;
  final DateTime createdAt;
  final bool expenseGenerated;
  final int? supplierId;
  final String? supplierName;

  factory OrderModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    return OrderModel(
      id: (read('id') ?? '').toString(),
      eventId: (read('eventId') as num?)?.toInt() ?? 0,
      eventName: (read('eventName') ?? '').toString(),
      productId: (read('productId') ?? '').toString(),
      productName: (read('productName') ?? '').toString(),
      quantity: (read('quantity') as num?)?.toInt() ?? 0,
      totalPrice: (read('totalPrice') as num?)?.toDouble() ?? 0,
      status: (read('status') ?? '').toString(),
      orderedAt: DateTime.tryParse((read('orderedAt') ?? '').toString()) ??
          DateTime.now(),
      title: (read('title') ?? read('productName') ?? read('name') ?? '')
          .toString(),
      total: (read('total') as num?)?.toDouble() ??
          (read('totalPrice') as num?)?.toDouble() ??
          0,
      createdAt: DateTime.tryParse((read('createdAt') ?? '').toString()) ??
          DateTime.tryParse((read('orderedAt') ?? '').toString()) ??
          DateTime.now(),
      expenseGenerated: (read('expenseGenerated') as bool?) ?? false,
      supplierId: (read('supplierId') as num?)?.toInt(),
      supplierName: _nullable(read('supplierName')),
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }

  static String? _nullable(dynamic value) {
    final text = value?.toString();
    if (text == null || text.trim().isEmpty) {
      return null;
    }
    return text;
  }
}

class UpdateOrderStatusRequest {
  const UpdateOrderStatusRequest(this.status);

  final String status;

  Map<String, dynamic> toJson() => {'status': status};
}

class CreateOrderRequest {
  const CreateOrderRequest({
    required this.eventId,
    required this.title,
    required this.total,
    this.quantity = 1,
    this.supplierId,
    this.description,
    this.status,
  });

  final int eventId;
  final String title;
  final int quantity;
  final double total;
  final int? supplierId;
  final String? description;
  final String? status;

  Map<String, dynamic> toJson() => {
        'eventId': eventId,
        'title': title,
        'quantity': quantity,
        'total': total,
        'supplierId': supplierId,
        'description': description,
        'status': status,
      }..removeWhere((key, value) => value == null);
}
