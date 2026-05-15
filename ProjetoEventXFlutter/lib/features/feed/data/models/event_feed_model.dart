class EventFeedModel {
  const EventFeedModel({
    required this.id,
    required this.name,
    required this.type,
    required this.description,
    required this.startDate,
    required this.location,
    required this.coverImageUrl,
    required this.status,
  });

  final int id;
  final String name;
  final String type;
  final String description;
  final DateTime? startDate;
  final String location;
  final String? coverImageUrl;
  final String status;

  factory EventFeedModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_pascalCase(key)];

    return EventFeedModel(
      id: (read('id') as num?)?.toInt() ??
          int.tryParse(read('id')?.toString() ?? '') ??
          0,
      name: (read('name') ?? '').toString(),
      type: (read('type') ?? '').toString(),
      description: (read('description') ?? '').toString(),
      startDate: DateTime.tryParse((read('startDate') ?? '').toString()),
      location: (read('location') ?? '').toString(),
      coverImageUrl: read('coverImageUrl')?.toString(),
      status: (read('status') ?? '').toString(),
    );
  }

  static String _pascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
