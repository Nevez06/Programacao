enum RecommendationType {
  recommendedSupplier,
  topRatedSupplier,
  fastResponderSupplier,
  trendingPost,
  relevantPost,
  relevantStory,
}

class RecommendationContext {
  const RecommendationContext({
    this.currentUserId,
    this.eventCity,
    this.eventType,
    this.relevantEventIds = const <int>{},
  });

  final int? currentUserId;
  final String? eventCity;
  final String? eventType;
  final Set<int> relevantEventIds;
}

class RecommendationItem {
  const RecommendationItem({
    required this.type,
    required this.title,
    required this.subtitle,
    required this.score,
    this.highlightTag,
    this.payload = const <String, String>{},
  });

  final RecommendationType type;
  final String title;
  final String subtitle;
  final double score;
  final String? highlightTag;
  final Map<String, String> payload;
}
