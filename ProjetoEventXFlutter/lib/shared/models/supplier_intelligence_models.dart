import 'package:projeto_eventx_flutter/shared/services/marketplace_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/ranking_repository.dart';

enum MarketplaceSortMode {
  recommended,
  topRated,
  lowestPrice,
  fastestResponse,
  mostPopular,
}

class MarketplaceFilters {
  const MarketplaceFilters({
    this.category,
    this.city,
    this.minPrice,
    this.maxPrice,
    this.featuredOnly = false,
  });

  final String? category;
  final String? city;
  final double? minPrice;
  final double? maxPrice;
  final bool featuredOnly;
}

class MarketplaceContext {
  const MarketplaceContext({
    this.currentUserId,
    this.eventType,
    this.eventCity,
    this.preferredCategories = const <String>{},
    this.preferredSupplierIds = const <int>{},
    this.budgetMin,
    this.budgetMax,
  });

  final int? currentUserId;
  final String? eventType;
  final String? eventCity;
  final Set<String> preferredCategories;
  final Set<int> preferredSupplierIds;
  final double? budgetMin;
  final double? budgetMax;
}

class SupplierIntelligenceModel {
  const SupplierIntelligenceModel({
    required this.id,
    required this.name,
    required this.category,
    required this.city,
    required this.startingPriceLabel,
    required this.averageRating,
    required this.reviewCount,
    required this.acceptanceRate,
    required this.responseRate,
    required this.cancellationRate,
    required this.punctualityScore,
    required this.featured,
    required this.rankingPosition,
    required this.recentPerformanceScore,
    required this.popularityScore,
    required this.totalHires,
    this.priceMin,
    this.priceMax,
    this.premium = false,
  });

  final int id;
  final String name;
  final String category;
  final String city;
  final String startingPriceLabel;
  final double averageRating;
  final int reviewCount;
  final double acceptanceRate;
  final double responseRate;
  final double cancellationRate;
  final double punctualityScore;
  final bool featured;
  final int? rankingPosition;
  final double recentPerformanceScore;
  final double popularityScore;
  final int totalHires;
  final double? priceMin;
  final double? priceMax;
  final bool premium;

  factory SupplierIntelligenceModel.fromMarketplace(
    MarketplaceSupplierSummary summary,
  ) {
    return SupplierIntelligenceModel(
      id: summary.id,
      name: summary.name,
      category: summary.category,
      city: summary.city,
      startingPriceLabel: summary.startingPrice,
      averageRating: summary.rating,
      reviewCount: summary.reviewCount,
      acceptanceRate: summary.acceptanceRate,
      responseRate: summary.responseRate,
      cancellationRate: summary.cancellationRate,
      punctualityScore: summary.punctualityScore,
      featured: summary.featured,
      rankingPosition: summary.rankingPosition,
      recentPerformanceScore: summary.recentPerformanceScore,
      popularityScore: summary.popularityScore,
      totalHires: summary.totalHires,
      priceMin: summary.priceMin,
      priceMax: summary.priceMax,
      premium: summary.premium,
    );
  }

  factory SupplierIntelligenceModel.fromRanking(RankingEntry entry) {
    return SupplierIntelligenceModel(
      id: entry.supplierId <= 0 ? entry.position : entry.supplierId,
      name: entry.name,
      category: entry.category,
      city: entry.city,
      startingPriceLabel: '',
      averageRating: entry.averageRating,
      reviewCount: entry.reviewCount,
      acceptanceRate: entry.acceptanceRate,
      responseRate: entry.responseRate,
      cancellationRate: entry.cancellationRate,
      punctualityScore: entry.punctualityScore,
      featured: entry.position <= 3,
      rankingPosition: entry.position,
      recentPerformanceScore: entry.recentPerformanceScore,
      popularityScore: entry.popularityScore,
      totalHires: 0,
    );
  }
}

class SupplierScoreBreakdown {
  const SupplierScoreBreakdown({
    required this.total,
    required this.ratingQuality,
    required this.reviewConfidence,
    required this.acceptance,
    required this.punctuality,
    required this.response,
    required this.cancellation,
    required this.popularity,
    required this.recentPerformance,
  });

  final double total;
  final double ratingQuality;
  final double reviewConfidence;
  final double acceptance;
  final double punctuality;
  final double response;
  final double cancellation;
  final double popularity;
  final double recentPerformance;
}

class RankedSupplier {
  const RankedSupplier({
    required this.supplier,
    required this.score,
    required this.badges,
    required this.reasons,
    required this.breakdown,
  });

  final SupplierIntelligenceModel supplier;
  final double score;
  final List<String> badges;
  final List<String> reasons;
  final SupplierScoreBreakdown breakdown;
}
