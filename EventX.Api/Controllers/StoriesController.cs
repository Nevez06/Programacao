using EventX.Api.Auth;
using EventX.Api.DTOs.Social;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/stories")]
public sealed class StoriesController : ControllerBase
{
    private readonly ISocialService _socialService;

    public StoriesController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<StoryDto>>> Get(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var stories = await _socialService.GetActiveStoriesAsync(userId, cancellationToken);
        return Ok(stories);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StoryDto>> Create(
        [FromBody] CreateStoryRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _socialService.CreateStoryAsync(userId, request, cancellationToken);
        if (created is null)
        {
            return BadRequest(new { message = "Invalid story payload." });
        }

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("{id:int}/view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> View(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var marked = await _socialService.MarkStoryAsViewedAsync(id, userId, cancellationToken);
        return marked ? Ok(new { storyId = id, viewed = true }) : NotFound();
    }
}
