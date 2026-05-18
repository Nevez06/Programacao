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
    [Route("api/social/profile")]
    public class SocialProfileApiController : ControllerBase
    {
        private const string DefaultProfileImage = "/uploads/social/defaults/default-profile.svg";
        private const string DefaultPostImage = "/uploads/social/defaults/default-post.svg";

        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SocialProfileApiController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("me")]
        public async Task<ActionResult<SocialProfileDto>> GetMe()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var perfil = await EnsurePerfilAsync(currentUser);
            var dto = await BuildProfileDtoAsync(perfil, currentUser.Id);
            return Ok(dto);
        }

        [HttpPut("me")]
        public async Task<ActionResult<SocialProfileDto>> UpdateMe([FromBody] UpdateSocialProfileDto request)
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

            var perfil = await EnsurePerfilAsync(currentUser);

            if (request.NomeExibicao != null)
            {
                if (!SecurityValidator.IsValidInput(request.NomeExibicao))
                {
                    return BadRequest(new { message = "Nome de exibição inválido." });
                }
                perfil.NomeExibicao = SecurityValidator.SanitizeInput(request.NomeExibicao);
            }

            if (request.Username != null)
            {
                if (!SecurityValidator.IsValidInput(request.Username))
                {
                    return BadRequest(new { message = "Username inválido." });
                }
                var normalized = request.Username.Trim().TrimStart('@');
                if (await _context.PerfisSociais.AnyAsync(p => p.Id != perfil.Id && p.Username != null && p.Username.ToLower() == normalized.ToLower()))
                {
                    return Conflict(new { message = "Username já está em uso." });
                }

                perfil.Username = normalized;
            }

            if (request.Bio != null)
            {
                if (!SecurityValidator.IsValidInput(request.Bio, allowHtml: true))
                {
                    return BadRequest(new { message = "Bio inválida." });
                }
                perfil.Bio = SecurityValidator.SanitizeHtml(request.Bio);
            }

            if (request.FotoPerfilUrl != null)
            {
                if (!SecurityValidator.IsValidInput(request.FotoPerfilUrl))
                {
                    return BadRequest(new { message = "Foto inválida." });
                }
                perfil.FotoPerfilUrl = SecurityValidator.SanitizeInput(request.FotoPerfilUrl);
            }

            if (request.Cidade != null)
            {
                perfil.Cidade = SecurityValidator.SanitizeInput(request.Cidade);
            }

            if (request.Instagram != null)
            {
                perfil.Instagram = SecurityValidator.SanitizeInput(request.Instagram);
            }

            if (request.Site != null)
            {
                perfil.Site = SecurityValidator.SanitizeInput(request.Site);
            }

            perfil.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var dto = await BuildProfileDtoAsync(perfil, currentUser.Id);
            return Ok(dto);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SocialProfileDto>> GetById(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var perfil = await ResolveProfileByRouteIdAsync(id);
            if (perfil == null)
            {
                return NotFound(new { message = "Perfil social não encontrado." });
            }

            var dto = await BuildProfileDtoAsync(perfil, currentUser.Id);
            return Ok(dto);
        }

        [HttpGet("username/{username}")]
        public async Task<ActionResult<SocialProfileDto>> GetByUsername(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var normalized = username.Trim().TrimStart('@').ToLowerInvariant();
            var perfil = await _context.PerfisSociais
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Username != null && p.Username.ToLower() == normalized);

            if (perfil == null)
            {
                return NotFound(new { message = "Perfil social não encontrado." });
            }

            var dto = await BuildProfileDtoAsync(perfil, currentUser.Id);
            return Ok(dto);
        }

        [HttpGet("{id:int}/posts")]
        public async Task<ActionResult<IEnumerable<SocialPostSummaryDto>>> GetProfilePosts(int id, [FromQuery] int take = 60)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var perfil = await ResolveProfileByRouteIdAsync(id);
            if (perfil == null)
            {
                return NotFound(new { message = "Perfil social não encontrado." });
            }

            var normalizedTake = Math.Clamp(take, 1, 200);
            var posts = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.PerfilSocialId == perfil.Id && p.Ativo && !p.IsDeleted)
                .OrderByDescending(p => p.CriadoEm)
                .Take(normalizedTake)
                .Select(p => new SocialPostSummaryDto
                {
                    Id = p.Id,
                    AuthorId = p.UserId,
                    AuthorName = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.NomeExibicao)
                        ? p.PerfilSocial.NomeExibicao
                        : (p.User != null ? (p.User.UserName ?? p.User.Email ?? "Participante") : "Participante"),
                    AuthorAvatar = SocialImagemHelper.NormalizePublicImageUrl(
                        p.PerfilSocial != null ? p.PerfilSocial.FotoPerfilUrl : null,
                        DefaultProfileImage),
                    EventId = p.EventoId,
                    EventName = p.Evento != null ? p.Evento.NomeEvento : null,
                    ImageUrl = SocialImagemHelper.NormalizePublicImageUrl(p.ImagemUrl, DefaultPostImage),
                    Caption = p.Legenda,
                    Category = p.Categoria,
                    ContentType = p.TipoConteudo,
                    LikesCount = p.Curtidas.Count,
                    CommentsCount = p.Comentarios.Count(c => c.Ativo),
                    SharesCount = 0,
                    ViewsCount = 0,
                    CreatedAt = p.CriadoEm,
                    IsLikedByCurrentUser = p.Curtidas.Any(c => c.UserId == currentUser.Id),
                    IsSavedByCurrentUser = p.Salvos.Any(s => s.UserId == currentUser.Id)
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("{id:int}/stories")]
        public async Task<ActionResult<IEnumerable<StoryDto>>> GetProfileStories(int id, [FromQuery] int take = 60)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var perfil = await ResolveProfileByRouteIdAsync(id);
            if (perfil == null)
            {
                return NotFound(new { message = "Perfil social não encontrado." });
            }

            var now = DateTime.UtcNow;
            var normalizedTake = Math.Clamp(take, 1, 200);

            var rawStories = await _context.Stories
                .AsNoTracking()
                .Where(s => s.PerfilSocialId == perfil.Id && s.Ativo && s.ExpireAt > now)
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

        [HttpPost("{id:int}/follow")]
        public async Task<IActionResult> Follow(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var currentPerfil = await EnsurePerfilAsync(currentUser);
            var targetPerfil = await ResolveProfileByRouteIdAsync(id);
            if (targetPerfil == null)
            {
                return NotFound(new { message = "Perfil social não encontrado." });
            }

            if (targetPerfil.Id == currentPerfil.Id)
            {
                return BadRequest(new { message = "Não é possível seguir o próprio perfil." });
            }

            var alreadyFollowing = await _context.PerfilSocialFollows.AnyAsync(f =>
                f.SeguidorPerfilId == currentPerfil.Id &&
                f.SeguindoPerfilId == targetPerfil.Id);

            if (!alreadyFollowing)
            {
                _context.PerfilSocialFollows.Add(new PerfilSocialFollow
                {
                    SeguidorPerfilId = currentPerfil.Id,
                    SeguindoPerfilId = targetPerfil.Id,
                    CriadoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id:int}/follow")]
        public async Task<IActionResult> Unfollow(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var currentPerfil = await EnsurePerfilAsync(currentUser);
            var targetPerfil = await ResolveProfileByRouteIdAsync(id);
            if (targetPerfil == null)
            {
                return NotFound(new { message = "Perfil social não encontrado." });
            }

            var follow = await _context.PerfilSocialFollows.FirstOrDefaultAsync(f =>
                f.SeguidorPerfilId == currentPerfil.Id &&
                f.SeguindoPerfilId == targetPerfil.Id);

            if (follow != null)
            {
                _context.PerfilSocialFollows.Remove(follow);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpGet("{id:int}/followers")]
        public async Task<ActionResult<IEnumerable<SocialProfileDto>>> GetFollowers(int id, [FromQuery] int take = 60)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var targetPerfil = await ResolveProfileByRouteIdAsync(id);
            if (targetPerfil == null)
            {
                return NotFound(new { message = "Perfil social não encontrado." });
            }

            var normalizedTake = Math.Clamp(take, 1, 200);
            var followerProfileIds = await _context.PerfilSocialFollows
                .AsNoTracking()
                .Where(f => f.SeguindoPerfilId == targetPerfil.Id)
                .OrderByDescending(f => f.CriadoEm)
                .Select(f => f.SeguidorPerfilId)
                .Take(normalizedTake)
                .ToListAsync();

            var followers = new List<SocialProfileDto>(followerProfileIds.Count);
            foreach (var followerProfileId in followerProfileIds)
            {
                var follower = await _context.PerfisSociais.AsNoTracking().FirstOrDefaultAsync(p => p.Id == followerProfileId);
                if (follower == null)
                {
                    continue;
                }

                followers.Add(await BuildProfileDtoAsync(follower, currentUser.Id));
            }

            return Ok(followers);
        }

        [HttpGet("{id:int}/following")]
        public async Task<ActionResult<IEnumerable<SocialProfileDto>>> GetFollowing(int id, [FromQuery] int take = 60)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var sourcePerfil = await ResolveProfileByRouteIdAsync(id);
            if (sourcePerfil == null)
            {
                return NotFound(new { message = "Perfil social não encontrado." });
            }

            var normalizedTake = Math.Clamp(take, 1, 200);
            var followingProfileIds = await _context.PerfilSocialFollows
                .AsNoTracking()
                .Where(f => f.SeguidorPerfilId == sourcePerfil.Id)
                .OrderByDescending(f => f.CriadoEm)
                .Select(f => f.SeguindoPerfilId)
                .Take(normalizedTake)
                .ToListAsync();

            var following = new List<SocialProfileDto>(followingProfileIds.Count);
            foreach (var followingProfileId in followingProfileIds)
            {
                var followedProfile = await _context.PerfisSociais.AsNoTracking().FirstOrDefaultAsync(p => p.Id == followingProfileId);
                if (followedProfile == null)
                {
                    continue;
                }

                following.Add(await BuildProfileDtoAsync(followedProfile, currentUser.Id));
            }

            return Ok(following);
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

        private async Task<PerfilSocial?> ResolveProfileByRouteIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _context.PerfisSociais.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == id)
                ?? await _context.PerfisSociais.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        private async Task<SocialProfileDto> BuildProfileDtoAsync(PerfilSocial perfil, int currentUserId)
        {
            var followersCount = await _context.PerfilSocialFollows.CountAsync(f => f.SeguindoPerfilId == perfil.Id);
            var followingCount = await _context.PerfilSocialFollows.CountAsync(f => f.SeguidorPerfilId == perfil.Id);
            var postsCount = await _context.SocialPosts.CountAsync(p => p.PerfilSocialId == perfil.Id && p.Ativo && !p.IsDeleted);

            var currentPerfilId = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => p.UserId == currentUserId)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

            var isFollowing = currentPerfilId.HasValue && await _context.PerfilSocialFollows.AnyAsync(f =>
                f.SeguidorPerfilId == currentPerfilId.Value &&
                f.SeguindoPerfilId == perfil.Id);

            var username = string.IsNullOrWhiteSpace(perfil.Username)
                ? $"user{perfil.UserId}"
                : perfil.Username!;

            return new SocialProfileDto
            {
                Id = perfil.Id,
                UserId = perfil.UserId,
                NomeExibicao = perfil.NomeExibicao,
                Username = $"@{username.TrimStart('@')}",
                Bio = perfil.Bio,
                FotoPerfilUrl = SocialImagemHelper.NormalizePublicImageUrl(perfil.FotoPerfilUrl, DefaultProfileImage),
                TipoPerfil = perfil.TipoPerfil,
                Cidade = perfil.Cidade,
                Instagram = perfil.Instagram,
                Site = perfil.Site,
                PostsCount = postsCount,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                IsFollowing = isFollowing,
                AtualizadoEm = perfil.AtualizadoEm
            };
        }
    }
}
