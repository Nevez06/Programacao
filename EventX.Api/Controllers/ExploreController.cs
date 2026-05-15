using EventX.Api.Auth;
using EventX.Api.DTOs.Social;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/explore")]
public sealed class ExploreController : ControllerBase
{
    private readonly ISocialService _socialService;

    public ExploreController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ExploreResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ExploreResponseDto>> Get(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var response = await _socialService.GetExploreAsync(userId, cancellationToken);
        return Ok(response);
    }
}
