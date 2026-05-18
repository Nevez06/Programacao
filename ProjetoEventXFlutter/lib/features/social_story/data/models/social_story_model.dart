import 'package:projeto_eventx_flutter/core/utils/id_value_parser.dart';

class SocialStoryModel {
  const SocialStoryModel({
    required this.id,
    required this.userId,
    required this.authorName,
    required this.mediaUrl,
    required this.createdAt,
    required this.expireAt,
    required this.viewedByCurrentUser,
    required this.viewsCount,
    this.perfilSocialId,
    this.authorAvatar,
    this.caption,
    this.theme,
    this.eventId,
    this.eventName,
    this.sharedPostId,
  });

  final int id;
  final int userId;
  final int? perfilSocialId;
  final String authorName;
  final String? authorAvatar;
  final String mediaUrl;
  final String? caption;
  final String? theme;
  final int? eventId;
  final String? eventName;
  final int? sharedPostId;
  final DateTime createdAt;
  final DateTime expireAt;
  final bool viewedByCurrentUser;
  final int viewsCount;

  factory SocialStoryModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    final createdAt = DateTime.tryParse(
      (read('createdAt') ?? read('createdAtUtc') ?? '').toString(),
    );
    final expiresAt = DateTime.tryParse(
      (read('expireAt') ?? read('expiresAtUtc') ?? '').toString(),
    );

    return SocialStoryModel(
      id: IdValueParser.parseToInt(read('id')),
      userId: IdValueParser.parseToInt(
        read('userId') ?? read('authorId'),
      ),
      perfilSocialId: IdValueParser.parseToNullableInt(
        read('perfilSocialId') ?? read('authorProfileId'),
      ),
      authorName: _firstNonEmpty([
        read('authorName'),
        read('authorUserName'),
        read('userName'),
      ]),
      authorAvatar:
          _nullable(read('authorAvatar')) ?? _nullable(read('authorAvatarUrl')),
      mediaUrl: (read('mediaUrl') ?? read('imageUrl') ?? '').toString(),
      caption: _nullable(read('caption')),
      theme: _nullable(read('theme')),
      eventId: IdValueParser.parseToNullableInt(read('eventId')),
      eventName: _nullable(read('eventName')),
      sharedPostId: IdValueParser.parseToNullableInt(read('sharedPostId')),
      createdAt: createdAt ?? DateTime.now(),
      expireAt: expiresAt ?? DateTime.now().add(const Duration(hours: 24)),
      viewedByCurrentUser: (read('viewedByCurrentUser') as bool?) ??
          (read('viewed') as bool?) ??
          false,
      viewsCount: (read('viewsCount') as num?)?.toInt() ?? 0,
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

  static String _firstNonEmpty(List<dynamic> values) {
    for (final value in values) {
      final text = value?.toString().trim();
      if (text != null && text.isNotEmpty) {
        return text;
      }
    }
    return 'Story';
  }
}

class CreateSocialStoryRequest {
  const CreateSocialStoryRequest({
    required this.mediaUrl,
    this.caption,
    this.theme,
    this.eventId,
    this.sharedPostId,
  });

  final String mediaUrl;
  final String? caption;
  final String? theme;
  final int? eventId;
  final int? sharedPostId;

  Map<String, dynamic> toJson() {
    return {
      'imageUrl': mediaUrl,
      'mediaUrl': mediaUrl,
      'caption': caption,
      'theme': theme,
      'eventId': eventId,
      'sharedPostId': sharedPostId,
    }..removeWhere((key, value) =>
        value == null || (value is String && value.trim().isEmpty));
  }
}
