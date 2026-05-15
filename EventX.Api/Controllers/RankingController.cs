using EventX.Api.DTOs.Ranking;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ranking/suppliers")]
public sealed class RankingController : ControllerBase
{
    private readonly IRankingService _rankingService;

    public RankingController(IRankingService rankingService)
    {
        _rankingService = rankingService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierRankingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SupplierRankingDto>>> GetSuppliersRanking(
        [FromQuery] RankingSuppliersQueryDto query,
        CancellationToken cancellationToken)
    {
        var ranking = await _rankingService.GetRankingAsync(query, cancellationToken);
        return Ok(ranking);
    }

    [HttpGet("top")]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierRankingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SupplierRankingDto>>> GetTopSuppliers(
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var top = await _rankingService.GetTopAsync(take, cancellationToken);
        return Ok(top);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SupplierRankingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SupplierRankingDto>> GetSupplierRankingById(
        int id,
        CancellationToken cancellationToken)
    {
        var supplier = await _rankingService.GetBySupplierIdAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }
}
