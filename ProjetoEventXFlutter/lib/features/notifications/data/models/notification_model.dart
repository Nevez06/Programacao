class NotificationModel {
  const NotificationModel({
    required this.id,
    required this.title,
    required this.message,
    required this.type,
    required this.isRead,
    required this.createdAt,
    this.link,
    required this.timeAgo,
  });

  final int id;
  final String title;
  final String message;
  final String type;
  final bool isRead;
  final DateTime createdAt;
  final String? link;
  final String timeAgo;

  factory NotificationModel.fromJson(Map<String, dynamic> json) {
    return NotificationModel(
      id: _readInt(json, 'id'),
      title: _readString(json, 'title'),
      message: _readString(json, 'message'),
      type: _readString(json, 'type'),
      isRead: _readBool(json, 'isRead'),
      createdAt: _readDateTime(json, 'createdAt'),
      link: _readNullableString(json, 'link'),
      timeAgo: _readString(json, 'timeAgo'),
    );
  }

  NotificationModel copyWith({
    bool? isRead,
  }) {
    return NotificationModel(
      id: id,
      title: title,
      message: message,
      type: type,
      isRead: isRead ?? this.isRead,
      createdAt: createdAt,
      link: link,
      timeAgo: timeAgo,
    );
  }

  static int _readInt(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[_toPascalCase(key)];
    return (value as num?)?.toInt() ?? 0;
  }

  static String _readString(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[_toPascalCase(key)];
    return (value ?? '').toString();
  }

  static String? _readNullableString(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[_toPascalCase(key)];
    final text = value?.toString();
    if (text == null || text.isEmpty) {
      return null;
    }
    return text;
  }

  static bool _readBool(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[_toPascalCase(key)];
    if (value is bool) {
      return value;
    }
    if (value is String) {
      if (value.toLowerCase() == 'true') {
        return true;
      }
      if (value.toLowerCase() == 'false') {
        return false;
      }
    }
    return false;
  }

  static DateTime _readDateTime(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[_toPascalCase(key)];
    return DateTime.tryParse((value ?? '').toString()) ??
        DateTime.fromMillisecondsSinceEpoch(0);
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
