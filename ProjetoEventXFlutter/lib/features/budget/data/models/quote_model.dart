class QuoteModel {
  const QuoteModel({
    required this.id,
    required this.eventId,
    required this.eventName,
    required this.supplierId,
    required this.supplierName,
    required this.organizerId,
    required this.serviceName,
    required this.description,
    required this.estimatedValue,
    required this.status,
    required this.createdAt,
    required this.currentRound,
    this.responseMessage,
    this.responseValue,
    this.responseDate,
    this.counterProposalValue,
    this.counterProposalMessage,
    this.counterProposalDate,
    this.expireAt,
    this.generatedOrderId,
  });

  final int id;
  final int eventId;
  final String eventName;
  final int supplierId;
  final String supplierName;
  final String organizerId;
  final String serviceName;
  final String description;
  final double estimatedValue;
  final String status;
  final DateTime createdAt;
  final String? responseMessage;
  final double? responseValue;
  final DateTime? responseDate;
  final double? counterProposalValue;
  final String? counterProposalMessage;
  final DateTime? counterProposalDate;
  final int currentRound;
  final DateTime? expireAt;
  final String? generatedOrderId;

  factory QuoteModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    return QuoteModel(
      id: (read('id') as num?)?.toInt() ?? 0,
      eventId: (read('eventId') as num?)?.toInt() ?? 0,
      eventName: (read('eventName') ?? '').toString(),
      supplierId: (read('supplierId') as num?)?.toInt() ?? 0,
      supplierName: (read('supplierName') ?? '').toString(),
      organizerId: (read('organizerId') ?? '').toString(),
      serviceName: (read('serviceName') ?? '').toString(),
      description: (read('description') ?? '').toString(),
      estimatedValue: (read('estimatedValue') as num?)?.toDouble() ?? 0,
      status: (read('status') ?? '').toString(),
      createdAt: DateTime.tryParse((read('createdAt') ?? '').toString()) ??
          DateTime.now(),
      responseMessage: _nullable(read('responseMessage')),
      responseValue: (read('responseValue') as num?)?.toDouble(),
      responseDate: _parseDate(read('responseDate')),
      counterProposalValue: (read('counterProposalValue') as num?)?.toDouble(),
      counterProposalMessage: _nullable(read('counterProposalMessage')),
      counterProposalDate: _parseDate(read('counterProposalDate')),
      currentRound: (read('currentRound') as num?)?.toInt() ?? 0,
      expireAt: _parseDate(read('expireAt')),
      generatedOrderId: _nullable(read('generatedOrderId')),
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

  static DateTime? _parseDate(dynamic value) {
    final text = value?.toString();
    if (text == null || text.trim().isEmpty) {
      return null;
    }
    return DateTime.tryParse(text);
  }
}

class CreateQuoteRequest {
  const CreateQuoteRequest({
    required this.eventId,
    required this.supplierId,
    required this.serviceName,
    required this.description,
    required this.estimatedValue,
    this.expireAt,
  });

  final int eventId;
  final int supplierId;
  final String serviceName;
  final String description;
  final double estimatedValue;
  final DateTime? expireAt;

  Map<String, dynamic> toJson() {
    return {
      'eventId': eventId,
      'supplierId': supplierId,
      'serviceName': serviceName,
      'description': description,
      'estimatedValue': estimatedValue,
      'expireAt': expireAt?.toIso8601String(),
    }..removeWhere((key, value) => value == null);
  }
}

class UpdateQuoteStatusRequest {
  const UpdateQuoteStatusRequest({
    required this.status,
    this.message,
    this.value,
  });

  final String status;
  final String? message;
  final double? value;

  Map<String, dynamic> toJson() {
    return {
      'status': status,
      'message': message,
      'value': value,
    }..removeWhere((key, value) => value == null);
  }
}

class NegotiateQuoteRequest {
  const NegotiateQuoteRequest({
    required this.action,
    this.message,
    this.value,
  });

  final String action;
  final String? message;
  final double? value;

  Map<String, dynamic> toJson() {
    return {
      'action': action,
      'message': message,
      'value': value,
    }..removeWhere((key, value) => value == null);
  }
}
