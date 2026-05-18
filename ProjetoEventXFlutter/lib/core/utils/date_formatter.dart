class DateFormatter {
  static String formatDate(DateTime value) {
    final date = value.toLocal();
    final day = _two(date.day);
    final month = _two(date.month);
    final year = date.year;
    return '$day/$month/$year';
  }

  static String formatDateTime(DateTime value) {
    final date = value.toLocal();
    final day = _two(date.day);
    final month = _two(date.month);
    final year = date.year;
    final hour = _two(date.hour);
    final minute = _two(date.minute);
    return '$day/$month/$year $hour:$minute';
  }

  static String _two(int value) => value.toString().padLeft(2, '0');
}
