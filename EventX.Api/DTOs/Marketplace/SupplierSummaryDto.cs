namespace EventX.Api.DTOs.Marketplace;

public sealed class SupplierSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string StartingPrice { get; set; } = string.Empty;
    public int ReviewCount { get; set; }
    public decimal AcceptanceRate { get; set; }
    public decimal ResponseRate { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal PunctualityScore { get; set; }
    public bool Featured { get; set; }
    public int? RankingPosition { get; set; }
    public decimal RecentPerformanceScore { get; set; }
    public decimal PopularityScore { get; set; }
    public int TotalHires { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public bool Premium { get; set; }
    public string? ImageUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public IReadOnlyList<string> Badges { get; set; } = [];
}
