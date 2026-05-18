import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_eventx_flutter/features/feed/data/models/post_model.dart';
import 'package:projeto_eventx_flutter/shared/models/content_intelligence_models.dart';
import 'package:projeto_eventx_flutter/shared/models/supplier_intelligence_models.dart';
import 'package:projeto_eventx_flutter/shared/services/feed_ranking_service.dart';
import 'package:projeto_eventx_flutter/shared/services/marketplace_scoring_service.dart';
import 'package:projeto_eventx_flutter/shared/services/supplier_ranking_service.dart';

void main() {
  group('FeedRankingService', () {
    test('prioriza post recente e com engajamento', () {
      final service = const FeedRankingService();
      final posts = [
        _post(
          id: 1,
          authorId: 10,
          minutesAgo: 10,
          likes: 220,
          comments: 40,
          shares: 20,
          views: 1800,
        ),
        _post(
          id: 2,
          authorId: 11,
          minutesAgo: 1400,
          likes: 5,
          comments: 1,
          shares: 0,
          views: 60,
        ),
      ];

      final ranked = service.rankFeedPosts(
        posts,
        const FeedRankingContext(currentUserId: 999),
      );

      expect(ranked.first.post.id, 1);
      expect(ranked.first.reasons, isNotEmpty);
    });

    test('stories nao vistos vem primeiro', () {
      final service = const FeedRankingService();
      final stories = [
        StoryCandidate(
          id: '1',
          authorId: 1,
          authorName: 'a',
          createdAt: DateTime.now().subtract(const Duration(hours: 1)),
          viewedByCurrentUser: true,
        ),
        StoryCandidate(
          id: '2',
          authorId: 2,
          authorName: 'b',
          createdAt: DateTime.now().subtract(const Duration(hours: 2)),
          viewedByCurrentUser: false,
        ),
      ];

      final ranked = service.rankStories(stories, const StoryRankingContext());
      expect(ranked.first.story.id, '2');
    });
  });

  group('Supplier scoring', () {
    test('score composto favorece fornecedor mais confiavel', () {
      final rankingService = const SupplierRankingService();
      final marketplaceService = MarketplaceScoringService(
        rankingService: rankingService,
      );

      final suppliers = [
        _supplier(
          id: 1,
          rating: 4.9,
          response: 0.92,
          cancellation: 0.02,
          punctuality: 0.9,
          hires: 180,
        ),
        _supplier(
          id: 2,
          rating: 4.2,
          response: 0.5,
          cancellation: 0.18,
          punctuality: 0.55,
          hires: 25,
        ),
      ];

      final ranked = marketplaceService.rankSuppliers(
        suppliers,
        mode: MarketplaceSortMode.recommended,
      );

      expect(ranked.first.supplier.id, 1);
      expect(ranked.first.badges, isNotEmpty);
    });
  });
}

PostModel _post({
  required int id,
  required int authorId,
  required int minutesAgo,
  required int likes,
  required int comments,
  required int shares,
  required int views,
}) {
  final createdAt = DateTime.now().subtract(Duration(minutes: minutesAgo));
  return PostModel(
    id: id,
    autorUserId: authorId,
    autorNome: 'Autor $authorId',
    conteudo: 'Post $id',
    dataCriacao: createdAt,
    dataAtualizacao: createdAt,
    totalCurtidas: likes,
    totalComentarios: comments,
    totalCompartilhamentos: shares,
    totalVisualizacoes: views,
    usuarioCurtiu: false,
    isOwner: false,
    commentsEnabled: true,
    isPinned: false,
    isArchived: false,
    hideLikesCount: false,
    hideSharesCount: false,
    comentarios: const [],
  );
}

SupplierIntelligenceModel _supplier({
  required int id,
  required double rating,
  required double response,
  required double cancellation,
  required double punctuality,
  required int hires,
}) {
  return SupplierIntelligenceModel(
    id: id,
    name: 'Supplier $id',
    category: 'Decoracao',
    city: 'Sao Paulo',
    startingPriceLabel: 'R\$ 1000',
    averageRating: rating,
    reviewCount: hires,
    acceptanceRate: 0.9,
    responseRate: response,
    cancellationRate: cancellation,
    punctualityScore: punctuality,
    featured: true,
    rankingPosition: id,
    recentPerformanceScore: 0.82,
    popularityScore: 0.8,
    totalHires: hires,
    priceMin: 1000,
    priceMax: 8000,
    premium: id == 1,
  );
}
