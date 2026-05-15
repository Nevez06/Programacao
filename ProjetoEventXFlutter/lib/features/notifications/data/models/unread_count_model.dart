class UnreadCountModel {
  const UnreadCountModel({
    required this.count,
  });

  final int count;

  factory UnreadCountModel.fromJson(Map<String, dynamic> json) {
    final value = json['count'] ?? json['Count'];
    return UnreadCountModel(
      count: (value as num?)?.toInt() ?? 0,
    );
  }
}
