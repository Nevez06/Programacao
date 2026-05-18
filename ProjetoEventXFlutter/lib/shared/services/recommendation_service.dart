import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/shared/models/content_intelligence_models.dart';
import 'package:projeto_eventx_flutter/shared/models/recommendation_models.dart';
import 'package:projeto_eventx_flutter/shared/models/supplier_intelligence_models.dart';
import 'package:projeto_eventx_flutter/shared/services/feed_ranking_service.dart';
import 'package:projeto_eventx_flutter/shared/services/marketplace_scoring_service.dart';

class RecommendationService {
  const RecommendationService({
    FeedRankingService feedRankingService = const FeedRankingService(),
    MarketplaceScoringService marketplaceScoringService =
        const MarketplaceScoringService(),
  })  : _feedRankingService = feedRankingService,
        _marketplaceScoringService = marketplaceScoringService;

  final FeedRankingService _feedRankingService;
  final MarketplaceScoringService _marketplaceScoringService;

  List<RecommendationItem> getRecommendations({
    required RecommendationContext context,
    List<PostModel> posts = const <PostModel>[],
    List<SupplierIntelligenceModel> suppliers =
        const <SupplierIntelligenceModel>[],
    List<StoryCandidate> stories = const <StoryCandidate>[],
  }) {
    final recommendations = <RecommendationItem>[];

    if (posts.isNotEmpty) {
      final feedContext = FeedRankingContext(
        currentUserId: context.currentUserId,
        preferredEventIds: context.relevantEventIds,
      );

      final topFeed = _feedRankingService.rankFeedPosts(
        posts,
        feedContext,
        mode: FeedSortMode.trending,
      );

      if (topFeed.isNotEmpty) {
        final first = topFeed.first;
        recommendations.add(
          RecommendationItem(
            type: RecommendationType.trendingPost,
            title: first.post.titulo?.trim().isNotEmpty == true
                ? first.post.titulo!
                : 'Post em alta no seu feed',
            subtitle: first.reasons.firstOrNull ?? 'Alto engajamento recente',
            score: first.score,
            highlightTag: 'Em alta',
            payload: {
              'postId': first.post.id.toString(),
              'author': first.post.autorNome,
            },
          ),
        );
      }
    }

    if (suppliers.isNotEmpty) {
      final rankedSuppliers = _marketplaceScoringService.rankSuppliers(
        suppliers,
        context: MarketplaceContext(
          currentUserId: context.currentUserId,
          eventCity: context.eventCity,
          eventType: context.eventType,
        ),
      );

      if (rankedSuppliers.isNotEmpty) {
        final best = rankedSuppliers.first;
        recommendations.add(
          RecommendationItem(
            type: RecommendationType.recommendedSupplier,
            title: best.supplier.name,
            subtitle: best.reasons.firstOrNull ?? 'Fornecedor recomendado',
            score: best.score,
            highlightTag: best.badges.firstOrNull ?? 'Recomendado',
            payload: {
              'supplierId': best.supplier.id.toString(),
              'category': best.supplier.category,
              'city': best.supplier.city,
            },
          ),
        );
      }
    }

    if (stories.isNotEmpty) {
      final rankedStories = _feedRankingService.rankStories(
        stories,
        StoryRankingContext(
          currentUserId: context.currentUserId,
          relevantEventIds: context.relevantEventIds,
        ),
      );

      if (rankedStories.isNotEmpty) {
        final topStory = rankedStories.first;
        recommendations.add(
          RecommendationItem(
            type: RecommendationType.relevantStory,
            title: '@${topStory.story.authorName}',
            subtitle: topStory.reasons.firstOrNull ?? 'Story recomendado',
            score: topStory.score,
            highlightTag: topStory.story.isLive ? 'Ao vivo' : 'Nao visto',
            payload: {'storyId': topStory.story.id},
          ),
        );
      }
    }

    recommendations.sort((a, b) => b.score.compareTo(a.score));
    return recommendations;
  }
}

extension on List<String> {
  String? get firstOrNull => isEmpty ? null : first;
}
