using EventX.Api.Auth;
using EventX.Api.DTOs.Social;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/feed")]
public sealed class FeedController : ControllerBase
{
    private readonly ISocialService _socialService;

    public FeedController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FeedPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FeedPostDto>>> Get(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var feed = await _socialService.GetFeedAsync(userId, cancellationToken);
        return Ok(feed);
    }

    [HttpPost("posts")]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostDetailsDto>> CreatePost(
        [FromBody] CreatePostRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _socialService.CreatePostAsync(userId, request, cancellationToken);
        if (created is null)
        {
            return BadRequest(new { message = "Invalid post payload." });
        }

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
