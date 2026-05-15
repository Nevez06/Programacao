using System.ComponentModel.DataAnnotations;

namespace EventX.Api.Entities;

public sealed class Supplier
{
    public int Id { get; set; }

    [Required]
    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    public int SupplierCategoryId { get; set; }
    public SupplierCategory SupplierCategory { get; set; } = null!;

    [MaxLength(120)]
    public string City { get; set; } = string.Empty;

    [MaxLength(2)]
    public string State { get; set; } = string.Empty;

    [MaxLength(2500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }

    public decimal Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public decimal AcceptanceRate { get; set; } = 0;
    public decimal ResponseRate { get; set; } = 0;
    public decimal CancellationRate { get; set; } = 0;
    public decimal PunctualityScore { get; set; } = 0;
    public bool Featured { get; set; }
    public int? RankingPosition { get; set; }
    public decimal RecentPerformanceScore { get; set; } = 0;
    public decimal PopularityScore { get; set; } = 0;
    public int TotalHires { get; set; } = 0;
    public bool Premium { get; set; }

    [MaxLength(2000)]
    public string BadgesJson { get; set; } = "[]";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
