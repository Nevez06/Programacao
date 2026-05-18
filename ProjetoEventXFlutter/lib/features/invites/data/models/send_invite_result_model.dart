class SendInviteResultModel {
  const SendInviteResultModel({
    required this.sent,
    required this.message,
  });

  final bool sent;
  final String message;

  factory SendInviteResultModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];

    return SendInviteResultModel(
      sent: (read('sent') as bool?) ?? false,
      message: (read('message') ?? '').toString(),
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
