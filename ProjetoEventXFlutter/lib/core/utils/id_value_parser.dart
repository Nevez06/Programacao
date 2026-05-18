class IdValueParser {
  const IdValueParser._();

  static int parseToInt(dynamic value, {int fallback = 0}) {
    if (value == null) {
      return fallback;
    }

    if (value is int) {
      return value;
    }

    if (value is num) {
      return value.toInt();
    }

    final text = value.toString().trim();
    if (text.isEmpty) {
      return fallback;
    }

    final numeric = int.tryParse(text);
    if (numeric != null) {
      return numeric;
    }

    // Stable fallback for non-numeric ids (for example GUIDs).
    return _fnv1a32(text);
  }

  static int? parseToNullableInt(dynamic value) {
    if (value == null) {
      return null;
    }

    final parsed = parseToInt(value, fallback: -1);
    if (parsed < 0) {
      return null;
    }
    return parsed;
  }

  static int _fnv1a32(String input) {
    const int prime = 0x01000193;
    int hash = 0x811C9DC5;

    for (final unit in input.codeUnits) {
      hash ^= unit;
      hash = (hash * prime) & 0xFFFFFFFF;
    }

    return hash & 0x7FFFFFFF;
  }
}
