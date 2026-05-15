import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';

enum FeedSortMode {
  forYou,
  recent,
  trending,
}

class FeedRankingContext {
  const FeedRankingContext({
    this.currentUserId,
    this.preferredEventIds = const <int>{},
    this.preferredContentTypes = const <String>{},
    this.authorAffinity = const <int, double>{},
    this.manualPriorityPosts = const <int>{},
    this.fallbackToChronological = false,
  });

  final int? currentUserId;
  final Set<int> preferredEventIds;
  final Set<String> preferredContentTypes;
  final Map<int, double> authorAffinity;
  final Set<int> manualPriorityPosts;
  final bool fallbackToChronological;
}

class RankedPost {
  const RankedPost({
    required this.post,
    required this.score,
    required this.reasons,
  });

  final PostModel post;
  final double score;
  final List<String> reasons;
}

class StoryCandidate {
  const StoryCandidate({
    required this.id,
    required this.authorId,
    required this.authorName,
    this.imageUrl,
    required this.createdAt,
    required this.viewedByCurrentUser,
    this.authorAffinity = 0,
    this.eventRelevance = 0,
    this.priorityBoost = 0,
    this.isLive = false,
    this.eventId,
  });

  final String id;
  final int authorId;
  final String authorName;
  final String? imageUrl;
  final DateTime createdAt;
  final bool viewedByCurrentUser;
  final double authorAffinity;
  final double eventRelevance;
  final double priorityBoost;
  final bool isLive;
  final int? eventId;
}

class StoryRankingContext {
  const StoryRankingContext({
    this.currentUserId,
    this.relevantEventIds = const <int>{},
    this.authorAffinity = const <int, double>{},
  });

  final int? currentUserId;
  final Set<int> relevantEventIds;
  final Map<int, double> authorAffinity;
}

class RankedStory {
  const RankedStory({
    required this.story,
    required this.score,
    required this.reasons,
  });

  final StoryCandidate story;
  final double score;
  final List<String> reasons;
}
