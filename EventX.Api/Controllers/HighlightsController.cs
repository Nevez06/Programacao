using EventX.Api.Auth;
using EventX.Api.DTOs.Social;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/highlights")]
public sealed class HighlightsController : ControllerBase
{
    private readonly ISocialService _socialService;

    public HighlightsController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<StoryHighlightDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<StoryHighlightDto>>> GetMe(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var highlights = await _socialService.GetMyHighlightsAsync(userId, cancellationToken);
        return Ok(highlights);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StoryHighlightDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StoryHighlightDto>> Create(
        [FromBody] CreateHighlightRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _socialService.CreateHighlightAsync(userId, request, cancellationToken);
        if (created is null)
        {
            return BadRequest(new { message = "Invalid highlight payload." });
        }

        return CreatedAtAction(nameof(GetMe), routeValues: null, value: created);
    }
}
