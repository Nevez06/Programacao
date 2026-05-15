class MarketplaceProductModel {
  const MarketplaceProductModel({
    required this.id,
    required this.title,
    required this.priceCents,
    required this.imageUrl,
    this.previousPriceCents,
    this.location,
  });

  final int id;
  final String title;
  final int priceCents;
  final int? previousPriceCents;
  final String imageUrl;
  final String? location;
}
