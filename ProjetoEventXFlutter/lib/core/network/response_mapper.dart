class ResponseMapper {
  ResponseMapper._();

  static Map<String, dynamic> ensureMap(dynamic data) {
    if (data is Map<String, dynamic>) {
      return data;
    }
    return <String, dynamic>{};
  }

  static List<Map<String, dynamic>> ensureMapList(dynamic data) {
    if (data is List) {
      return data.whereType<Map<String, dynamic>>().toList(growable: false);
    }
    return const <Map<String, dynamic>>[];
  }

  static String readString(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[_pascalCase(key)];
    return (value ?? '').toString();
  }

  static int readInt(Map<String, dynamic> json, String key,
      {int fallback = 0}) {
    final value = json[key] ?? json[_pascalCase(key)];
    if (value is int) {
      return value;
    }
    if (value is num) {
      return value.toInt();
    }
    return int.tryParse((value ?? '').toString()) ?? fallback;
  }

  static bool readBool(Map<String, dynamic> json, String key,
      {bool fallback = false}) {
    final value = json[key] ?? json[_pascalCase(key)];
    if (value is bool) {
      return value;
    }
    final text = (value ?? '').toString().toLowerCase();
    if (text == 'true') {
      return true;
    }
    if (text == 'false') {
      return false;
    }
    return fallback;
  }

  static DateTime readDateTime(Map<String, dynamic> json, String key) {
    final value = json[key] ?? json[_pascalCase(key)];
    return DateTime.tryParse((value ?? '').toString()) ??
        DateTime.fromMillisecondsSinceEpoch(0);
  }

  static String _pascalCase(String key) {
    if (key.isEmpty) {
      return key;
    }
    return key[0].toUpperCase() + key.substring(1);
  }
}
