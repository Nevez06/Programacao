using EventX.Api.Data;
using EventX.Api.DTOs.Ranking;
using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Services;

public sealed class RankingService : IRankingService
{
    private static readonly RankingWeights Weights = new();

    private readonly AppDbContext _dbContext;
    private readonly IMarketplaceService _marketplaceService;

    public RankingService(AppDbContext dbContext, IMarketplaceService marketplaceService)
    {
        _dbContext = dbContext;
        _marketplaceService = marketplaceService;
    }

    public async Task<IReadOnlyList<SupplierRankingDto>> GetRankingAsync(
        RankingSuppliersQueryDto query,
        CancellationToken cancellationToken)
    {
        await EnsureSuppliersSeededAsync(cancellationToken);

        var safeTake = Math.Clamp(query.Take <= 0 ? 120 : query.Take, 1, 300);
        var category = Normalize(query.Category);
        var city = Normalize(query.City);
        var state = Normalize(query.State)?.ToUpperInvariant();

        var suppliersQuery = _dbContext.Suppliers
            .AsNoTracking()
            .Include(x => x.SupplierCategory)
            .Where(x => x.IsActive);

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

        var suppliers = await suppliersQuery.ToListAsync(cancellationToken);
        if (suppliers.Count == 0)
        {
            return [];
        }

        var supplierIds = suppliers.Select(x => x.Id).ToArray();
        var orderStats = await LoadOrderStatsAsync(supplierIds, cancellationToken);

        var rankedRows = suppliers
            .Select(supplier => BuildRankingRow(supplier, orderStats.GetValueOrDefault(supplier.Id)))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Supplier.Rating)
            .ThenByDescending(x => x.CompletedOrders)
            .ThenBy(x => x.Supplier.Name)
            .Take(safeTake)
            .ToList();

        return rankedRows
            .Select((row, index) => Map(row, index + 1))
            .ToList();
    }

    public async Task<IReadOnlyList<SupplierRankingDto>> GetTopAsync(int take, CancellationToken cancellationToken)
    {
        return await GetRankingAsync(
            new RankingSuppliersQueryDto { Take = Math.Clamp(take, 1, 50) },
            cancellationToken);
    }

    public async Task<SupplierRankingDto?> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken)
    {
        if (supplierId <= 0)
        {
            return null;
        }

        await EnsureSuppliersSeededAsync(cancellationToken);

        var suppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .Include(x => x.SupplierCategory)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        if (suppliers.Count == 0)
        {
            return null;
        }

        var orderStats = await LoadOrderStatsAsync(suppliers.Select(x => x.Id).ToArray(), cancellationToken);
        var rankedRows = suppliers
            .Select(supplier => BuildRankingRow(supplier, orderStats.GetValueOrDefault(supplier.Id)))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Supplier.Rating)
            .ThenByDescending(x => x.CompletedOrders)
            .ThenBy(x => x.Supplier.Name)
            .ToList();

        var index = rankedRows.FindIndex(x => x.Supplier.Id == supplierId);
        if (index < 0)
        {
            return null;
        }

        return Map(rankedRows[index], index + 1);
    }

    private async Task EnsureSuppliersSeededAsync(CancellationToken cancellationToken)
    {
        var hasSuppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (!hasSuppliers)
        {
            _ = await _marketplaceService.GetCategoriesAsync(cancellationToken);
        }
    }

    private async Task<Dictionary<int, SupplierOrderStats>> LoadOrderStatsAsync(
        IReadOnlyCollection<int> supplierIds,
        CancellationToken cancellationToken)
    {
        var rows = await _dbContext.Orders
            .AsNoTracking()
            .Where(x => x.SupplierId != null && supplierIds.Contains(x.SupplierId.Value))
            .GroupBy(x => x.SupplierId!.Value)
            .Select(group => new SupplierOrderStats
            {
                SupplierId = group.Key,
                TotalOrders = group.Count(),
                CompletedOrders = group.Count(x => x.Status == OrderStatus.Entregue),
                CancelledOrders = group.Count(x => x.Status == OrderStatus.Cancelado),
                EventsAttended = group
                    .Where(x => x.Status == OrderStatus.Pago ||
                                x.Status == OrderStatus.Enviado ||
                                x.Status == OrderStatus.Entregue)
                    .Select(x => x.EventId)
                    .Distinct()
                    .Count()
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.SupplierId, x => x);
    }

    private static RankedSupplierRow BuildRankingRow(Supplier supplier, SupplierOrderStats? stats)
    {
        var reviewCount = Math.Max(0, supplier.ReviewCount);
        var ratingNormalized = Clamp01((double)supplier.Rating / 5d);
        var reviewConfidence = Clamp01(Math.Log10(reviewCount + 1d) / 2d);

        var completedOrders = stats?.CompletedOrders ?? 0;
        var eventsAttended = stats?.EventsAttended ?? 0;
        if (completedOrders == 0 && supplier.TotalHires > 0)
        {
            completedOrders = supplier.TotalHires;
        }

        if (eventsAttended == 0 && completedOrders > 0)
        {
            eventsAttended = Math.Max(1, completedOrders / 3);
        }

        var completedOrdersNormalized = Clamp01(completedOrders / 50d);
        var eventsAttendedNormalized = Clamp01(eventsAttended / 30d);
        var responseRateNormalized = NormalizeRate(supplier.ResponseRate);
        var acceptanceRateNormalized = NormalizeRate(supplier.AcceptanceRate);

        var cancellationFromOrders = stats is { TotalOrders: > 0 }
            ? Clamp01((double)stats.CancelledOrders / stats.TotalOrders)
            : NormalizeRate(supplier.CancellationRate);
        var cancellationScore = 1d - cancellationFromOrders;

        var finalNormalized =
            (Weights.RatingQuality * ratingNormalized) +
            (Weights.ReviewCount * reviewConfidence) +
            (Weights.CompletedOrders * completedOrdersNormalized) +
            (Weights.EventsAttended * eventsAttendedNormalized) +
            (Weights.ResponseRate * responseRateNormalized) +
            (Weights.AcceptanceRate * acceptanceRateNormalized) +
            (Weights.Cancellation * cancellationScore);

        return new RankedSupplierRow
        {
            Supplier = supplier,
            Score = (int)Math.Round(Clamp01(finalNormalized) * 1000d, MidpointRounding.AwayFromZero),
            CompletedOrders = completedOrders,
            EventsAttended = eventsAttended
        };
    }

    private static SupplierRankingDto Map(RankedSupplierRow row, int position)
    {
        return new SupplierRankingDto
        {
            Id = row.Supplier.Id,
            SupplierId = row.Supplier.Id,
            Position = position,
            Name = row.Supplier.Name,
            AvatarUrl = row.Supplier.ImageUrl,
            Rating = row.Supplier.Rating,
            Score = row.Score,
            Delta = BuildDelta(row.Supplier.RankingPosition, position),
            Category = row.Supplier.SupplierCategory.Name,
            City = row.Supplier.City,
            AverageRating = row.Supplier.Rating,
            ReviewCount = row.Supplier.ReviewCount,
            AcceptanceRate = row.Supplier.AcceptanceRate,
            ResponseRate = row.Supplier.ResponseRate,
            CancellationRate = row.Supplier.CancellationRate,
            PunctualityScore = row.Supplier.PunctualityScore,
            PopularityScore = row.Supplier.PopularityScore,
            RecentPerformanceScore = row.Supplier.RecentPerformanceScore,
            CompletedOrders = row.CompletedOrders,
            EventsAttended = row.EventsAttended
        };
    }

    private static double NormalizeRate(decimal value)
    {
        var raw = (double)value;
        if (raw <= 1d)
        {
            return Clamp01(raw);
        }

        return Clamp01(raw / 100d);
    }

    private static double Clamp01(double value)
    {
        if (value < 0d)
        {
            return 0d;
        }

        if (value > 1d)
        {
            return 1d;
        }

        return value;
    }

    private static string BuildDelta(int? previousPosition, int currentPosition)
    {
        if (previousPosition is null or <= 0)
        {
            return "novo";
        }

        if (previousPosition.Value == currentPosition)
        {
            return "—";
        }

        var delta = previousPosition.Value - currentPosition;
        if (delta > 0)
        {
            return $"↑{delta}";
        }

        return $"↓{Math.Abs(delta)}";
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private sealed class RankingWeights
    {
        public double RatingQuality { get; init; } = 0.30d;
        public double ReviewCount { get; init; } = 0.10d;
        public double CompletedOrders { get; init; } = 0.20d;
        public double EventsAttended { get; init; } = 0.10d;
        public double ResponseRate { get; init; } = 0.10d;
        public double AcceptanceRate { get; init; } = 0.10d;
        public double Cancellation { get; init; } = 0.10d;
    }

    private sealed class SupplierOrderStats
    {
        public int SupplierId { get; init; }
        public int TotalOrders { get; init; }
        public int CompletedOrders { get; init; }
        public int CancelledOrders { get; init; }
        public int EventsAttended { get; init; }
    }

    private sealed class RankedSupplierRow
    {
        public Supplier Supplier { get; init; } = null!;
        public int Score { get; init; }
        public int CompletedOrders { get; init; }
        public int EventsAttended { get; init; }
    }
}
