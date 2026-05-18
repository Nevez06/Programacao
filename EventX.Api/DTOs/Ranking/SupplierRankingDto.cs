namespace EventX.Api.DTOs.Ranking;

public sealed class SupplierRankingDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int Position { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public decimal Rating { get; set; }
    public int Score { get; set; }
    public string Delta { get; set; } = "—";
    public string Category { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public decimal AcceptanceRate { get; set; }
    public decimal ResponseRate { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal PunctualityScore { get; set; }
    public decimal PopularityScore { get; set; }
    public decimal RecentPerformanceScore { get; set; }
    public int CompletedOrders { get; set; }
    public int EventsAttended { get; set; }
}
