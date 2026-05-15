import 'dart:math' as math;

import 'package:projeto_eventx_flutter/shared/models/supplier_intelligence_models.dart';

class SupplierRankingWeights {
  const SupplierRankingWeights({
    this.ratingQuality = 0.28,
    this.reviewConfidence = 0.12,
    this.acceptance = 0.14,
    this.punctuality = 0.14,
    this.response = 0.12,
    this.cancellation = 0.10,
    this.popularity = 0.06,
    this.recentPerformance = 0.04,
  });

  final double ratingQuality;
  final double reviewConfidence;
  final double acceptance;
  final double punctuality;
  final double response;
  final double cancellation;
  final double popularity;
  final double recentPerformance;
}

class SupplierRankingService {
  const SupplierRankingService({
    this.weights = const SupplierRankingWeights(),
  });

  final SupplierRankingWeights weights;

  SupplierScoreBreakdown calculateSupplierScore(
    SupplierIntelligenceModel supplier,
  ) {
    final ratingQuality =
        (supplier.averageRating / 5).clamp(0.0, 1.0).toDouble();
    final reviewConfidence = _reviewConfidence(supplier.reviewCount);
    final acceptance = supplier.acceptanceRate.clamp(0.0, 1.0).toDouble();
    final punctuality = supplier.punctualityScore.clamp(0.0, 1.0).toDouble();
    final response = supplier.responseRate.clamp(0.0, 1.0).toDouble();
    final cancellation =
        (1 - supplier.cancellationRate).clamp(0.0, 1.0).toDouble();
    final popularity = _popularityScore(supplier);
    final recentPerformance =
        supplier.recentPerformanceScore.clamp(0.0, 1.0).toDouble();

    final total = ((weights.ratingQuality * ratingQuality) +
            (weights.reviewConfidence * reviewConfidence) +
            (weights.acceptance * acceptance) +
            (weights.punctuality * punctuality) +
            (weights.response * response) +
            (weights.cancellation * cancellation) +
            (weights.popularity * popularity) +
            (weights.recentPerformance * recentPerformance)) *
        1000;

    return SupplierScoreBreakdown(
      total: total.clamp(0.0, 1000.0).toDouble(),
      ratingQuality: ratingQuality,
      reviewConfidence: reviewConfidence,
      acceptance: acceptance,
      punctuality: punctuality,
      response: response,
      cancellation: cancellation,
      popularity: popularity,
      recentPerformance: recentPerformance,
    );
  }

  List<RankedSupplier> rankSuppliers(
    List<SupplierIntelligenceModel> suppliers, {
    String? category,
    String? city,
    double? minPrice,
    double? maxPrice,
  }) {
    final filtered = suppliers.where((supplier) {
      final categoryOk = category == null ||
          category.trim().isEmpty ||
          supplier.category.toLowerCase() == category.toLowerCase();
      final cityOk = city == null ||
          city.trim().isEmpty ||
          supplier.city.toLowerCase() == city.toLowerCase();
      final minOk = minPrice == null ||
          supplier.priceMax == null ||
          supplier.priceMax! >= minPrice;
      final maxOk = maxPrice == null ||
          supplier.priceMin == null ||
          supplier.priceMin! <= maxPrice;
      return categoryOk && cityOk && minOk && maxOk;
    }).toList(growable: false);

    final ranked = filtered.map((supplier) {
      final breakdown = calculateSupplierScore(supplier);
      final badges = _buildBadges(supplier, breakdown);
      final reasons = _buildReasons(supplier, breakdown);

      return RankedSupplier(
        supplier: supplier,
        score: breakdown.total,
        badges: badges,
        reasons: reasons,
        breakdown: breakdown,
      );
    }).toList(growable: false);

    ranked.sort((a, b) => b.score.compareTo(a.score));
    return ranked;
  }

  double _reviewConfidence(int reviewCount) {
    if (reviewCount <= 0) {
      return 0.08;
    }
    // Curva suavizada para não privilegiar apenas volume bruto.
    final confidence = math.log(reviewCount + 1) / math.log(201);
    return confidence.clamp(0.08, 1.0).toDouble();
  }

  double _popularityScore(SupplierIntelligenceModel supplier) {
    final hiresSignal = supplier.totalHires <= 0
        ? 0.1
        : (math.log(supplier.totalHires + 1) / math.log(251))
            .clamp(0.0, 1.0)
            .toDouble();
    final raw = (supplier.popularityScore * 0.6) + (hiresSignal * 0.4);
    return raw.clamp(0.0, 1.0).toDouble();
  }

  List<String> _buildBadges(
    SupplierIntelligenceModel supplier,
    SupplierScoreBreakdown breakdown,
  ) {
    final badges = <String>[
      if (supplier.featured || supplier.premium) 'Top fornecedor',
      if (breakdown.response >= 0.8) 'Resposta rapida',
      if (breakdown.ratingQuality >= 0.9 && supplier.reviewCount >= 10)
        'Melhor avaliado',
      if (breakdown.popularity >= 0.75) 'Mais contratado',
      if (breakdown.recentPerformance >= 0.78) 'Em alta',
    ];

    if (badges.isEmpty) {
      badges.add('Recomendado para voce');
    }
    return badges;
  }

  List<String> _buildReasons(
    SupplierIntelligenceModel supplier,
    SupplierScoreBreakdown breakdown,
  ) {
    final reasons = <String>[
      if (breakdown.ratingQuality >= 0.85)
        'Nota media ${supplier.averageRating.toStringAsFixed(1)}',
      if (breakdown.reviewConfidence >= 0.6)
        '${supplier.reviewCount} avaliacoes qualificadas',
      if (breakdown.acceptance >= 0.75)
        'Alta taxa de aceitacao (${(supplier.acceptanceRate * 100).round()}%)',
      if (breakdown.punctuality >= 0.75) 'Historico de pontualidade forte',
      if (breakdown.response >= 0.8) 'Tempo de resposta acima da media',
      if (breakdown.cancellation >= 0.8)
        'Baixa taxa de cancelamento (${(supplier.cancellationRate * 100).round()}%)',
    ];

    if (reasons.isEmpty) {
      reasons.add('Boa aderencia para o contexto atual');
    }
    return reasons;
  }
}
