import 'package:projeto_eventx_flutter/shared/models/supplier_intelligence_models.dart';
import 'package:projeto_eventx_flutter/shared/services/supplier_ranking_service.dart';

class MarketplaceScoringService {
  const MarketplaceScoringService({
    SupplierRankingService rankingService = const SupplierRankingService(),
  }) : _rankingService = rankingService;

  final SupplierRankingService _rankingService;

  List<RankedSupplier> rankSuppliers(
    List<SupplierIntelligenceModel> suppliers, {
    MarketplaceFilters filters = const MarketplaceFilters(),
    MarketplaceContext context = const MarketplaceContext(),
    MarketplaceSortMode mode = MarketplaceSortMode.recommended,
  }) {
    final seeded = _rankingService.rankSuppliers(
      suppliers,
      category: filters.category,
      city: filters.city,
      minPrice: filters.minPrice,
      maxPrice: filters.maxPrice,
    );

    final decorated = seeded.map((item) {
      final filterMatch = _filterMatchScore(item.supplier, filters);
      final contextualFit = _contextFitScore(item.supplier, context);
      final featuredBoost = item.supplier.featured ? 0.08 : 0;
      final premiumBoost = item.supplier.premium ? 0.05 : 0;
      final score = item.score +
          (filterMatch * 120) +
          (contextualFit * 140) +
          (featuredBoost * 100) +
          (premiumBoost * 100);

      final reasons = <String>[
        ...item.reasons,
        if (filterMatch >= 0.75) 'Aderencia alta aos filtros',
        if (contextualFit >= 0.7) 'Boa compatibilidade com seu contexto',
      ];

      final badges = <String>[
        ...item.badges,
        if (contextualFit >= 0.74) 'Recomendado para voce',
      ];

      return RankedSupplier(
        supplier: item.supplier,
        score: score,
        badges: badges.toSet().toList(growable: false),
        reasons: reasons.toSet().toList(growable: false),
        breakdown: item.breakdown,
      );
    }).toList(growable: false);

    return _sortByMode(decorated, mode);
  }

  double calculateSupplierScore(
    SupplierIntelligenceModel supplier, {
    MarketplaceFilters filters = const MarketplaceFilters(),
    MarketplaceContext context = const MarketplaceContext(),
  }) {
    final ranked = rankSuppliers(
      <SupplierIntelligenceModel>[supplier],
      filters: filters,
      context: context,
    );
    if (ranked.isEmpty) {
      return 0;
    }
    return ranked.first.score;
  }

  double _filterMatchScore(
    SupplierIntelligenceModel supplier,
    MarketplaceFilters filters,
  ) {
    var score = 0.35;

    if (filters.category != null && filters.category!.trim().isNotEmpty) {
      if (supplier.category.toLowerCase() == filters.category!.toLowerCase()) {
        score += 0.28;
      }
    }

    if (filters.city != null && filters.city!.trim().isNotEmpty) {
      if (supplier.city.toLowerCase() == filters.city!.toLowerCase()) {
        score += 0.24;
      }
    }

    if (filters.featuredOnly) {
      score += supplier.featured ? 0.2 : -0.4;
    }

    if (filters.minPrice != null || filters.maxPrice != null) {
      final inRange = _priceRangeMatch(
        supplier,
        minPrice: filters.minPrice,
        maxPrice: filters.maxPrice,
      );
      score += inRange ? 0.2 : -0.15;
    }

    return score.clamp(0, 1);
  }

  double _contextFitScore(
    SupplierIntelligenceModel supplier,
    MarketplaceContext context,
  ) {
    var score = 0.3;

    if (context.eventCity != null &&
        context.eventCity!.trim().isNotEmpty &&
        supplier.city.toLowerCase() == context.eventCity!.toLowerCase()) {
      score += 0.24;
    }

    if (context.eventType != null &&
        context.eventType!.trim().isNotEmpty &&
        supplier.category.toLowerCase() == context.eventType!.toLowerCase()) {
      score += 0.24;
    }

    if (context.preferredCategories
        .map((value) => value.toLowerCase())
        .contains(supplier.category.toLowerCase())) {
      score += 0.2;
    }

    if (context.preferredSupplierIds.contains(supplier.id)) {
      score += 0.2;
    }

    if (context.budgetMin != null || context.budgetMax != null) {
      if (_priceRangeMatch(
        supplier,
        minPrice: context.budgetMin,
        maxPrice: context.budgetMax,
      )) {
        score += 0.12;
      }
    }

    return score.clamp(0, 1);
  }

  bool _priceRangeMatch(
    SupplierIntelligenceModel supplier, {
    double? minPrice,
    double? maxPrice,
  }) {
    if (minPrice == null && maxPrice == null) {
      return true;
    }

    final min = supplier.priceMin;
    final max = supplier.priceMax;
    if (min == null && max == null) {
      return true;
    }

    if (minPrice != null && max != null && max < minPrice) {
      return false;
    }

    if (maxPrice != null && min != null && min > maxPrice) {
      return false;
    }

    return true;
  }

  List<RankedSupplier> _sortByMode(
    List<RankedSupplier> items,
    MarketplaceSortMode mode,
  ) {
    final sorted = items.toList(growable: false);

    switch (mode) {
      case MarketplaceSortMode.recommended:
        sorted.sort((a, b) => b.score.compareTo(a.score));
        break;
      case MarketplaceSortMode.topRated:
        sorted.sort(
          (a, b) =>
              b.supplier.averageRating.compareTo(a.supplier.averageRating),
        );
        break;
      case MarketplaceSortMode.lowestPrice:
        sorted.sort((a, b) {
          final left = a.supplier.priceMin ?? double.infinity;
          final right = b.supplier.priceMin ?? double.infinity;
          return left.compareTo(right);
        });
        break;
      case MarketplaceSortMode.fastestResponse:
        sorted.sort(
          (a, b) => b.supplier.responseRate.compareTo(a.supplier.responseRate),
        );
        break;
      case MarketplaceSortMode.mostPopular:
        sorted.sort(
          (a, b) =>
              b.supplier.popularityScore.compareTo(a.supplier.popularityScore),
        );
        break;
    }

    return sorted;
  }
}
