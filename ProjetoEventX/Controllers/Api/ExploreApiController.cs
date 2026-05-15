using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Social;
using ProjetoEventX.Helpers;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/explore")]
    public class ExploreApiController : ControllerBase
    {
        private const string DefaultProfileImage = "/uploads/social/defaults/default-profile.svg";
        private const string DefaultPostImage = "/uploads/social/defaults/default-post.svg";

        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExploreApiController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<ExploreResponseDto>> GetExplore([FromQuery] int take = 24)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var normalizedTake = Math.Clamp(take, 6, 80);
            var trendingPosts = await GetTrendingPostsAsync(currentUser.Id, normalizedTake);
            var trendingProfiles = await GetTrendingProfilesAsync(currentUser.Id, 20);

            var categories = trendingPosts
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .Cast<string>()
                .ToArray();

            var recommended = trendingPosts
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.LikesCount)
                .Take(12)
                .ToList();

            return Ok(new ExploreResponseDto
            {
                Categories = categories,
                TrendingPosts = trendingPosts,
                TrendingProfiles = trendingProfiles,
                RecommendedPosts = recommended
            });
        }

        [HttpGet("trending")]
        public async Task<ActionResult<IEnumerable<SocialPostSummaryDto>>> GetTrending([FromQuery] int take = 24)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var posts = await GetTrendingPostsAsync(currentUser.Id, Math.Clamp(take, 6, 80));
            return Ok(posts);
        }

        [HttpGet("profiles")]
        public async Task<ActionResult<IEnumerable<SocialProfileDto>>> GetTrendingProfiles([FromQuery] int take = 20)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var profiles = await GetTrendingProfilesAsync(currentUser.Id, Math.Clamp(take, 5, 60));
            return Ok(profiles);
        }

        private async Task<List<SocialPostSummaryDto>> GetTrendingPostsAsync(int currentUserId, int take)
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var posts = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo && !p.IsDeleted && !p.IsArchived)
                .Where(p => p.CriadoEm >= sevenDaysAgo || p.IsPinned)
                .OrderByDescending(p => p.IsPinned)
                .ThenByDescending(p => p.Curtidas.Count * 3 + p.Comentarios.Count(c => c.Ativo) * 2)
                .ThenByDescending(p => p.CriadoEm)
                .Take(take)
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
                    IsLikedByCurrentUser = p.Curtidas.Any(c => c.UserId == currentUserId),
                    IsSavedByCurrentUser = p.Salvos.Any(s => s.UserId == currentUserId)
                })
                .ToListAsync();

            return posts;
        }

        private async Task<List<SocialProfileDto>> GetTrendingProfilesAsync(int currentUserId, int take)
        {
            var currentPerfilId = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => p.UserId == currentUserId)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

            var profiles = await _context.PerfisSociais
                .AsNoTracking()
                .OrderByDescending(p => p.Posts.Count(sp => sp.Ativo && !sp.IsDeleted))
                .ThenByDescending(p => p.AtualizadoEm)
                .Take(take)
                .Select(p => new SocialProfileDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    NomeExibicao = p.NomeExibicao,
                    Username = $"@{(string.IsNullOrWhiteSpace(p.Username) ? $"user{p.UserId}" : p.Username).TrimStart('@')}",
                    Bio = p.Bio,
                    FotoPerfilUrl = SocialImagemHelper.NormalizePublicImageUrl(p.FotoPerfilUrl, DefaultProfileImage),
                    TipoPerfil = p.TipoPerfil,
                    Cidade = p.Cidade,
                    Instagram = p.Instagram,
                    Site = p.Site,
                    PostsCount = p.Posts.Count(sp => sp.Ativo && !sp.IsDeleted),
                    FollowersCount = _context.PerfilSocialFollows.Count(f => f.SeguindoPerfilId == p.Id),
                    FollowingCount = _context.PerfilSocialFollows.Count(f => f.SeguidorPerfilId == p.Id),
                    IsFollowing = currentPerfilId.HasValue && _context.PerfilSocialFollows.Any(f => f.SeguidorPerfilId == currentPerfilId && f.SeguindoPerfilId == p.Id),
                    AtualizadoEm = p.AtualizadoEm
                })
                .ToListAsync();

            return profiles;
        }
    }
}