import 'dart:math' as math;

import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/shared/models/content_intelligence_models.dart';

class FeedRankingWeights {
  const FeedRankingWeights({
    this.recency = 0.34,
    this.engagement = 0.29,
    this.affinity = 0.18,
    this.eventRelevance = 0.11,
    this.contentType = 0.05,
    this.priorityBoost = 0.03,
  });

  final double recency;
  final double engagement;
  final double affinity;
  final double eventRelevance;
  final double contentType;
  final double priorityBoost;
}

class StoryRankingWeights {
  const StoryRankingWeights({
    this.unseen = 0.37,
    this.recency = 0.27,
    this.affinity = 0.17,
    this.eventRelevance = 0.11,
    this.priorityBoost = 0.08,
  });

  final double unseen;
  final double recency;
  final double affinity;
  final double eventRelevance;
  final double priorityBoost;
}

class FeedRankingService {
  const FeedRankingService({
    this.postWeights = const FeedRankingWeights(),
    this.storyWeights = const StoryRankingWeights(),
  });

  final FeedRankingWeights postWeights;
  final StoryRankingWeights storyWeights;

  List<RankedPost> rankFeedPosts(
    List<PostModel> posts,
    FeedRankingContext context, {
    FeedSortMode mode = FeedSortMode.forYou,
  }) {
    if (posts.isEmpty) {
      return const <RankedPost>[];
    }

    if (context.fallbackToChronological || mode == FeedSortMode.recent) {
      final ranked = posts
          .map(
            (post) => RankedPost(
              post: post,
              score: post.dataCriacao.millisecondsSinceEpoch.toDouble(),
              reasons: const ['Mais recente'],
            ),
          )
          .toList(growable: false);
      ranked.sort((a, b) => b.post.dataCriacao.compareTo(a.post.dataCriacao));
      return ranked;
    }

    final maxEngagement = _maxEngagement(posts);
    final now = DateTime.now();

    final ranked = posts.map((post) {
      final recency = _recencyScore(post.dataCriacao, now);
      final engagement = _engagementScore(post, maxEngagement);
      final affinity = _affinityScore(post, context);
      final eventRelevance = _eventRelevance(post, context);
      final contentType = _contentTypeScore(post, context);
      final priorityBoost = _priorityScore(post, context);

      final baseScore = (postWeights.recency * recency) +
          (postWeights.engagement * engagement) +
          (postWeights.affinity * affinity) +
          (postWeights.eventRelevance * eventRelevance) +
          (postWeights.contentType * contentType) +
          (postWeights.priorityBoost * priorityBoost);

      final score = mode == FeedSortMode.trending
          ? ((engagement * 0.62) + (recency * 0.28) + (priorityBoost * 0.10))
          : baseScore;

      return RankedPost(
        post: post,
        score: score,
        reasons: _postReasons(
          post: post,
          recency: recency,
          engagement: engagement,
          affinity: affinity,
          eventRelevance: eventRelevance,
          priorityBoost: priorityBoost,
          mode: mode,
        ),
      );
    }).toList(growable: false);

    ranked.sort((a, b) => b.score.compareTo(a.score));
    return ranked;
  }

  List<PostModel> sortPosts(
    List<PostModel> posts,
    FeedRankingContext context, {
    FeedSortMode mode = FeedSortMode.forYou,
  }) {
    return rankFeedPosts(posts, context, mode: mode)
        .map((item) => item.post)
        .toList(growable: false);
  }

  List<RankedStory> rankStories(
    List<StoryCandidate> stories,
    StoryRankingContext context,
  ) {
    if (stories.isEmpty) {
      return const <RankedStory>[];
    }

    final now = DateTime.now();

    final ranked = stories.map((story) {
      final unseen = story.viewedByCurrentUser ? 0.08 : 1.0;
      final recency = _recencyScore(story.createdAt, now);
      final affinity =
          context.authorAffinity[story.authorId] ?? story.authorAffinity;
      final eventRelevance = story.eventId != null &&
              context.relevantEventIds.contains(story.eventId)
          ? math.max(story.eventRelevance, 0.85)
          : story.eventRelevance.clamp(0, 1);
      final priorityBoost =
          (story.priorityBoost.clamp(0, 1) + (story.isLive ? 0.45 : 0))
              .clamp(0, 1);

      final score = (storyWeights.unseen * unseen) +
          (storyWeights.recency * recency) +
          (storyWeights.affinity * affinity.clamp(0, 1)) +
          (storyWeights.eventRelevance * eventRelevance) +
          (storyWeights.priorityBoost * priorityBoost);

      final reasons = <String>[
        if (!story.viewedByCurrentUser) 'Nao visto',
        if (story.isLive) 'Ao vivo',
        if (recency >= 0.75) 'Recente',
        if (affinity >= 0.65) 'Afinidade alta',
        if (eventRelevance >= 0.7) 'Evento relevante',
      ];

      return RankedStory(
        story: story,
        score: score,
        reasons: reasons.isEmpty ? const ['Ordem padrao'] : reasons,
      );
    }).toList(growable: false);

    ranked.sort((a, b) => b.score.compareTo(a.score));
    return ranked;
  }

  List<String> _postReasons({
    required PostModel post,
    required double recency,
    required double engagement,
    required double affinity,
    required double eventRelevance,
    required double priorityBoost,
    required FeedSortMode mode,
  }) {
    final reasons = <String>[
      if (mode == FeedSortMode.trending) 'Em alta',
      if (post.isPinned || priorityBoost >= 0.85) 'Prioritario',
      if (engagement >= 0.72) 'Alta interacao',
      if (recency >= 0.8) 'Publicacao recente',
      if (affinity >= 0.65) 'Autor relevante para voce',
      if (eventRelevance >= 0.7) 'Relacionado ao seu evento',
      if (post.totalVisualizacoes >= 300) 'Muito visualizado',
    ];

    if (reasons.isEmpty) {
      reasons.add('Recomendado para voce');
    }

    return reasons;
  }

  double _engagementScore(PostModel post, double maxEngagement) {
    final raw = (post.totalCurtidas * 1.0) +
        (post.totalComentarios * 1.8) +
        (post.totalCompartilhamentos * 2.3) +
        (post.totalVisualizacoes * 0.14);

    if (maxEngagement <= 0) {
      return 0;
    }

    return (raw / maxEngagement).clamp(0, 1);
  }

  double _maxEngagement(List<PostModel> posts) {
    var maxEngagement = 0.0;
    for (final post in posts) {
      final raw = (post.totalCurtidas * 1.0) +
          (post.totalComentarios * 1.8) +
          (post.totalCompartilhamentos * 2.3) +
          (post.totalVisualizacoes * 0.14);
      if (raw > maxEngagement) {
        maxEngagement = raw;
      }
    }
    return maxEngagement;
  }

  double _recencyScore(DateTime createdAt, DateTime now) {
    final ageHours = now.difference(createdAt).inMinutes / 60;
    if (ageHours <= 0) {
      return 1;
    }

    // half-life de 36h: favorece conteudo recente sem matar evergreen.
    const halfLifeHours = 36.0;
    return math.pow(0.5, ageHours / halfLifeHours).toDouble().clamp(0, 1);
  }

  double _affinityScore(PostModel post, FeedRankingContext context) {
    if (context.currentUserId != null &&
        context.currentUserId == post.autorUserId) {
      return 1;
    }
    final affinity = context.authorAffinity[post.autorUserId];
    if (affinity != null) {
      return affinity.clamp(0, 1);
    }
    if (post.usuarioCurtiu) {
      return 0.72;
    }
    return 0.35;
  }

  double _eventRelevance(PostModel post, FeedRankingContext context) {
    if (post.eventoId == null) {
      return 0.2;
    }
    if (context.preferredEventIds.contains(post.eventoId)) {
      return 1.0;
    }
    return 0.45;
  }

  double _contentTypeScore(PostModel post, FeedRankingContext context) {
    final type = post.tipoConteudo?.toLowerCase().trim();
    if (type == null || type.isEmpty) {
      return 0.35;
    }
    if (context.preferredContentTypes.contains(type)) {
      return 1.0;
    }
    return switch (type) {
      'video' => 0.75,
      'story' => 0.68,
      'foto' || 'image' => 0.64,
      _ => 0.45,
    };
  }

  double _priorityScore(PostModel post, FeedRankingContext context) {
    if (context.manualPriorityPosts.contains(post.id) || post.isPinned) {
      return 1.0;
    }
    if (post.totalCompartilhamentos > 0 && post.totalVisualizacoes > 100) {
      return 0.65;
    }
    return 0.2;
  }
}
