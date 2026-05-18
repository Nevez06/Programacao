using EventX.Api.DTOs.Marketplace;

namespace EventX.Api.Services;

public interface IMarketplaceService
{
    Task<IReadOnlyList<SupplierSummaryDto>> GetSuppliersAsync(
        MarketplaceSuppliersQueryDto query,
        CancellationToken cancellationToken);

    Task<SupplierSummaryDto?> GetSupplierByIdAsync(int supplierId, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken);
}
