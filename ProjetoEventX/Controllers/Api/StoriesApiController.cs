using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Social;
using ProjetoEventX.Helpers;
using ProjetoEventX.Models;
using ProjetoEventX.Security;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api")]
    public class StoriesApiController : ControllerBase
    {
        private const string DefaultProfileImage = "/uploads/social/defaults/default-profile.svg";
        private const string DefaultPostImage = "/uploads/social/defaults/default-post.svg";

        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StoriesApiController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("stories")]
        public async Task<ActionResult<IEnumerable<StoryDto>>> GetStories([FromQuery] int take = 60)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var normalizedTake = Math.Clamp(take, 1, 200);
            var now = DateTime.UtcNow;

            var rawStories = await _context.Stories
                .AsNoTracking()
                .Where(s => s.Ativo && s.ExpireAt > now)
                .OrderByDescending(s => s.CreatedAt)
                .Take(normalizedTake)
                .Select(s => new
                {
                    Id = s.Id,
                    UserIdRaw = s.UserId,
                    PerfilSocialId = s.PerfilSocialId,
                    AuthorName = s.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.PerfilSocial.NomeExibicao)
                        ? s.PerfilSocial.NomeExibicao
                        : "Participante",
                    AuthorAvatar = SocialImagemHelper.NormalizePublicImageUrl(
                        s.PerfilSocial != null ? s.PerfilSocial.FotoPerfilUrl : null,
                        DefaultProfileImage),
                    MediaUrl = SocialImagemHelper.NormalizePublicImageUrl(s.MediaUrl, DefaultPostImage),
                    Caption = s.TextOverlay,
                    Theme = (string?)null,
                    EventId = s.EventoId,
                    EventName = s.Evento != null ? s.Evento.NomeEvento : null,
                    SharedPostId = s.SharedPostId,
                    CreatedAt = s.CreatedAt,
                    ExpireAt = s.ExpireAt,
                    ViewedByCurrentUser = s.Views.Any(v => v.UserId == currentUser.Id.ToString()),
                    ViewsCount = s.Views.Count
                })
                .ToListAsync();

            var stories = rawStories.Select(s => new StoryDto
            {
                Id = s.Id,
                UserId = int.TryParse(s.UserIdRaw, out var uid) ? uid : 0,
                PerfilSocialId = s.PerfilSocialId,
                AuthorName = s.AuthorName,
                AuthorAvatar = s.AuthorAvatar,
                MediaUrl = s.MediaUrl,
                Caption = s.Caption,
                Theme = s.Theme,
                EventId = s.EventId,
                EventName = s.EventName,
                SharedPostId = s.SharedPostId,
                CreatedAt = s.CreatedAt,
                ExpireAt = s.ExpireAt,
                ViewedByCurrentUser = s.ViewedByCurrentUser,
                ViewsCount = s.ViewsCount
            }).ToList();

            return Ok(stories);
        }

        [HttpPost("stories")]
        public async Task<ActionResult<StoryDto>> CreateStory([FromBody] CreateStoryDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!SecurityValidator.IsValidInput(request.MediaUrl))
            {
                return BadRequest(new { message = "MediaUrl inválida." });
            }

            if (!string.IsNullOrWhiteSpace(request.Caption) && !SecurityValidator.IsValidInput(request.Caption, allowHtml: true))
            {
                return BadRequest(new { message = "Legenda inválida." });
            }

            if (request.EventId.HasValue)
            {
                var eventExists = await _context.Eventos.AnyAsync(e => e.Id == request.EventId.Value);
                if (!eventExists)
                {
                    return BadRequest(new { message = "Evento informado não existe." });
                }
            }

            if (request.SharedPostId.HasValue)
            {
                var postExists = await _context.SocialPosts.AnyAsync(p => p.Id == request.SharedPostId.Value && !p.IsDeleted);
                if (!postExists)
                {
                    return BadRequest(new { message = "Post compartilhado não encontrado." });
                }
            }

            var perfil = await EnsurePerfilAsync(currentUser);
            var story = new Story
            {
                UserId = currentUser.Id.ToString(),
                PerfilSocialId = perfil.Id,
                MediaUrl = SecurityValidator.SanitizeInput(request.MediaUrl),
                TextOverlay = string.IsNullOrWhiteSpace(request.Caption)
                    ? string.Empty
                    : SecurityValidator.SanitizeHtml(request.Caption),
                SharedPostId = request.SharedPostId,
                EventoId = request.EventId,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddHours(24),
                Ativo = true
            };

            _context.Stories.Add(story);
            await _context.SaveChangesAsync();

            var created = await _context.Stories
                .AsNoTracking()
                .Include(s => s.PerfilSocial)
                .Include(s => s.Evento)
                .FirstAsync(s => s.Id == story.Id);

            return CreatedAtAction(nameof(GetStories), new { id = story.Id }, new StoryDto
            {
                Id = created.Id,
                UserId = int.TryParse(created.UserId, out var uid) ? uid : 0,
                PerfilSocialId = created.PerfilSocialId,
                AuthorName = created.PerfilSocial?.NomeExibicao ?? currentUser.UserName ?? "Participante",
                AuthorAvatar = SocialImagemHelper.NormalizePublicImageUrl(created.PerfilSocial?.FotoPerfilUrl, DefaultProfileImage),
                MediaUrl = SocialImagemHelper.NormalizePublicImageUrl(created.MediaUrl, DefaultPostImage),
                Caption = created.TextOverlay,
                Theme = request.Theme,
                EventId = created.EventoId,
                EventName = created.Evento?.NomeEvento,
                SharedPostId = created.SharedPostId,
                CreatedAt = created.CreatedAt,
                ExpireAt = created.ExpireAt,
                ViewedByCurrentUser = false,
                ViewsCount = 0
            });
        }

        [HttpPost("stories/{id:int}/view")]
        public async Task<IActionResult> MarkStoryAsViewed(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Story inválido." });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == id && s.Ativo && s.ExpireAt > DateTime.UtcNow);
            if (story == null)
            {
                return NotFound(new { message = "Story não encontrado." });
            }

            if (story.UserId == currentUser.Id.ToString())
            {
                return NoContent();
            }

            var existingView = await _context.StoryViews
                .FirstOrDefaultAsync(v => v.StoryId == id && v.UserId == currentUser.Id.ToString());

            if (existingView == null)
            {
                _context.StoryViews.Add(new StoryView
                {
                    StoryId = id,
                    UserId = currentUser.Id.ToString(),
                    ViewedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpPost("highlights")]
        public async Task<ActionResult<StoryHighlightDto>> CreateHighlight([FromBody] CreateHighlightDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!SecurityValidator.IsValidInput(request.Nome))
            {
                return BadRequest(new { message = "Nome do destaque inválido." });
            }

            var highlight = new StoryHighlight
            {
                UserId = currentUser.Id.ToString(),
                Nome = SecurityValidator.SanitizeInput(request.Nome),
                CapaUrl = string.IsNullOrWhiteSpace(request.CapaUrl)
                    ? null
                    : SecurityValidator.SanitizeInput(request.CapaUrl),
                CreatedAt = DateTime.UtcNow
            };

            _context.StoryHighlights.Add(highlight);
            await _context.SaveChangesAsync();

            if (request.StoryIds.Count > 0)
            {
                var myStoryIds = await _context.Stories
                    .AsNoTracking()
                    .Where(s => s.UserId == currentUser.Id.ToString() && request.StoryIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                var order = 1;
                foreach (var storyId in myStoryIds)
                {
                    _context.StoryHighlightItems.Add(new StoryHighlightItem
                    {
                        HighlightId = highlight.Id,
                        StoryId = storyId,
                        Ordem = order++
                    });
                }

                await _context.SaveChangesAsync();
            }

            var dto = await BuildHighlightDtoAsync(highlight.Id, currentUser.Id);
            return Ok(dto);
        }

        [HttpGet("highlights/me")]
        public async Task<ActionResult<IEnumerable<StoryHighlightDto>>> GetMyHighlights()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var highlightIds = await _context.StoryHighlights
                .AsNoTracking()
                .Where(h => h.UserId == currentUser.Id.ToString())
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => h.Id)
                .ToListAsync();

            var items = new List<StoryHighlightDto>();
            foreach (var highlightId in highlightIds)
            {
                items.Add(await BuildHighlightDtoAsync(highlightId, currentUser.Id));
            }

            return Ok(items);
        }

        private async Task<StoryHighlightDto> BuildHighlightDtoAsync(int highlightId, int currentUserId)
        {
            var highlight = await _context.StoryHighlights
                .AsNoTracking()
                .FirstAsync(h => h.Id == highlightId);

            var rawStories = await _context.StoryHighlightItems
                .AsNoTracking()
                .Where(i => i.HighlightId == highlightId)
                .OrderBy(i => i.Ordem)
                .Select(i => new
                {
                    Id = i.Story.Id,
                    UserIdRaw = i.Story.UserId,
                    PerfilSocialId = i.Story.PerfilSocialId,
                    AuthorName = i.Story.PerfilSocial != null && !string.IsNullOrWhiteSpace(i.Story.PerfilSocial.NomeExibicao)
                        ? i.Story.PerfilSocial.NomeExibicao
                        : "Participante",
                    AuthorAvatar = SocialImagemHelper.NormalizePublicImageUrl(
                        i.Story.PerfilSocial != null ? i.Story.PerfilSocial.FotoPerfilUrl : null,
                        DefaultProfileImage),
                    MediaUrl = SocialImagemHelper.NormalizePublicImageUrl(i.Story.MediaUrl, DefaultPostImage),
                    Caption = i.Story.TextOverlay,
                    Theme = (string?)null,
                    EventId = i.Story.EventoId,
                    EventName = i.Story.Evento != null ? i.Story.Evento.NomeEvento : null,
                    SharedPostId = i.Story.SharedPostId,
                    CreatedAt = i.Story.CreatedAt,
                    ExpireAt = i.Story.ExpireAt,
                    ViewedByCurrentUser = i.Story.Views.Any(v => v.UserId == currentUserId.ToString()),
                    ViewsCount = i.Story.Views.Count
                })
                .ToListAsync();

            var stories = rawStories.Select(i => new StoryDto
            {
                Id = i.Id,
                UserId = int.TryParse(i.UserIdRaw, out var uid) ? uid : 0,
                PerfilSocialId = i.PerfilSocialId,
                AuthorName = i.AuthorName,
                AuthorAvatar = i.AuthorAvatar,
                MediaUrl = i.MediaUrl,
                Caption = i.Caption,
                Theme = i.Theme,
                EventId = i.EventId,
                EventName = i.EventName,
                SharedPostId = i.SharedPostId,
                CreatedAt = i.CreatedAt,
                ExpireAt = i.ExpireAt,
                ViewedByCurrentUser = i.ViewedByCurrentUser,
                ViewsCount = i.ViewsCount
            }).ToList();

            return new StoryHighlightDto
            {
                Id = highlight.Id,
                Nome = highlight.Nome,
                CapaUrl = SocialImagemHelper.NormalizePublicImageUrl(highlight.CapaUrl, DefaultPostImage),
                TotalStories = stories.Count,
                CreatedAt = highlight.CreatedAt,
                Stories = stories
            };
        }

        private async Task<PerfilSocial> EnsurePerfilAsync(ApplicationUser user)
        {
            var perfil = await _context.PerfisSociais.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (perfil != null)
            {
                return perfil;
            }

            perfil = new PerfilSocial
            {
                UserId = user.Id,
                NomeExibicao = user.UserName ?? user.Email ?? $"Usuario {user.Id}",
                Username = user.UserName,
                TipoPerfil = user.TipoUsuario ?? "Convidado",
                FotoPerfilUrl = DefaultProfileImage,
                CriadoEm = DateTime.UtcNow,
                AtualizadoEm = DateTime.UtcNow
            };

            _context.PerfisSociais.Add(perfil);
            await _context.SaveChangesAsync();
            return perfil;
        }
    }
}
