import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';

class MarketplaceSupplierSummary {
  const MarketplaceSupplierSummary({
    required this.id,
    required this.name,
    required this.category,
    required this.city,
    required this.state,
    required this.description,
    required this.rating,
    required this.startingPrice,
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
    required this.priceMin,
    required this.priceMax,
    required this.premium,
    required this.imageUrl,
    required this.badges,
  });

  final int id;
  final String name;
  final String category;
  final String city;
  final String state;
  final String description;
  final double rating;
  final String startingPrice;

  // Intelligence signals
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
  final String? imageUrl;
  final List<String> badges;

  factory MarketplaceSupplierSummary.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_pascalCase(key)];

    final rawPriceMin = _readDouble(
      read('priceMin') ??
          read('precoMinimo') ??
          read('minPrice') ??
          read('valorMinimo'),
    );
    final rawPriceMax = _readDouble(
      read('priceMax') ??
          read('precoMaximo') ??
          read('maxPrice') ??
          read('valorMaximo'),
    );
    final initialPriceText =
        (read('startingPrice') ?? read('precoInicial') ?? '').toString();

    return MarketplaceSupplierSummary(
      id: (read('id') as num?)?.toInt() ?? 0,
      name: (read('name') ?? read('nome') ?? '').toString(),
      category: (read('category') ?? read('categoria') ?? '').toString(),
      city: (read('city') ?? read('cidade') ?? '').toString(),
      state: (read('state') ?? read('uf') ?? '').toString(),
      description: (read('description') ?? read('descricao') ?? '').toString(),
      rating: _readDouble(read('rating') ?? read('averageRating')),
      startingPrice: initialPriceText.isEmpty
          ? _formatPriceFallback(rawPriceMin)
          : initialPriceText,
      reviewCount: (read('reviewCount') as num?)?.toInt() ??
          (read('totalReviews') as num?)?.toInt() ??
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
      featured: _readBool(read('featured') ?? read('destaque')),
      rankingPosition: (read('rankingPosition') as num?)?.toInt() ??
          (read('posicaoRanking') as num?)?.toInt(),
      recentPerformanceScore: _readScore(
        read('recentPerformanceScore') ?? read('performanceRecente'),
      ),
      popularityScore: _readScore(
        read('popularityScore') ?? read('popularidade'),
      ),
      totalHires: (read('totalHires') as num?)?.toInt() ??
          (read('contratacoes') as num?)?.toInt() ??
          0,
      priceMin: rawPriceMin > 0 ? rawPriceMin : null,
      priceMax: rawPriceMax > 0 ? rawPriceMax : null,
      premium: _readBool(read('premium') ?? read('isPremium')),
      imageUrl: read('imageUrl')?.toString() ?? read('avatarUrl')?.toString(),
      badges: (read('badges') as List<dynamic>? ?? const <dynamic>[])
          .map((item) => item.toString())
          .where((item) => item.trim().isNotEmpty)
          .toList(growable: false),
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

  static bool _readBool(dynamic value) {
    if (value is bool) {
      return value;
    }
    final text = value?.toString().toLowerCase().trim();
    return text == 'true' || text == '1' || text == 'sim' || text == 'yes';
  }

  static String _formatPriceFallback(double priceMin) {
    if (priceMin <= 0) {
      return '';
    }
    final intValue = priceMin.toInt();
    return 'R\$ $intValue';
  }
}

class MarketplaceRepository {
  MarketplaceRepository({required ApiClient apiClient})
      : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<MarketplaceSupplierSummary>> getSuppliers({
    String? search,
    String? category,
    String? city,
    String? state,
    double? minPrice,
    double? maxPrice,
    bool? featuredOnly,
    String? orderBy,
    int take = 120,
  }) async {
    final list = await _apiClient.getList(
      '/api/marketplace/suppliers',
      queryParameters: {
        if (search != null && search.trim().isNotEmpty) 'search': search,
        if (category != null && category.trim().isNotEmpty)
          'category': category,
        if (city != null && city.trim().isNotEmpty) 'city': city,
        if (state != null && state.trim().isNotEmpty) 'state': state,
        if (minPrice != null) 'priceMin': minPrice,
        if (maxPrice != null) 'priceMax': maxPrice,
        if (featuredOnly != null) 'featuredOnly': featuredOnly,
        if (orderBy != null && orderBy.trim().isNotEmpty) 'orderBy': orderBy,
        'take': take,
      },
    );
    return list
        .whereType<Map<String, dynamic>>()
        .map(MarketplaceSupplierSummary.fromJson)
        .toList(growable: false);
  }

  Future<MarketplaceSupplierSummary> getSupplierById(int id) async {
    final json = await _apiClient.getJson('/api/marketplace/suppliers/$id');
    return MarketplaceSupplierSummary.fromJson(json);
  }

  Future<List<String>> getCategories() async {
    try {
      final list = await _apiClient.getList('/api/marketplace/categories');
      return list
          .map((item) => item.toString())
          .where((item) => item.trim().isNotEmpty)
          .toList(growable: false);
    } on ApiException catch (error) {
      if (error.isNotFound) {
        return const <String>[];
      }
      rethrow;
    }
  }
}
