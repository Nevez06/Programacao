using EventX.Api.Auth;
using EventX.Api.DTOs.Social;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/posts")]
public sealed class PostsController : ControllerBase
{
    private readonly ISocialService _socialService;

    public PostsController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostDetailsDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var post = await _socialService.GetPostByIdAsync(id, userId, cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostDetailsDto>> Create(
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

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:int}/like")]
    [ProducesResponseType(typeof(ToggleLikeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ToggleLikeResponseDto>> ToggleLike(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var response = await _socialService.ToggleLikeAsync(id, userId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("{id:int}/comment")]
    [ProducesResponseType(typeof(PostCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostCommentDto>> AddComment(
        int id,
        [FromBody] CreateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var comment = await _socialService.AddCommentAsync(id, userId, request, cancellationToken);
        if (comment is null)
        {
            return BadRequest(new { message = "Unable to comment on this post." });
        }

        return Ok(comment);
    }
}
