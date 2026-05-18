import 'package:projeto_eventx_flutter/core/utils/id_value_parser.dart';

class InvitationDraftModel {
  const InvitationDraftModel({
    required this.id,
    required this.eventId,
    required this.organizerId,
    required this.name,
    required this.layoutJson,
    required this.updatedAt,
    this.templateId,
    this.previewHtml,
    this.previewUrl,
  });

  final int id;
  final int eventId;
  final int organizerId;
  final int? templateId;
  final String name;
  final String layoutJson;
  final String? previewHtml;
  final String? previewUrl;
  final DateTime updatedAt;

  factory InvitationDraftModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    return InvitationDraftModel(
      id: IdValueParser.parseToInt(read('id')),
      eventId: IdValueParser.parseToInt(read('eventId')),
      organizerId: IdValueParser.parseToInt(read('organizerId')),
      templateId: (read('templateId') as num?)?.toInt(),
      name: (read('name') ?? '').toString(),
      layoutJson: (read('layoutJson') ?? '{}').toString(),
      previewHtml: _nullable(read('previewHtml')),
      previewUrl: _nullable(read('previewUrl')),
      updatedAt: DateTime.tryParse((read('updatedAt') ?? '').toString()) ??
          DateTime.now(),
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

class SaveInvitationDraftRequest {
  const SaveInvitationDraftRequest({
    required this.eventId,
    required this.layoutJson,
    this.id,
    this.templateId,
    this.name,
    this.previewHtml,
    this.previewUrl,
  });

  final int? id;
  final int eventId;
  final int? templateId;
  final String? name;
  final String layoutJson;
  final String? previewHtml;
  final String? previewUrl;

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'eventId': eventId,
      'templateId': templateId,
      'name': name,
      'layoutJson': layoutJson,
      'previewHtml': previewHtml,
      'previewUrl': previewUrl,
    }..removeWhere((key, value) => value == null);
  }
}

class CreateInvitationFromTemplateRequest {
  const CreateInvitationFromTemplateRequest({
    required this.eventId,
    required this.templateId,
    this.name,
  });

  final int eventId;
  final int templateId;
  final String? name;

  Map<String, dynamic> toJson() {
    return {
      'eventId': eventId,
      'templateId': templateId,
      'name': name,
    }..removeWhere((key, value) =>
        value == null || (value is String && value.trim().isEmpty));
  }
}
