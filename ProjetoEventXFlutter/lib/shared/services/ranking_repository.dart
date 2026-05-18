import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';

class RankingEntry {
  const RankingEntry({
    required this.id,
    required this.supplierId,
    required this.position,
    required this.name,
    required this.avatarUrl,
    required this.rating,
    required this.score,
    required this.delta,
    required this.category,
    required this.city,
    required this.averageRating,
    required this.reviewCount,
    required this.completedOrders,
    required this.acceptanceRate,
    required this.responseRate,
    required this.cancellationRate,
    required this.punctualityScore,
    required this.popularityScore,
    required this.recentPerformanceScore,
  });

  final int id;
  final int supplierId;
  final int position;
  final String name;
  final String? avatarUrl;
  final double rating;
  final int score;
  final String delta;
  final String category;
  final String city;
  final double averageRating;
  final int reviewCount;
  final int completedOrders;
  final double acceptanceRate;
  final double responseRate;
  final double cancellationRate;
  final double punctualityScore;
  final double popularityScore;
  final double recentPerformanceScore;

  factory RankingEntry.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_pascalCase(key)];

    return RankingEntry(
      id: (read('id') as num?)?.toInt() ??
          (read('supplierId') as num?)?.toInt() ??
          0,
      supplierId: (read('supplierId') as num?)?.toInt() ??
          (read('fornecedorId') as num?)?.toInt() ??
          0,
      position: (read('position') as num?)?.toInt() ??
          (read('posicao') as num?)?.toInt() ??
          0,
      name: (read('name') ?? read('nome') ?? '').toString(),
      avatarUrl: (read('avatarUrl') ?? read('imageUrl') ?? read('fotoUrl'))
          ?.toString(),
      rating: _readDouble(read('rating') ?? read('averageRating') ?? read('notaMedia')),
      score: (read('score') as num?)?.toInt() ??
          (read('pontuacao') as num?)?.toInt() ??
          0,
      delta: (read('delta') ?? read('variacao') ?? '').toString(),
      category: (read('category') ?? read('categoria') ?? '').toString(),
      city: (read('city') ?? read('cidade') ?? '').toString(),
      averageRating: _readDouble(
        read('averageRating') ?? read('rating') ?? read('notaMedia'),
      ),
      reviewCount: (read('reviewCount') as num?)?.toInt() ??
          (read('avaliacoes') as num?)?.toInt() ??
          0,
      completedOrders: (read('completedOrders') as num?)?.toInt() ??
          (read('pedidosConcluidos') as num?)?.toInt() ??
          0,
      acceptanceRate: _readPercent(
        read('acceptanceRate') ?? read('taxaAceitacao'),
      ),
      responseRate: _readPercent(
        read('responseRate') ?? read('taxaResposta'),
      ),
      cancellationRate: _readPercent(
        read('cancellationRate') ?? read('taxaCancelamento'),
      ),
      punctualityScore: _readScore(
        read('punctualityScore') ?? read('pontualidade'),
      ),
      popularityScore: _readScore(
        read('popularityScore') ?? read('popularidade'),
      ),
      recentPerformanceScore: _readScore(
        read('recentPerformanceScore') ?? read('performanceRecente'),
      ),
    );
  }

  static String _pascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }

  static double _readDouble(dynamic value) {
    if (value is num) {
      return value.toDouble();
    }
    return double.tryParse(value?.toString() ?? '') ?? 0;
  }

  static double _readPercent(dynamic value) {
    final raw = _readDouble(value);
    if (raw <= 1) {
      return raw.clamp(0, 1);
    }
    return (raw / 100).clamp(0, 1);
  }

  static double _readScore(dynamic value) {
    final raw = _readDouble(value);
    if (raw <= 1) {
      return raw.clamp(0, 1);
    }
    if (raw <= 10) {
      return (raw / 10).clamp(0, 1);
    }
    return (raw / 100).clamp(0, 1);
  }
}

class RankingRepository {
  RankingRepository({required ApiClient apiClient}) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<RankingEntry>> getRanking({
    String? category,
    String? city,
    String? state,
    int take = 120,
  }) async {
    final list = await _apiClient.getList(
      '/api/ranking/suppliers',
      queryParameters: {
        if (category != null && category.trim().isNotEmpty)
          'category': category,
        if (city != null && city.trim().isNotEmpty) 'city': city,
        if (state != null && state.trim().isNotEmpty) 'state': state,
        'take': take,
      },
    );
    return list
        .whereType<Map<String, dynamic>>()
        .map(RankingEntry.fromJson)
        .toList(growable: false);
  }

  Future<List<RankingEntry>> getTopSuppliers({int take = 10}) async {
    try {
      final list = await _apiClient.getList(
        '/api/ranking/suppliers/top',
        queryParameters: {'take': take},
      );
      return list
          .whereType<Map<String, dynamic>>()
          .map(RankingEntry.fromJson)
          .toList(growable: false);
    } on ApiException catch (error) {
      if (error.isNotFound) {
        return const <RankingEntry>[];
      }
      rethrow;
    }
  }

  Future<RankingEntry> getSupplierRankingById(int supplierId) async {
    final json = await _apiClient.getJson('/api/ranking/suppliers/$supplierId');
    return RankingEntry.fromJson(json);
  }
}
