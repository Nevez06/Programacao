using System.Globalization;
using System.Text.Json;
using EventX.Api.Data;
using EventX.Api.DTOs.Marketplace;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class MarketplaceService : IMarketplaceService
{
    private static readonly IReadOnlyList<(string Name, string Description, int Sort)> SeedCategories =
    [
        ("Buffet", "Serviços de buffet e gastronomia para eventos.", 1),
        ("Fotografia", "Cobertura fotográfica e audiovisual.", 2),
        ("Decoracao", "Cenografia, decoração e floral design.", 3),
        ("Musica", "Bandas, DJs e entretenimento musical.", 4),
        ("Espaco", "Espaços para realização de eventos.", 5),
        ("Conviteria", "Convites, papelaria e identidade visual.", 6),
    ];

    private readonly AppDbContext _dbContext;

    public MarketplaceService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SupplierSummaryDto>> GetSuppliersAsync(
        MarketplaceSuppliersQueryDto query,
        CancellationToken cancellationToken)
    {
        await EnsureSeedDataAsync(cancellationToken);

        var safeTake = Math.Clamp(query.Take <= 0 ? 120 : query.Take, 1, 300);
        var search = Normalize(query.Search);
        var category = Normalize(query.Category);
        var city = Normalize(query.City);
        var state = Normalize(query.State)?.ToUpperInvariant();
        var orderBy = Normalize(query.OrderBy)?.ToLowerInvariant() ?? "recommended";

        var suppliersQuery = _dbContext.Suppliers
            .AsNoTracking()
            .Include(x => x.SupplierCategory)
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            suppliersQuery = suppliersQuery.Where(x =>
                EF.Functions.ILike(x.Name, $"%{search}%") ||
                EF.Functions.ILike(x.SupplierCategory.Name, $"%{search}%") ||
                EF.Functions.ILike(x.City, $"%{search}%") ||
                EF.Functions.ILike(x.State, $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            suppliersQuery = suppliersQuery.Where(x => EF.Functions.ILike(x.SupplierCategory.Name, category));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            suppliersQuery = suppliersQuery.Where(x => EF.Functions.ILike(x.City, city));
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            suppliersQuery = suppliersQuery.Where(x => x.State.ToUpper() == state);
        }

        if (query.FeaturedOnly == true)
        {
            suppliersQuery = suppliersQuery.Where(x => x.Featured);
        }

        if (query.PriceMin is >= 0)
        {
            suppliersQuery = suppliersQuery.Where(x => x.PriceMax == null || x.PriceMax >= query.PriceMin.Value);
        }

        if (query.PriceMax is >= 0)
        {
            suppliersQuery = suppliersQuery.Where(x => x.PriceMin == null || x.PriceMin <= query.PriceMax.Value);
        }

        suppliersQuery = orderBy switch
        {
            "toprated" => suppliersQuery.OrderByDescending(x => x.Rating)
                .ThenByDescending(x => x.ReviewCount),
            "lowestprice" => suppliersQuery.OrderBy(x => x.PriceMin ?? decimal.MaxValue),
            "fastestresponse" => suppliersQuery.OrderByDescending(x => x.ResponseRate),
            "mostpopular" => suppliersQuery.OrderByDescending(x => x.PopularityScore)
                .ThenByDescending(x => x.TotalHires),
            _ => suppliersQuery
                .OrderByDescending(x => x.Featured)
                .ThenByDescending(x => x.Premium)
                .ThenByDescending(x => x.RecentPerformanceScore)
                .ThenByDescending(x => x.Rating)
        };

        var suppliers = await suppliersQuery
            .Take(safeTake)
            .ToListAsync(cancellationToken);

        return suppliers.Select(MapSupplier).ToList();
    }

    public async Task<SupplierSummaryDto?> GetSupplierByIdAsync(int supplierId, CancellationToken cancellationToken)
    {
        if (supplierId <= 0)
        {
            return null;
        }

        var supplier = await _dbContext.Suppliers
            .AsNoTracking()
            .Include(x => x.SupplierCategory)
            .FirstOrDefaultAsync(x => x.Id == supplierId && x.IsActive, cancellationToken);

        return supplier is null ? null : MapSupplier(supplier);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        await EnsureSeedDataAsync(cancellationToken);

        return await _dbContext.SupplierCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureSeedDataAsync(CancellationToken cancellationToken)
    {
        var hasCategories = await _dbContext.SupplierCategories
            .AnyAsync(cancellationToken);

        if (!hasCategories)
        {
            foreach (var item in SeedCategories)
            {
                _dbContext.SupplierCategories.Add(new SupplierCategory
                {
                    Name = item.Name,
                    Description = item.Description,
                    SortOrder = item.Sort,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var hasSuppliers = await _dbContext.Suppliers
            .AnyAsync(cancellationToken);

        if (hasSuppliers)
        {
            return;
        }

        var categories = await _dbContext.SupplierCategories
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Name, x => x.Id, cancellationToken);

        if (categories.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var seedSuppliers = new List<SeedSupplier>
        {
            NewSupplier("Buffet Prime", "Buffet", "Sao Paulo", "SP", 4.9m, 218, 0.91m, 0.95m, 0.03m, 0.94m, true, 1, 0.93m, 0.91m, 145, 12000, 42000, true, ["Top fornecedor","Mais contratado"]),
            NewSupplier("Lens Story", "Fotografia", "Campinas", "SP", 4.8m, 162, 0.88m, 0.92m, 0.04m, 0.91m, true, 2, 0.9m, 0.85m, 120, 6500, 18000, true, ["Resposta rapida","Melhor avaliado"]),
            NewSupplier("Atelie Floral", "Decoracao", "Sao Paulo", "SP", 4.7m, 133, 0.86m, 0.89m, 0.05m, 0.9m, false, 3, 0.86m, 0.8m, 97, 7000, 26000, false, ["Em alta"]),
            NewSupplier("Vibe DJ Live", "Musica", "Rio de Janeiro", "RJ", 4.6m, 104, 0.84m, 0.9m, 0.06m, 0.88m, false, 5, 0.84m, 0.76m, 80, 5000, 15000, false, ["Popular"]),
            NewSupplier("Espaco Lumiere", "Espaco", "Belo Horizonte", "MG", 4.8m, 198, 0.89m, 0.93m, 0.04m, 0.92m, true, 4, 0.89m, 0.84m, 111, 18000, 65000, true, ["Premium"]),
            NewSupplier("Papelaria Aurora", "Conviteria", "Curitiba", "PR", 4.5m, 92, 0.83m, 0.88m, 0.07m, 0.87m, false, 7, 0.79m, 0.69m, 64, 1200, 6800, false, ["Custo-beneficio"]),
        };

        foreach (var supplier in seedSuppliers)
        {
            if (!categories.TryGetValue(supplier.CategoryName, out var categoryId))
            {
                continue;
            }

            _dbContext.Suppliers.Add(new Supplier
            {
                Name = supplier.Name,
                SupplierCategoryId = categoryId,
                City = supplier.City,
                State = supplier.State,
                Description = supplier.Description,
                Rating = supplier.Rating,
                ReviewCount = supplier.ReviewCount,
                AcceptanceRate = supplier.AcceptanceRate,
                ResponseRate = supplier.ResponseRate,
                CancellationRate = supplier.CancellationRate,
                PunctualityScore = supplier.PunctualityScore,
                Featured = supplier.Featured,
                RankingPosition = supplier.RankingPosition,
                RecentPerformanceScore = supplier.RecentPerformanceScore,
                PopularityScore = supplier.PopularityScore,
                TotalHires = supplier.TotalHires,
                PriceMin = supplier.PriceMin,
                PriceMax = supplier.PriceMax,
                Premium = supplier.Premium,
                BadgesJson = supplier.BadgesJson,
                IsActive = true,
                CreatedAt = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SeedSupplier NewSupplier(
        string name,
        string categoryName,
        string city,
        string state,
        decimal rating,
        int reviewCount,
        decimal acceptanceRate,
        decimal responseRate,
        decimal cancellationRate,
        decimal punctualityScore,
        bool featured,
        int? rankingPosition,
        decimal recentPerformanceScore,
        decimal popularityScore,
        int totalHires,
        decimal? priceMin,
        decimal? priceMax,
        bool premium,
        IReadOnlyList<string> badges)
    {
        return new SeedSupplier(
            Name: name,
            CategoryName: categoryName,
            City: city,
            State: state,
            Description: $"Fornecedor {name} especializado em {categoryName}.",
            Rating: rating,
            ReviewCount: reviewCount,
            AcceptanceRate: acceptanceRate,
            ResponseRate: responseRate,
            CancellationRate: cancellationRate,
            PunctualityScore: punctualityScore,
            Featured: featured,
            RankingPosition: rankingPosition,
            RecentPerformanceScore: recentPerformanceScore,
            PopularityScore: popularityScore,
            TotalHires: totalHires,
            PriceMin: priceMin,
            PriceMax: priceMax,
            Premium: premium,
            BadgesJson: JsonSerializer.Serialize(badges));
    }

    private static SupplierSummaryDto MapSupplier(Supplier supplier)
    {
        IReadOnlyList<string> badges;
        try
        {
            badges = JsonSerializer.Deserialize<List<string>>(supplier.BadgesJson) ?? [];
        }
        catch
        {
            badges = [];
        }

        var startingPrice = supplier.PriceMin.HasValue
            ? string.Format(CultureInfo.InvariantCulture, "R$ {0:N0}", supplier.PriceMin.Value)
            : string.Empty;

        return new SupplierSummaryDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Category = supplier.SupplierCategory.Name,
            City = supplier.City,
            State = supplier.State,
            Description = supplier.Description,
            Rating = supplier.Rating,
            StartingPrice = startingPrice,
            ReviewCount = supplier.ReviewCount,
            AcceptanceRate = supplier.AcceptanceRate,
            ResponseRate = supplier.ResponseRate,
            CancellationRate = supplier.CancellationRate,
            PunctualityScore = supplier.PunctualityScore,
            Featured = supplier.Featured,
            RankingPosition = supplier.RankingPosition,
            RecentPerformanceScore = supplier.RecentPerformanceScore,
            PopularityScore = supplier.PopularityScore,
            TotalHires = supplier.TotalHires,
            PriceMin = supplier.PriceMin,
            PriceMax = supplier.PriceMax,
            Premium = supplier.Premium,
            ImageUrl = supplier.ImageUrl,
            AvatarUrl = supplier.ImageUrl,
            Badges = badges
        };
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private sealed record SeedSupplier(
        string Name,
        string CategoryName,
        string City,
        string State,
        string Description,
        decimal Rating,
        int ReviewCount,
        decimal AcceptanceRate,
        decimal ResponseRate,
        decimal CancellationRate,
        decimal PunctualityScore,
        bool Featured,
        int? RankingPosition,
        decimal RecentPerformanceScore,
        decimal PopularityScore,
        int TotalHires,
        decimal? PriceMin,
        decimal? PriceMax,
        bool Premium,
        string BadgesJson);
}
