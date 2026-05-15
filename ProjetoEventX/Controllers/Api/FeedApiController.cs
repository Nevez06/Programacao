using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoEventX.DTOs.Feed;
using ProjetoEventX.Services;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api")]
    public class FeedApiController : ControllerBase
    {
        private readonly FeedService _feedService;

        public FeedApiController(FeedService feedService)
        {
            _feedService = feedService;
        }

        [HttpGet("feed")]
        public async Task<ActionResult<IEnumerable<FeedPostDto>>> GetFeed([FromQuery] int take = 40)
        {
            var user = await _feedService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var feed = await _feedService.GetFeedAsync(user, take);
            return Ok(feed);
        }

        [HttpGet("posts/{id:int}")]
        public async Task<ActionResult<FeedPostDto>> GetPostById(int id)
        {
            var user = await _feedService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var post = await _feedService.GetPostDetailsAsync(user, id);
            if (post == null)
            {
                return NotFound(new { message = "Post nao encontrado." });
            }

            return Ok(post);
        }

        [HttpPost("posts")]
        public async Task<ActionResult<FeedPostDto>> CreatePost([FromBody] CreatePostDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _feedService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var created = await _feedService.CreatePostAsync(user, request);
                return CreatedAtAction(nameof(GetPostById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("posts/{id:int}")]
        public async Task<ActionResult<FeedPostDto>> UpdatePost(int id, [FromBody] UpdatePostDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _feedService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var updated = await _feedService.UpdatePostAsync(user, id, request);
                if (updated == null)
                {
                    return NotFound(new { message = "Post nao encontrado." });
                }

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("posts/{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var user = await _feedService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var deleted = await _feedService.DeletePostAsync(user, id);
            if (!deleted)
            {
                return NotFound(new { message = "Post nao encontrado." });
            }

            return NoContent();
        }

        [HttpPost("posts/{id:int}/like")]
        public async Task<IActionResult> ToggleLike(int id, [FromBody] ToggleLikeDto? request)
        {
            var user = await _feedService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _feedService.ToggleLikeAsync(user, id, request?.Curtir);
            if (result == null)
            {
                return NotFound(new { message = "Post nao encontrado." });
            }

            return Ok(new
            {
                usuarioCurtiu = result.Value.UsuarioCurtiu,
                totalCurtidas = result.Value.TotalCurtidas
            });
        }

        [HttpPost("posts/{id:int}/comment")]
        public async Task<ActionResult<PostCommentDto>> AddComment(int id, [FromBody] PostCommentDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _feedService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var comment = await _feedService.AddCommentAsync(user, id, request.Texto);
                if (comment == null)
                {
                    return NotFound(new { message = "Post nao encontrado." });
                }

                return Ok(comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
