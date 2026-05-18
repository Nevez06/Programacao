using EventX.Api.DTOs.Ranking;

namespace EventX.Api.Services;

public interface IRankingService
{
    Task<IReadOnlyList<SupplierRankingDto>> GetRankingAsync(
        RankingSuppliersQueryDto query,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupplierRankingDto>> GetTopAsync(int take, CancellationToken cancellationToken);

    Task<SupplierRankingDto?> GetBySupplierIdAsync(int supplierId, CancellationToken cancellationToken);
}
