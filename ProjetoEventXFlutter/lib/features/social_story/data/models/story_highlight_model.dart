import 'package:projeto_eventx_flutter/features/social_story/data/models/social_story_model.dart';

class StoryHighlightModel {
  const StoryHighlightModel({
    required this.id,
    required this.nome,
    required this.totalStories,
    required this.createdAt,
    required this.stories,
    this.capaUrl,
  });

  final int id;
  final String nome;
  final String? capaUrl;
  final int totalStories;
  final DateTime createdAt;
  final List<SocialStoryModel> stories;

  factory StoryHighlightModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    final storiesRaw = (read('stories') as List<dynamic>? ?? const <dynamic>[]);

    return StoryHighlightModel(
      id: (read('id') as num?)?.toInt() ?? 0,
      nome: (read('nome') ?? read('name') ?? '').toString(),
      capaUrl: _nullable(read('capaUrl')) ?? _nullable(read('coverUrl')),
      totalStories: (read('totalStories') as num?)?.toInt() ?? 0,
      createdAt: DateTime.tryParse(
              (read('createdAt') ?? read('createdAtUtc') ?? '').toString()) ??
          DateTime.now(),
      stories: storiesRaw
          .whereType<Map<String, dynamic>>()
          .map(SocialStoryModel.fromJson)
          .toList(growable: false),
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

class CreateStoryHighlightRequest {
  const CreateStoryHighlightRequest({
    required this.nome,
    required this.storyIds,
    this.capaUrl,
  });

  final String nome;
  final String? capaUrl;
  final List<int> storyIds;

  Map<String, dynamic> toJson() {
    return {
      'name': nome,
      'coverUrl': capaUrl,
      'nome': nome,
      'capaUrl': capaUrl,
      'storyIds': storyIds,
    }..removeWhere((key, value) => value == null);
  }
}
