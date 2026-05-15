import 'package:projeto_eventx_flutter/core/utils/id_value_parser.dart';

class InvitationTemplateModel {
  const InvitationTemplateModel({
    required this.id,
    required this.name,
    required this.defaultSystem,
    required this.organizerId,
    this.style,
    this.backgroundColor,
    this.primaryColor,
    this.textColor,
    this.font,
    this.title,
    this.message,
    this.previewUrl,
    this.eventId,
  });

  final int id;
  final String name;
  final String? style;
  final String? backgroundColor;
  final String? primaryColor;
  final String? textColor;
  final String? font;
  final String? title;
  final String? message;
  final String? previewUrl;
  final bool defaultSystem;
  final int organizerId;
  final int? eventId;

  factory InvitationTemplateModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    return InvitationTemplateModel(
      id: IdValueParser.parseToInt(read('id')),
      name: (read('name') ?? '').toString(),
      style: _nullable(read('style')),
      backgroundColor: _nullable(read('backgroundColor')),
      primaryColor: _nullable(read('primaryColor')),
      textColor: _nullable(read('textColor')),
      font: _nullable(read('font')),
      title: _nullable(read('title')),
      message: _nullable(read('message')),
      previewUrl: _nullable(read('previewUrl')),
      defaultSystem: (read('defaultSystem') as bool?) ?? false,
      organizerId: IdValueParser.parseToInt(read('organizerId')),
      eventId: (read('eventId') as num?)?.toInt(),
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
