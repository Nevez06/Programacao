namespace ProjetoEventX.DTOs.Marketplace
{
    public class MarketplaceSupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string StartingPrice { get; set; } = string.Empty;
        public double? PriceMin { get; set; }
        public double? PriceMax { get; set; }
        public bool Featured { get; set; }
        public bool Premium { get; set; }
        public int? RankingPosition { get; set; }
        public double AcceptanceRate { get; set; }
        public double ResponseRate { get; set; }
        public double CancellationRate { get; set; }
        public double PunctualityScore { get; set; }
        public double PopularityScore { get; set; }
        public double RecentPerformanceScore { get; set; }
        public int TotalHires { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Badges { get; set; } = new();
    }
}