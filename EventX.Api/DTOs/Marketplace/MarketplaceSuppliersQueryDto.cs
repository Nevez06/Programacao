namespace EventX.Api.DTOs.Marketplace;

public sealed class MarketplaceSuppliersQueryDto
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public bool? FeaturedOnly { get; set; }
    public string? OrderBy { get; set; }
    public int Take { get; set; } = 120;
}
