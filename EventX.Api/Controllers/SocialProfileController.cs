using EventX.Api.Auth;
using EventX.Api.DTOs.Social;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/social/profile")]
public sealed class SocialProfileController : ControllerBase
{
    private readonly ISocialService _socialService;

    public SocialProfileController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(SocialProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SocialProfileDto>> GetMe(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _socialService.GetMyProfileAsync(userId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(SocialProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SocialProfileDto>> UpdateMe(
        [FromBody] UpdateSocialProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _socialService.UpdateMyProfileAsync(userId, request, cancellationToken);
        if (profile is null)
        {
            return BadRequest(new { message = "Unable to update social profile." });
        }

        return Ok(profile);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SocialProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SocialProfileDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var requesterId = User.TryGetUserId(out var userId) ? (Guid?)userId : null;
        var profile = await _socialService.GetProfileByIdAsync(id, requesterId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(SocialProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SocialProfileDto>> GetByUserName(
        string username,
        CancellationToken cancellationToken)
    {
        var requesterId = User.TryGetUserId(out var userId) ? (Guid?)userId : null;
        var profile = await _socialService.GetProfileByUserNameAsync(username, requesterId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("{id:int}/posts")]
    [ProducesResponseType(typeof(IReadOnlyList<FeedPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FeedPostDto>>> GetPostsByProfile(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var posts = await _socialService.GetPostsByProfileIdAsync(id, userId, cancellationToken);
        return Ok(posts);
    }

    [HttpGet("{id:int}/stories")]
    [ProducesResponseType(typeof(IReadOnlyList<StoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<StoryDto>>> GetStoriesByProfile(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var stories = await _socialService.GetStoriesByProfileIdAsync(id, userId, cancellationToken);
        return Ok(stories);
    }

    [HttpPost("{id:int}/follow")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Follow(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _socialService.FollowProfileAsync(id, userId, cancellationToken);
        return result ? Ok(new { followed = true }) : NotFound();
    }

    [HttpDelete("{id:int}/follow")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unfollow(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _socialService.UnfollowProfileAsync(id, userId, cancellationToken);
        return result ? Ok(new { followed = false }) : NotFound();
    }

    [HttpGet("{id:int}/followers")]
    [ProducesResponseType(typeof(IReadOnlyList<SocialProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SocialProfileDto>>> GetFollowers(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profiles = await _socialService.GetFollowersAsync(id, userId, cancellationToken);
        return Ok(profiles);
    }

    [HttpGet("{id:int}/following")]
    [ProducesResponseType(typeof(IReadOnlyList<SocialProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SocialProfileDto>>> GetFollowing(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profiles = await _socialService.GetFollowingAsync(id, userId, cancellationToken);
        return Ok(profiles);
    }
}
