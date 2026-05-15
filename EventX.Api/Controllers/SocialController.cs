using EventX.Api.Auth;
using EventX.Api.DTOs.Social;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/social")]
public sealed class SocialController : ControllerBase
{
    private readonly ISocialService _socialService;

    public SocialController(ISocialService socialService)
    {
        _socialService = socialService;
    }

    [HttpGet("feed")]
    [ProducesResponseType(typeof(IReadOnlyList<FeedPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FeedPostDto>>> GetFeed(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var feed = await _socialService.GetFeedAsync(userId, cancellationToken);
        return Ok(feed);
    }

    [HttpGet("posts/{id:int}")]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostDetailsDto>> GetPostById(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var post = await _socialService.GetPostByIdAsync(id, userId, cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpPost("posts")]
    [ProducesResponseType(typeof(PostDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostDetailsDto>> CreatePost(
        [FromBody] CreateSocialPostDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _socialService.CreatePostAsync(
            userId,
            new CreatePostRequestDto
            {
                Caption = request.Content,
                ImageUrl = request.ImageUrl
            },
            cancellationToken);

        if (created is null)
        {
            return BadRequest(new { message = "Conteudo do post invalido." });
        }

        return CreatedAtAction(nameof(GetPostById), new { id = created.Id }, created);
    }

    [HttpPost("posts/{id:int}/like")]
    [ProducesResponseType(typeof(ToggleLikeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ToggleLikeResponseDto>> LikePost(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _socialService.LikePostAsync(id, userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("posts/{id:int}/like")]
    [ProducesResponseType(typeof(ToggleLikeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ToggleLikeResponseDto>> UnlikePost(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _socialService.UnlikePostAsync(id, userId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("posts/{id:int}/comments")]
    [ProducesResponseType(typeof(IReadOnlyList<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetComments(
        int id,
        CancellationToken cancellationToken)
    {
        var comments = await _socialService.GetCommentsAsync(id, cancellationToken);
        if (comments is null)
        {
            return NotFound();
        }

        var mapped = comments
            .Select(x => new CommentDto
            {
                Id = x.Id,
                AuthorName = x.AuthorName,
                Content = x.Content,
                CreatedAt = x.CreatedAtUtc
            })
            .ToList();

        return Ok(mapped);
    }

    [HttpPost("posts/{id:int}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CommentDto>> AddComment(
        int id,
        [FromBody] CreateCommentDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var comment = await _socialService.AddCommentAsync(
            id,
            userId,
            new CreateCommentRequestDto { Content = request.Content },
            cancellationToken);

        if (comment is null)
        {
            return BadRequest(new { message = "Nao foi possivel adicionar comentario." });
        }

        return Ok(new CommentDto
        {
            Id = comment.Id,
            AuthorName = comment.AuthorName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAtUtc
        });
    }
}
