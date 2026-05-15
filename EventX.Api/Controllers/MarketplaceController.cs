using EventX.Api.DTOs.Marketplace;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/marketplace")]
public sealed class MarketplaceController : ControllerBase
{
    private readonly IMarketplaceService _marketplaceService;

    public MarketplaceController(IMarketplaceService marketplaceService)
    {
        _marketplaceService = marketplaceService;
    }

    [HttpGet("suppliers")]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SupplierSummaryDto>>> GetSuppliers(
        [FromQuery] MarketplaceSuppliersQueryDto query,
        CancellationToken cancellationToken)
    {
        var suppliers = await _marketplaceService.GetSuppliersAsync(query, cancellationToken);
        return Ok(suppliers);
    }

    [HttpGet("suppliers/{id:int}")]
    [ProducesResponseType(typeof(SupplierSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SupplierSummaryDto>> GetSupplierById(int id, CancellationToken cancellationToken)
    {
        var supplier = await _marketplaceService.GetSupplierByIdAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _marketplaceService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }
}
