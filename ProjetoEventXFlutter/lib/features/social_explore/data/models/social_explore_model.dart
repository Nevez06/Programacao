import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/models/social_profile_model.dart';

class SocialExploreModel {
  const SocialExploreModel({
    required this.categories,
    required this.trendingPosts,
    required this.trendingProfiles,
    required this.recommendedPosts,
  });

  final List<String> categories;
  final List<PostModel> trendingPosts;
  final List<SocialProfileModel> trendingProfiles;
  final List<PostModel> recommendedPosts;

  factory SocialExploreModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    final categoriesRaw =
        (read('categories') as List<dynamic>? ?? const <dynamic>[]);
    final trendingPostsRaw =
        (read('trendingPosts') as List<dynamic>? ?? const <dynamic>[]);
    final trendingProfilesRaw =
        (read('trendingProfiles') as List<dynamic>? ?? const <dynamic>[]);
    final recommendedPostsRaw =
        (read('recommendedPosts') as List<dynamic>? ?? const <dynamic>[]);

    final trendingPosts = trendingPostsRaw
        .whereType<Map<String, dynamic>>()
        .map(PostModel.fromJson)
        .toList(growable: false);
    final categories = categoriesRaw
        .map((item) => item.toString())
        .where((item) => item.trim().isNotEmpty)
        .toList(growable: false);

    final derivedCategories = categories.isNotEmpty
        ? categories
        : trendingPosts
            .map((post) => post.categoria?.trim() ?? '')
            .where((category) => category.isNotEmpty)
            .toSet()
            .toList(growable: false);

    return SocialExploreModel(
      categories: derivedCategories,
      trendingPosts: trendingPosts,
      trendingProfiles: trendingProfilesRaw
          .whereType<Map<String, dynamic>>()
          .map(SocialProfileModel.fromJson)
          .toList(growable: false),
      recommendedPosts: recommendedPostsRaw.isNotEmpty
          ? recommendedPostsRaw
              .whereType<Map<String, dynamic>>()
              .map(PostModel.fromJson)
              .toList(growable: false)
          : trendingPosts,
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
