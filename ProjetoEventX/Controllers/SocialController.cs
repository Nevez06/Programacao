using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using ProjetoEventX.Data;
using ProjetoEventX.Helpers;
using ProjetoEventX.Models;
using ProjetoEventX.ViewModels.Social;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class SocialController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private static readonly string[] ExtensoesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
        private const long TamanhoMaximoUpload = 8 * 1024 * 1024;

        private const string PastaUploadPosts = "uploads/social/posts";
        private const string PastaUploadPerfis = "uploads/social/perfis";
        private const string PastaUploadStatus = "uploads/social/status";
        private const string PastaUploadStories = "uploads/stories";
        private const string FotoPerfilPadrao = "/uploads/social/defaults/default-profile.svg";
        private const string ImagemPostPadrao = "/uploads/social/defaults/default-post.svg";
        private static readonly HashSet<string> TiposReacaoPermitidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "like", "fire", "heart", "clap", "wow"
        };
        private static readonly Regex MentionRegex = new(@"@([a-zA-Z0-9._-]{2,40})", RegexOptions.Compiled);
        private static readonly string[] CategoriasExplorar =
        [
            "casamento", "aniversário", "corporativo", "formatura", "show", "infantil",
            "decoração", "buffet", "fotografia"
        ];
        private static readonly string[] ExploreBlockedTokens =
        [
            "login","signin","auth","autentic","admin","dashboard","painel","settings","config","swagger",
            "form","formulario","interface","sistema","interno","screen","screenshot","captura","print",
            "placeholder","default","mock","wireframe","crud","api","endpoint","schema","database"
        ];
        private static readonly string[] ExploreStrongCategoryTokens =
        [
            "casamento","decor","fotografia","festa","evento","show","anivers","corporativo","mood","inspira","premium","editorial"
        ];

        public SocialController(EventXContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        private async Task<ApplicationUser?> ObterUsuarioAtualAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        private async Task<PerfilSocial> GarantirPerfilSocialAsync(ApplicationUser user)
        {
            var perfil = await _context.PerfisSociais.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (perfil != null)
            {
                return perfil;
            }

            perfil = new PerfilSocial
            {
                UserId = user.Id,
                NomeExibicao = user.UserName ?? $"Usuário {user.Id}",
                Username = user.UserName,
                TipoPerfil = user.TipoUsuario ?? "Convidado",
                CriadoEm = DateTime.UtcNow,
                AtualizadoEm = DateTime.UtcNow,
                FotoPerfilUrl = FotoPerfilPadrao
            };

            _context.PerfisSociais.Add(perfil);
            await _context.SaveChangesAsync();
            return perfil;
        }

        private static string ResolverNomeExibicao(PerfilSocial? perfil, ApplicationUser? user)
        {
            if (!string.IsNullOrWhiteSpace(perfil?.NomeExibicao))
            {
                return perfil.NomeExibicao;
            }

            if (!string.IsNullOrWhiteSpace(user?.UserName))
            {
                return user.UserName;
            }

            return string.IsNullOrWhiteSpace(user?.Email) ? "Participante" : user.Email;
        }

        private static string ResolverFotoPerfil(PerfilSocial? perfil)
        {
            return NormalizarUrlImagemPerfil(perfil?.FotoPerfilUrl);
        }

        private static string NormalizarUrlImagemPost(string? imagemUrl)
            => SocialImagemHelper.NormalizePublicImageUrl(imagemUrl, ImagemPostPadrao);

        private static string NormalizarUrlImagemPerfil(string? imagemUrl)
            => SocialImagemHelper.NormalizePublicImageUrl(imagemUrl, FotoPerfilPadrao);

        private static string? NormalizarUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var limpo = username.Trim().TrimStart('@');
            return string.IsNullOrWhiteSpace(limpo) ? null : $"@{limpo.ToLowerInvariant()}";
        }

        private static string ResolverIdentidadePrincipal(PerfilSocial perfil, ApplicationUser? usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario?.UserName))
            {
                return usuario.UserName!;
            }

            if (!string.IsNullOrWhiteSpace(perfil.NomeExibicao))
            {
                return perfil.NomeExibicao;
            }

            if (!string.IsNullOrWhiteSpace(usuario?.Email))
            {
                return usuario.Email!;
            }

            return $"perfil.{perfil.Id}";
        }

        private static decimal CalcularBonusRecencia(DateTime criadoEmUtc)
        {
            var idade = DateTime.UtcNow - criadoEmUtc;
            if (idade.TotalHours <= 2) return 20m;
            if (idade.TotalHours <= 6) return 10m;
            return 0m;
        }

        private static SocialStatusItemViewModel StoryToStatusItemViewModel(
            Story story,
            PerfilSocial perfil,
            bool visualizado,
            int totalViews,
            int totalReactions,
            Dictionary<string, int> reactionsByType,
            bool isOwner,
            string? userReaction,
            List<StoryMentionItemViewModel>? mentions = null)
        {
            var engagementScore = (totalViews * 1) + (totalReactions * 3);
            var rankingScore = (totalViews * 1m) + (totalReactions * 4m) + CalcularBonusRecencia(story.CreatedAt);

            return new SocialStatusItemViewModel
            {
                Id = story.Id,
                PerfilId = perfil.Id,
                UserId = int.TryParse(story.UserId, out var uid) ? uid : 0,
                NomePerfil = string.IsNullOrWhiteSpace(perfil.NomeExibicao) ? "Participante" : perfil.NomeExibicao,
                FotoPerfilUrl = NormalizarUrlImagemPerfil(perfil.FotoPerfilUrl),
                ImagemUrl = SocialImagemHelper.NormalizePublicImageUrl(story.MediaUrl, ImagemPostPadrao),
                TextoOverlay = string.IsNullOrWhiteSpace(story.TextOverlay) ? null : story.TextOverlay,
                CriadoEm = story.CreatedAt,
                Visualizado = visualizado,
                SequenciaNoPerfil = 1,
                TotalNoPerfil = 1,
                EventoId = story.EventoId,
                NomeEvento = story.Evento?.NomeEvento,
                TotalViews = totalViews,
                TotalReactions = totalReactions,
                EngagementScore = engagementScore,
                RankingScore = rankingScore,
                SharedPostId = story.SharedPostId,
                SharedPostImageUrl = story.SharedPost != null ? NormalizarUrlImagemPost(story.SharedPost.ImagemUrl) : null,
                SharedPostCaption = story.SharedPost?.Legenda,
                IsOwner = isOwner,
                UserReaction = userReaction,
                CanReply = !isOwner,
                Mentions = mentions ?? new List<StoryMentionItemViewModel>(),
                ReactionsByType = reactionsByType
            };
        }

        private async Task CriarNotificacaoSocialAsync(int userId, string titulo, string mensagem, string tipo, string? link = null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = titulo,
                Message = mensagem,
                Type = tipo,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Link = link
            });
            await _context.SaveChangesAsync();
        }

        private static string TempoRelativo(DateTime dataUtc)
        {
            var diff = DateTime.UtcNow - dataUtc;
            if (diff.TotalMinutes < 1) return "agora";
            if (diff.TotalHours < 1) return $"{Math.Max(1, (int)diff.TotalMinutes)} min";
            if (diff.TotalDays < 1) return $"{Math.Max(1, (int)diff.TotalHours)} h";
            return $"{Math.Max(1, (int)diff.TotalDays)} d";
        }

        private static bool PostVisivelPublicamente(SocialPost p)
            => p.Ativo && !p.IsArchived && !p.IsDeleted;

        private static bool PostVisivelParaFeed(FeedPostViewModel p)
            => !p.IsArchived;

        private async Task<List<SocialStatusItemViewModel>> ObterStoriesAtivosAsync(int? usuarioAtualId)
        {
            var agora = DateTime.UtcNow;
            var stories = await _context.Stories
                .AsNoTracking()
                .Where(s => s.ExpireAt > agora && s.Ativo)
                .Include(s => s.Evento)
                .Include(s => s.SharedPost)
                .OrderByDescending(s => s.CreatedAt)
                .Take(32)
                .ToListAsync();

            if (stories.Count == 0)
            {
                return new List<SocialStatusItemViewModel>();
            }

            var userIds = stories.Select(s => s.UserId).Distinct().ToList();
            var userIdsInt = userIds
                .Select(x => int.TryParse(x, out var parsed) ? parsed : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();
            var perfis = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => userIdsInt.Contains(p.UserId))
                .ToListAsync();

            var perfisPorUser = perfis.ToDictionary(p => p.UserId.ToString(), p => p);
            var storyIds = stories.Select(s => s.Id).ToList();

            var vistos = usuarioAtualId.HasValue
                ? await _context.StoryViews
                    .AsNoTracking()
                    .Where(v => v.UserId == usuarioAtualId.Value.ToString() && storyIds.Contains(v.StoryId))
                    .Select(v => v.StoryId)
                    .ToHashSetAsync()
                : new HashSet<int>();

            var viewCounts = await _context.StoryViews
                .AsNoTracking()
                .Where(v => storyIds.Contains(v.StoryId))
                .GroupBy(v => v.StoryId)
                .Select(g => new { StoryId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.StoryId, x => x.Total);

            var reactions = await _context.StoryReactions
                .AsNoTracking()
                .Where(r => storyIds.Contains(r.StoryId))
                .ToListAsync();

            var mentionsByStory = await _context.StoryMentions
                .AsNoTracking()
                .Where(m => storyIds.Contains(m.StoryId))
                .GroupBy(m => m.StoryId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(x => new StoryMentionItemViewModel
                    {
                        PerfilId = x.MentionedPerfilId,
                        MentionText = x.MentionText
                    }).ToList());

            var reactionCounts = reactions
                .GroupBy(r => r.StoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            var reactionSummary = reactions
                .GroupBy(r => r.StoryId)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(x => x.ReactionType.ToLowerInvariant())
                        .ToDictionary(x => x.Key, x => x.Count()));

            var userReactions = usuarioAtualId.HasValue
                ? reactions
                    .Where(r => r.UserId == usuarioAtualId.Value.ToString())
                    .GroupBy(r => r.StoryId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First().ReactionType.ToLowerInvariant())
                : new Dictionary<int, string>();

            var result = new List<SocialStatusItemViewModel>();
            foreach (var story in stories)
            {
                if (!perfisPorUser.TryGetValue(story.UserId, out var perfil))
                {
                    continue;
                }
                result.Add(StoryToStatusItemViewModel(
                    story,
                    perfil,
                    vistos.Contains(story.Id),
                    viewCounts.TryGetValue(story.Id, out var totalViews) ? totalViews : 0,
                    reactionCounts.TryGetValue(story.Id, out var totalReactions) ? totalReactions : 0,
                    reactionSummary.TryGetValue(story.Id, out var grouped) ? grouped : new Dictionary<string, int>(),
                    usuarioAtualId.HasValue && int.TryParse(story.UserId, out var ownerId) && ownerId == usuarioAtualId.Value,
                    userReactions.TryGetValue(story.Id, out var reaction) ? reaction : null,
                    mentionsByStory.TryGetValue(story.Id, out var mentions) ? mentions : new List<StoryMentionItemViewModel>()));
            }

            return result
                .OrderBy(s => s.Visualizado)
                .ThenByDescending(s => s.RankingScore)
                .ThenByDescending(s => s.CriadoEm)
                .ToList();
        }

        private async Task<List<SelectListItem>> ObterEventosSelectListAsync()
        {
            return await _context.Eventos
                .OrderByDescending(e => e.DataEvento)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.NomeEvento
                })
                .ToListAsync();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Feed(string ordem = "recentes")
        {
            var usuarioAtual = await ObterUsuarioAtualAsync();
            var usuarioId = usuarioAtual?.Id;
            PerfilSocial? perfilAtual = null;
            if (usuarioAtual != null)
            {
                perfilAtual = await GarantirPerfilSocialAsync(usuarioAtual);
            }

            var ordemNormalizada = string.Equals(ordem, "populares", StringComparison.OrdinalIgnoreCase) ? "populares" : "recentes";
            var seteDiasAtras = DateTime.UtcNow.AddDays(-7);
            var queryPosts = _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo && !p.IsArchived && !p.IsDeleted)
                .Where(p => !string.IsNullOrWhiteSpace(p.ImagemUrl))
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    AutorUserId = p.UserId,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : "Participante"),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? NormalizarUrlImagemPerfil(p.PerfilSocial.FotoPerfilUrl)
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = NormalizarUrlImagemPost(p.ImagemUrl),
                    Categoria = p.Categoria,
                    TipoConteudo = p.TipoConteudo,
                    Localizacao = p.Localizacao,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    HideLikesCount = p.HideLikesCount,
                    HideSharesCount = p.HideSharesCount,
                    CommentsEnabled = p.CommentsEnabled,
                    IsPinned = p.IsPinned,
                    IsArchived = p.IsArchived,
                    UsuarioCurtiu = usuarioId.HasValue && p.Curtidas.Any(c => c.UserId == usuarioId.Value),
                    UsuarioSalvou = usuarioId.HasValue && p.Salvos.Any(s => s.UserId == usuarioId.Value),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId,
                    TipoPerfil = p.PerfilSocial != null ? p.PerfilSocial.TipoPerfil : (p.User != null ? p.User.TipoUsuario ?? "Convidado" : "Convidado"),
                    Cidade = p.PerfilSocial != null ? p.PerfilSocial.Cidade : null,
                    IsOwner = usuarioId.HasValue && p.UserId == usuarioId.Value,
                    PostRelacionadoAoUsuario = usuarioId.HasValue && (p.UserId == usuarioId.Value || (p.Evento != null && p.Evento.OrganizadorId == usuarioId.Value))
                });

            queryPosts = ordemNormalizada == "populares"
                ? queryPosts.OrderByDescending(p => p.IsPinned)
                    .ThenBy(p => p.IsPinned ? p.PostId : int.MaxValue)
                    .ThenByDescending(p => p.PostRelacionadoAoUsuario)
                    .ThenByDescending(p => p.TotalCurtidas)
                    .ThenByDescending(p => p.TotalComentarios)
                    .ThenByDescending(p => p.DataCriacao)
                : queryPosts.OrderByDescending(p => p.IsPinned)
                    .ThenByDescending(p => p.PostRelacionadoAoUsuario)
                    .ThenByDescending(p => p.DataCriacao);

            var posts = await queryPosts.Take(40).ToListAsync();
            var postIds = posts.Select(p => p.PostId).ToList();
            if (postIds.Count > 0)
            {
                var comentariosRecentes = await _context.SocialComentarios
                    .AsNoTracking()
                    .Where(c => c.Ativo && postIds.Contains(c.PostId))
                    .OrderByDescending(c => c.CriadoEm)
                    .Select(c => new
                    {
                        c.PostId,
                        NomeAutor = !string.IsNullOrWhiteSpace(c.PerfilSocial != null ? c.PerfilSocial.NomeExibicao : null)
                            ? c.PerfilSocial!.NomeExibicao
                            : (!string.IsNullOrWhiteSpace(c.User != null ? c.User.UserName : null) ? c.User!.UserName! : (c.User != null ? c.User.Email ?? "Participante" : "Participante")),
                        c.Texto
                    })
                    .ToListAsync();

                var previewsPorPost = comentariosRecentes
                    .GroupBy(c => c.PostId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Take(2).Select(c => new FeedComentarioPreviewViewModel
                        {
                            NomeAutor = c.NomeAutor,
                            Texto = c.Texto
                        }).ToList());

                foreach (var post in posts)
                {
                    if (previewsPorPost.TryGetValue(post.PostId, out var preview))
                    {
                        post.ComentariosPreview = preview;
                    }
                }
            }

            var fornecedoresEmAlta = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => p.TipoPerfil == "Fornecedor")
                .OrderByDescending(p => p.Posts.Count)
                .Take(6)
                .ToListAsync();

            var categoriasTendencia = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo && p.CriadoEm >= seteDiasAtras && !string.IsNullOrWhiteSpace(p.Categoria))
                .GroupBy(p => p.Categoria!)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(6)
                .ToListAsync();

            var eventoMaisCurtidoSemana = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo && p.CriadoEm >= seteDiasAtras && p.EventoId.HasValue)
                .OrderByDescending(p => p.Curtidas.Count)
                .ThenByDescending(p => p.CriadoEm)
                .Select(p => p.Evento != null ? p.Evento.NomeEvento : null)
                .FirstOrDefaultAsync();

            var storiesAtivos = await ObterStoriesAtivosAsync(usuarioId);

            var model = new FeedSocialViewModel
            {
                Posts = posts,
                Stories = storiesAtivos,
                StoriesEmAlta = storiesAtivos
                    .OrderByDescending(s => s.RankingScore)
                    .Take(10)
                    .ToList(),
                PerfisDestaque = await _context.PerfisSociais.AsNoTracking().OrderByDescending(p => p.Posts.Count).Take(8).ToListAsync(),
                EventosEmAlta = await _context.Eventos.AsNoTracking().OrderByDescending(e => e.DataEvento).Take(6).ToListAsync(),
                PerfilAtual = perfilAtual,
                OrdenacaoAtual = ordemNormalizada,
                FornecedoresEmAlta = fornecedoresEmAlta,
                CategoriasTendencia = categoriasTendencia,
                InsightSemana = string.IsNullOrWhiteSpace(eventoMaisCurtidoSemana)
                    ? "Publique bastidores para impulsionar seus eventos da semana."
                    : $"Evento mais curtido da semana: {eventoMaisCurtidoSemana}"
            };

            if (model.PerfilAtual != null)
            {
                model.PerfilAtual.FotoPerfilUrl = NormalizarUrlImagemPerfil(model.PerfilAtual.FotoPerfilUrl);
            }

            foreach (var perfil in model.PerfisDestaque)
            {
                perfil.FotoPerfilUrl = NormalizarUrlImagemPerfil(perfil.FotoPerfilUrl);
            }

            foreach (var perfil in model.FornecedoresEmAlta)
            {
                perfil.FotoPerfilUrl = NormalizarUrlImagemPerfil(perfil.FotoPerfilUrl);
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Explorar(string? busca = null, string? categoria = null)
        {
            var usuarioAtual = await ObterUsuarioAtualAsync();
            var buscaNormalizada = string.IsNullOrWhiteSpace(busca) ? null : busca.Trim();
            var categoriaNormalizada = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim().ToLowerInvariant();

            var recentes = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo)
                .OrderByDescending(p => p.CriadoEm)
                .Take(24)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : "Participante"),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? NormalizarUrlImagemPerfil(p.PerfilSocial.FotoPerfilUrl)
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = NormalizarUrlImagemPost(p.ImagemUrl),
                    Categoria = p.Categoria,
                    TipoConteudo = p.TipoConteudo,
                    Localizacao = p.Localizacao,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    UsuarioSalvou = usuarioAtual != null && p.Salvos.Any(s => s.UserId == usuarioAtual.Id),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId,
                    TipoPerfil = p.PerfilSocial != null ? p.PerfilSocial.TipoPerfil : (p.User != null ? p.User.TipoUsuario ?? "Convidado" : "Convidado"),
                    Cidade = p.PerfilSocial != null ? p.PerfilSocial.Cidade : null
                })
                .ToListAsync();

            var populares = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo)
                .OrderByDescending(p => p.Curtidas.Count)
                .ThenByDescending(p => p.Comentarios.Count(c => c.Ativo))
                .ThenByDescending(p => p.CriadoEm)
                .Take(24)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : "Participante"),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? NormalizarUrlImagemPerfil(p.PerfilSocial.FotoPerfilUrl)
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = NormalizarUrlImagemPost(p.ImagemUrl),
                    Categoria = p.Categoria,
                    TipoConteudo = p.TipoConteudo,
                    Localizacao = p.Localizacao,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    UsuarioSalvou = usuarioAtual != null && p.Salvos.Any(s => s.UserId == usuarioAtual.Id),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId,
                    TipoPerfil = p.PerfilSocial != null ? p.PerfilSocial.TipoPerfil : (p.User != null ? p.User.TipoUsuario ?? "Convidado" : "Convidado"),
                    Cidade = p.PerfilSocial != null ? p.PerfilSocial.Cidade : null
                })
                .ToListAsync();

            var eventosDestaque = await _context.Eventos
                .AsNoTracking()
                .OrderByDescending(e => _context.SocialPosts.Count(p => p.Ativo && p.EventoId == e.Id))
                .ThenByDescending(e => e.DataEvento)
                .Take(10)
                .Select(e => new ExplorarEventoCardViewModel
                {
                    EventoId = e.Id,
                    NomeEvento = e.NomeEvento,
                    TipoEvento = e.TipoEvento,
                    DataEvento = e.DataEvento,
                    ImagemCapa = string.IsNullOrWhiteSpace(e.ImagemCapa) ? ImagemPostPadrao : e.ImagemCapa,
                    TotalPostsRelacionados = _context.SocialPosts.Count(p => p.Ativo && p.EventoId == e.Id)
                })
                .ToListAsync();

            var fornecedoresPopulares = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => p.TipoPerfil == "Fornecedor")
                .OrderByDescending(p => p.Posts.Count)
                .Take(10)
                .Select(p => new ExplorarPerfilCardViewModel
                {
                    PerfilId = p.Id,
                    NomeExibicao = p.NomeExibicao,
                    TipoPerfil = p.TipoPerfil,
                    Cidade = p.Cidade,
                    Bio = p.Bio,
                    FotoPerfilUrl = NormalizarUrlImagemPerfil(p.FotoPerfilUrl),
                    TotalPosts = p.Posts.Count
                })
                .ToListAsync();

            var organizadoresDestaque = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => p.TipoPerfil == "Organizador")
                .OrderByDescending(p => p.Posts.Count)
                .Take(10)
                .Select(p => new ExplorarPerfilCardViewModel
                {
                    PerfilId = p.Id,
                    NomeExibicao = p.NomeExibicao,
                    TipoPerfil = p.TipoPerfil,
                    Cidade = p.Cidade,
                    Bio = p.Bio,
                    FotoPerfilUrl = NormalizarUrlImagemPerfil(p.FotoPerfilUrl),
                    TotalPosts = p.Posts.Count
                })
                .ToListAsync();

            var templatesEmAlta = await _context.TemplatesConvites
                .AsNoTracking()
                .Where(t => t.Ativo)
                .OrderByDescending(t => t.UpdatedAt)
                .Take(12)
                .Select(t => new ExplorarTemplateCardViewModel
                {
                    TemplateId = t.Id,
                    EventoId = t.EventoId,
                    NomeTemplate = t.Nome,
                    Categoria = string.IsNullOrWhiteSpace(t.Estilo) ? "template" : t.Estilo!,
                    Estilo = string.IsNullOrWhiteSpace(t.Estilo) ? "editor visual" : t.Estilo!,
                    ThumbnailUrl = t.Evento != null ? NormalizarUrlImagemPost(t.Evento.ImagemCapa) : ImagemPostPadrao,
                    AtualizadoEm = t.UpdatedAt,
                    LinkEditor = t.EventoId.HasValue
                        ? Url.Action("Editor", "Convite", new { eventoId = t.EventoId.Value, templateId = t.Id }) ?? "#"
                        : "#"
                })
                .ToListAsync();

            var storiesBase = await _context.Stories
                .AsNoTracking()
                .Where(s => s.Ativo && s.ExpireAt > DateTime.UtcNow)
                .OrderByDescending(s => s.CreatedAt)
                .Take(20)
                .Select(s => new
                {
                    s.Id,
                    s.CreatedAt,
                    s.MediaUrl,
                    s.UserId,
                    PerfilId = s.PerfilSocialId
                })
                .ToListAsync();

            var storiesIds = storiesBase.Select(s => s.Id).ToList();
            var storiesViews = await _context.StoryViews
                .AsNoTracking()
                .Where(v => storiesIds.Contains(v.StoryId))
                .GroupBy(v => v.StoryId)
                .Select(g => new { StoryId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.StoryId, x => x.Total);
            var storiesReactions = await _context.StoryReactions
                .AsNoTracking()
                .Where(r => storiesIds.Contains(r.StoryId))
                .GroupBy(r => r.StoryId)
                .Select(g => new { StoryId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.StoryId, x => x.Total);

            var perfisStoriesIds = storiesBase
                .Select(s => int.TryParse(s.UserId, out var uid) ? uid : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();
            var perfisStories = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => perfisStoriesIds.Contains(p.UserId))
                .ToListAsync();
            var perfilByUserId = perfisStories.ToDictionary(p => p.UserId, p => p);

            var storiesDestaque = storiesBase
                .Select(s =>
                {
                    perfilByUserId.TryGetValue(int.TryParse(s.UserId, out var uid) ? uid : -1, out var perfilStory);
                    var views = storiesViews.TryGetValue(s.Id, out var totalViews) ? totalViews : 0;
                    var reacoes = storiesReactions.TryGetValue(s.Id, out var totalReacoes) ? totalReacoes : 0;
                    var perfilId = s.PerfilId ?? perfilStory?.Id ?? 0;
                    return new ExplorarStoryCardViewModel
                    {
                        StoryId = s.Id,
                        PerfilId = perfilId,
                        NomePerfil = perfilStory?.NomeExibicao ?? "Perfil",
                        FotoPerfilUrl = NormalizarUrlImagemPerfil(perfilStory?.FotoPerfilUrl),
                        CapaUrl = SocialImagemHelper.NormalizePublicImageUrl(s.MediaUrl, ImagemPostPadrao),
                        CriadoEm = s.CreatedAt,
                        Visualizacoes = views,
                        Reacoes = reacoes
                    };
                })
                .Where(s => s.PerfilId > 0)
                .OrderByDescending(s => (s.Visualizacoes * 2) + (s.Reacoes * 3))
                .ThenByDescending(s => s.CriadoEm)
                .Take(12)
                .ToList();

            var categoriasPadrao = new List<string>
            {
                "casamento", "aniversário", "corporativo", "formatura", "show", "infantil",
                "buffet", "decoração", "fotografia", "música", "fornecedores", "templates", "stories"
            };

            var sugestoesBusca = recentes
                .SelectMany(p => new[] { p.NomeAutor, p.NomeEvento, p.Categoria, p.Cidade })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .ToList();

            IEnumerable<FeedPostViewModel> recentesFiltrados = recentes;
            IEnumerable<FeedPostViewModel> popularesFiltrados = populares;
            IEnumerable<ExplorarEventoCardViewModel> eventosFiltrados = eventosDestaque;
            IEnumerable<ExplorarPerfilCardViewModel> fornecedoresFiltrados = fornecedoresPopulares;
            IEnumerable<ExplorarPerfilCardViewModel> organizadoresFiltrados = organizadoresDestaque;
            IEnumerable<ExplorarTemplateCardViewModel> templatesFiltrados = templatesEmAlta;
            IEnumerable<ExplorarStoryCardViewModel> storiesFiltrados = storiesDestaque;

            if (!string.IsNullOrWhiteSpace(buscaNormalizada))
            {
                var termo = buscaNormalizada.ToLowerInvariant();
                recentesFiltrados = recentesFiltrados.Where(p =>
                    (!string.IsNullOrWhiteSpace(p.NomeEvento) && p.NomeEvento!.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(p.NomeAutor) && p.NomeAutor.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(p.Categoria) && p.Categoria!.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(p.Cidade) && p.Cidade!.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(p.Legenda) && p.Legenda.ToLowerInvariant().Contains(termo)));
                popularesFiltrados = popularesFiltrados.Where(p =>
                    (!string.IsNullOrWhiteSpace(p.NomeEvento) && p.NomeEvento!.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(p.NomeAutor) && p.NomeAutor.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(p.Categoria) && p.Categoria!.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(p.Cidade) && p.Cidade!.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(p.Legenda) && p.Legenda.ToLowerInvariant().Contains(termo)));
                eventosFiltrados = eventosFiltrados.Where(e =>
                    e.NomeEvento.ToLowerInvariant().Contains(termo)
                    || e.TipoEvento.ToLowerInvariant().Contains(termo));
                fornecedoresFiltrados = fornecedoresFiltrados.Where(f =>
                    f.NomeExibicao.ToLowerInvariant().Contains(termo)
                    || (!string.IsNullOrWhiteSpace(f.Cidade) && f.Cidade!.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(f.Bio) && f.Bio!.ToLowerInvariant().Contains(termo)));
                organizadoresFiltrados = organizadoresFiltrados.Where(f =>
                    f.NomeExibicao.ToLowerInvariant().Contains(termo)
                    || (!string.IsNullOrWhiteSpace(f.Cidade) && f.Cidade!.ToLowerInvariant().Contains(termo))
                    || (!string.IsNullOrWhiteSpace(f.Bio) && f.Bio!.ToLowerInvariant().Contains(termo)));
                templatesFiltrados = templatesFiltrados.Where(t =>
                    t.NomeTemplate.ToLowerInvariant().Contains(termo)
                    || t.Categoria.ToLowerInvariant().Contains(termo)
                    || t.Estilo.ToLowerInvariant().Contains(termo));
                storiesFiltrados = storiesFiltrados.Where(s =>
                    s.NomePerfil.ToLowerInvariant().Contains(termo));
            }

            if (!string.IsNullOrWhiteSpace(categoriaNormalizada))
            {
                if (categoriaNormalizada is "stories")
                {
                    recentesFiltrados = Enumerable.Empty<FeedPostViewModel>();
                    popularesFiltrados = Enumerable.Empty<FeedPostViewModel>();
                    eventosFiltrados = Enumerable.Empty<ExplorarEventoCardViewModel>();
                    fornecedoresFiltrados = Enumerable.Empty<ExplorarPerfilCardViewModel>();
                    organizadoresFiltrados = Enumerable.Empty<ExplorarPerfilCardViewModel>();
                    templatesFiltrados = Enumerable.Empty<ExplorarTemplateCardViewModel>();
                }
                else if (categoriaNormalizada is "fornecedores")
                {
                    recentesFiltrados = Enumerable.Empty<FeedPostViewModel>();
                    popularesFiltrados = Enumerable.Empty<FeedPostViewModel>();
                    eventosFiltrados = Enumerable.Empty<ExplorarEventoCardViewModel>();
                    templatesFiltrados = Enumerable.Empty<ExplorarTemplateCardViewModel>();
                    storiesFiltrados = Enumerable.Empty<ExplorarStoryCardViewModel>();
                }
                else if (categoriaNormalizada is "templates")
                {
                    recentesFiltrados = Enumerable.Empty<FeedPostViewModel>();
                    popularesFiltrados = Enumerable.Empty<FeedPostViewModel>();
                    eventosFiltrados = Enumerable.Empty<ExplorarEventoCardViewModel>();
                    fornecedoresFiltrados = Enumerable.Empty<ExplorarPerfilCardViewModel>();
                    organizadoresFiltrados = Enumerable.Empty<ExplorarPerfilCardViewModel>();
                    storiesFiltrados = Enumerable.Empty<ExplorarStoryCardViewModel>();
                }
                else
                {
                    recentesFiltrados = recentesFiltrados.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.Categoria) && p.Categoria!.ToLowerInvariant().Contains(categoriaNormalizada))
                        || (!string.IsNullOrWhiteSpace(p.Legenda) && p.Legenda.ToLowerInvariant().Contains(categoriaNormalizada)));
                    popularesFiltrados = popularesFiltrados.Where(p =>
                        (!string.IsNullOrWhiteSpace(p.Categoria) && p.Categoria!.ToLowerInvariant().Contains(categoriaNormalizada))
                        || (!string.IsNullOrWhiteSpace(p.Legenda) && p.Legenda.ToLowerInvariant().Contains(categoriaNormalizada)));
                    eventosFiltrados = eventosFiltrados.Where(e =>
                        e.TipoEvento.ToLowerInvariant().Contains(categoriaNormalizada)
                        || e.NomeEvento.ToLowerInvariant().Contains(categoriaNormalizada));
                    templatesFiltrados = templatesFiltrados.Where(t =>
                        t.Categoria.ToLowerInvariant().Contains(categoriaNormalizada)
                        || t.Estilo.ToLowerInvariant().Contains(categoriaNormalizada));
                }
            }

            const int loteInicial = 12;
            var agoraUtc = DateTime.UtcNow;
            const decimal scoreMinimo = 52m;

            var postsMisturados = popularesFiltrados
                .Concat(recentesFiltrados)
                .GroupBy(p => p.PostId)
                .Select(g => g.OrderByDescending(x => (x.TotalCurtidas * 3) + (x.TotalComentarios * 2)).ThenByDescending(x => x.DataCriacao).First())
                .ToList();

            var candidatos = new List<ExplorarVisualItemViewModel>();
            candidatos.AddRange(postsMisturados.Select((p, index) => CriarItemExplorarPost(p, index, agoraUtc)));
            candidatos.AddRange(eventosFiltrados.Select((e, index) => CriarItemExplorarEvento(e, index, agoraUtc)));
            candidatos.AddRange(storiesFiltrados.Select((s, index) => CriarItemExplorarStory(s, index, agoraUtc)));
            candidatos.AddRange(fornecedoresFiltrados.Select((f, index) => CriarItemExplorarFornecedor(f, index, agoraUtc)));
            candidatos.AddRange(templatesFiltrados.Select((t, index) => CriarItemExplorarTemplate(t, index, agoraUtc)));

            var candidatosQualificados = candidatos
                .Where(i => IsValidForExploreItem(i) && i.ScoreRelevancia >= scoreMinimo)
                .OrderByDescending(i => i.ScoreRelevancia)
                .ThenByDescending(i => i.ImageQualityScore)
                .ThenByDescending(i => i.DestaqueManual)
                .Take(96)
                .ToList();

            var heroItems = SelecionarHeroItens(candidatosQualificados);
            var mostrarHero = heroItems.Count >= 3 && candidatosQualificados.Count >= 6;
            var feedBase = mostrarHero
                ? candidatosQualificados.Where(i => heroItems.All(h => h.Chave != i.Chave)).ToList()
                : candidatosQualificados;

            var descobertaVisual = MisturarItensExplorar(feedBase).Take(72).ToList();
            var layoutCompacto = descobertaVisual.Count < 6;
            var layoutEditorial = descobertaVisual.Count >= 14;
            AplicarVariantesLayout(descobertaVisual, layoutCompacto);

            var feedRestante = descobertaVisual;
            var inspiracoes = BuildInspiracoes(descobertaVisual);

            var fornecedoresQuality = fornecedoresFiltrados.Where(FornecedorValidoParaExplore).Take(8).ToList();
            var eventosQuality = eventosFiltrados.Where(EventoValidoParaExplore).Take(8).ToList();
            var templatesQuality = templatesFiltrados.Where(TemplateValidoParaExplore).Take(8).ToList();
            var storiesQuality = storiesFiltrados.Where(s => IsImagemExploravel(s.CapaUrl)).Take(10).ToList();
            var recentesQuality = recentesFiltrados.Where(PostValidoParaExplore).Take(18).ToList();
            var popularesQuality = popularesFiltrados.Where(PostValidoParaExplore).Take(18).ToList();

            var model = new ExplorarViewModel
            {
                Busca = buscaNormalizada,
                CategoriaAtiva = categoriaNormalizada,
                PostsRecentes = recentesQuality,
                PostsPopulares = popularesQuality,
                OrganizadoresDestaque = organizadoresFiltrados.Where(FornecedorValidoParaExplore).Take(8).ToList(),
                FornecedoresPopulares = fornecedoresQuality,
                EventosEmAlta = eventosQuality,
                TemplatesEmAlta = templatesQuality,
                StoriesDestaque = storiesQuality,
                Inspiracoes = inspiracoes,
                HeroItems = heroItems,
                MostrarHero = mostrarHero,
                MostrarInspiracoes = inspiracoes.Count >= (layoutCompacto ? 2 : 3),
                MostrarEventos = eventosQuality.Count >= (layoutCompacto ? 2 : 3),
                MostrarFornecedores = fornecedoresQuality.Count >= (layoutCompacto ? 2 : 3),
                MostrarTemplates = templatesQuality.Count >= (layoutCompacto ? 2 : 3),
                MostrarStories = storiesQuality.Count >= 3,
                LayoutCompacto = layoutCompacto,
                LayoutEditorial = layoutEditorial,
                DescobertaVisual = feedRestante,
                DescobertaVisualInicial = feedRestante.Take(loteInicial).ToList(),
                CategoriasDestaque = categoriasPadrao,
                SugestoesBusca = sugestoesBusca,
                TotalPostsRecentes = recentesQuality.Count,
                TotalPostsPopulares = popularesQuality.Count,
                TotalEventosEmAlta = eventosQuality.Count,
                TotalFornecedoresPopulares = fornecedoresQuality.Count,
                TotalTemplatesEmAlta = templatesQuality.Count,
                TotalStoriesDestaque = storiesQuality.Count,
                LoteCarregamento = loteInicial,
                ProximoSkip = loteInicial,
                PodeCarregarMais = feedRestante.Count > loteInicial
            };

            if (model.DescobertaVisualInicial.Count < 8)
            {
                model.PodeCarregarMais = false;
            }
            if (model.DescobertaVisual.Count < 6)
            {
                model.MostrarInspiracoes = false;
                model.MostrarHero = false;
            }
            if (model.HeroItems.Count < 3)
            {
                model.MostrarHero = false;
            }
            if (!model.MostrarHero)
            {
                model.HeroItems.Clear();
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ExplorarMais(string? busca = null, string? categoria = null, int skip = 0, int take = 12)
        {
            var skipSeguro = Math.Max(0, skip);
            var takeSeguro = Math.Clamp(take, 6, 24);
            var resultado = await Explorar(busca, categoria) as ViewResult;
            if (resultado?.Model is not ExplorarViewModel model)
            {
                return PartialView("Partials/_ExploreVisualCards", new List<ExplorarVisualItemViewModel>());
            }

            var lote = model.DescobertaVisual.Skip(skipSeguro).Take(takeSeguro).ToList();
            if (!lote.Any())
            {
                return Content(string.Empty, "text/html; charset=utf-8");
            }

            return PartialView("Partials/_ExploreVisualCards", lote);
        }

        private ExplorarVisualItemViewModel CriarItemExplorarPost(FeedPostViewModel post, int index, DateTime agoraUtc)
        {
            var imageQuality = CalcularImageQualityScore(post.ImagemUrl, post.Categoria, post.TipoConteudo, post.Legenda);
            var bonusVisual = CalcularBonusVisual(post.ImagemUrl, post.Categoria, post.TipoConteudo);
            var visualizacoes = (post.TotalCurtidas * 2) + (post.TotalComentarios * 3);
            var compartilhamentos = post.UsuarioSalvou ? 2 : 0;
            var score = CalcularScoreRelevancia(
                post.ImagemUrl,
                post.DataCriacao,
                post.TotalCurtidas,
                post.TotalComentarios,
                visualizacoes,
                compartilhamentos,
                post.Categoria,
                index % 9 == 0,
                agoraUtc) + bonusVisual + (imageQuality * 0.85m);
            var titulo = string.IsNullOrWhiteSpace(post.NomeEvento) ? post.NomeAutor : post.NomeEvento!;
            var internalFlags = DetectarFlagsConteudoInterno(titulo, post.Categoria, post.Legenda, post.ImagemUrl);

            return new ExplorarVisualItemViewModel
            {
                Tipo = "post",
                Chave = $"post-{post.PostId}",
                Titulo = titulo,
                Subtitulo = post.Categoria,
                ImagemUrl = post.ImagemUrl,
                Link = Url.Action("Post", "Social", new { id = post.PostId }) ?? "#",
                Badge = "post",
                Score = (int)Math.Round(score),
                ScoreRelevancia = score,
                DataCriacao = post.DataCriacao,
                Categoria = post.Categoria,
                TotalCurtidas = post.TotalCurtidas,
                TotalComentarios = post.TotalComentarios,
                TotalVisualizacoes = visualizacoes,
                TotalCompartilhamentos = compartilhamentos,
                PossuiImagemValida = IsImagemExploravel(post.ImagemUrl),
                HasVisualValue = imageQuality >= 42m,
                IsInternal = internalFlags.IsInternal,
                IsAuthScreen = internalFlags.IsAuthScreen,
                IsForm = internalFlags.IsForm,
                IsSystemScreen = internalFlags.IsSystemScreen,
                ImageQualityScore = imageQuality,
                DestaqueManual = index % 9 == 0,
                Destaque = index % 7 == 0,
                IsVideo = !string.IsNullOrWhiteSpace(post.TipoConteudo) && post.TipoConteudo.Contains("video", StringComparison.OrdinalIgnoreCase),
                IsMulti = !string.IsNullOrWhiteSpace(post.Categoria) && post.Categoria.Contains("carrossel", StringComparison.OrdinalIgnoreCase)
            };
        }

        private ExplorarVisualItemViewModel CriarItemExplorarEvento(ExplorarEventoCardViewModel evento, int index, DateTime agoraUtc)
        {
            var imageQuality = CalcularImageQualityScore(evento.ImagemCapa, evento.TipoEvento, "evento", evento.NomeEvento);
            var bonusVisual = CalcularBonusVisual(evento.ImagemCapa, evento.TipoEvento, null);
            var visualizacoes = evento.TotalPostsRelacionados * 6;
            var score = CalcularScoreRelevancia(
                evento.ImagemCapa,
                evento.DataEvento.ToUniversalTime(),
                evento.TotalPostsRelacionados * 2,
                evento.TotalPostsRelacionados,
                visualizacoes,
                0,
                evento.TipoEvento,
                index == 0,
                agoraUtc) + bonusVisual + 8m + (imageQuality * 0.85m);
            var internalFlags = DetectarFlagsConteudoInterno(evento.NomeEvento, evento.TipoEvento, null, evento.ImagemCapa);

            return new ExplorarVisualItemViewModel
            {
                Tipo = "evento",
                Chave = $"evento-{evento.EventoId}",
                Titulo = evento.NomeEvento,
                Subtitulo = $"{evento.TipoEvento} · {evento.DataEvento:dd/MM}",
                ImagemUrl = string.IsNullOrWhiteSpace(evento.ImagemCapa) ? ImagemPostPadrao : evento.ImagemCapa!,
                Link = Url.Action("Evento", "Social", new { eventoId = evento.EventoId }) ?? "#",
                Badge = "evento",
                Score = (int)Math.Round(score),
                ScoreRelevancia = score,
                DataCriacao = evento.DataEvento.ToUniversalTime(),
                Categoria = evento.TipoEvento,
                TotalCurtidas = evento.TotalPostsRelacionados * 2,
                TotalComentarios = evento.TotalPostsRelacionados,
                TotalVisualizacoes = visualizacoes,
                TotalCompartilhamentos = 0,
                PossuiImagemValida = IsImagemExploravel(evento.ImagemCapa),
                HasVisualValue = imageQuality >= 45m,
                IsInternal = internalFlags.IsInternal,
                IsAuthScreen = internalFlags.IsAuthScreen,
                IsForm = internalFlags.IsForm,
                IsSystemScreen = internalFlags.IsSystemScreen,
                ImageQualityScore = imageQuality,
                DestaqueManual = index == 0,
                Destaque = index < 2
            };
        }

        private ExplorarVisualItemViewModel CriarItemExplorarTemplate(ExplorarTemplateCardViewModel template, int index, DateTime agoraUtc)
        {
            var imageQuality = CalcularImageQualityScore(template.ThumbnailUrl, template.Categoria, template.Estilo, template.NomeTemplate);
            var bonusVisual = CalcularBonusVisual(template.ThumbnailUrl, template.Categoria, template.Estilo);
            var visualizacoes = Math.Max(2, 28 - index);
            var score = CalcularScoreRelevancia(
                template.ThumbnailUrl,
                template.AtualizadoEm,
                0,
                0,
                visualizacoes,
                0,
                template.Categoria,
                index == 0,
                agoraUtc) + bonusVisual + (imageQuality * 0.85m);
            var internalFlags = DetectarFlagsConteudoInterno(template.NomeTemplate, template.Categoria, template.Estilo, template.ThumbnailUrl);

            return new ExplorarVisualItemViewModel
            {
                Tipo = "template",
                Chave = $"template-{template.TemplateId}",
                Titulo = template.NomeTemplate,
                Subtitulo = template.Estilo,
                ImagemUrl = template.ThumbnailUrl,
                Link = template.LinkEditor,
                Badge = "template",
                Score = (int)Math.Round(score),
                ScoreRelevancia = score,
                DataCriacao = template.AtualizadoEm,
                Categoria = template.Categoria,
                TotalCurtidas = 0,
                TotalComentarios = 0,
                TotalVisualizacoes = visualizacoes,
                TotalCompartilhamentos = 0,
                PossuiImagemValida = IsImagemExploravel(template.ThumbnailUrl),
                HasVisualValue = imageQuality >= 48m,
                IsInternal = internalFlags.IsInternal,
                IsAuthScreen = internalFlags.IsAuthScreen,
                IsForm = internalFlags.IsForm,
                IsSystemScreen = internalFlags.IsSystemScreen,
                ImageQualityScore = imageQuality,
                DestaqueManual = index == 0,
                Destaque = index == 0
            };
        }

        private ExplorarVisualItemViewModel CriarItemExplorarStory(ExplorarStoryCardViewModel story, int index, DateTime agoraUtc)
        {
            var imageQuality = CalcularImageQualityScore(story.CapaUrl, "stories", null, story.NomePerfil);
            var bonusVisual = CalcularBonusVisual(story.CapaUrl, "stories", null);
            var score = CalcularScoreRelevancia(
                story.CapaUrl,
                story.CriadoEm,
                story.Reacoes,
                0,
                story.Visualizacoes,
                0,
                "stories",
                index == 0,
                agoraUtc) + bonusVisual + (imageQuality * 0.8m);
            var internalFlags = DetectarFlagsConteudoInterno(story.NomePerfil, "stories", null, story.CapaUrl);

            return new ExplorarVisualItemViewModel
            {
                Tipo = "story",
                Chave = $"story-{story.StoryId}",
                Titulo = story.NomePerfil,
                Subtitulo = $"{story.Visualizacoes} visualizações",
                ImagemUrl = story.CapaUrl,
                Link = Url.Action("Perfil", "Social", new { id = story.PerfilId }) ?? "#",
                Badge = "story",
                Score = (int)Math.Round(score),
                ScoreRelevancia = score,
                DataCriacao = story.CriadoEm,
                Categoria = "stories",
                TotalCurtidas = story.Reacoes,
                TotalComentarios = 0,
                TotalVisualizacoes = story.Visualizacoes,
                TotalCompartilhamentos = 0,
                PossuiImagemValida = IsImagemExploravel(story.CapaUrl),
                HasVisualValue = imageQuality >= 40m,
                IsInternal = internalFlags.IsInternal,
                IsAuthScreen = internalFlags.IsAuthScreen,
                IsForm = internalFlags.IsForm,
                IsSystemScreen = internalFlags.IsSystemScreen,
                ImageQualityScore = imageQuality,
                DestaqueManual = index == 0,
                Destaque = index == 0
            };
        }

        private ExplorarVisualItemViewModel CriarItemExplorarFornecedor(ExplorarPerfilCardViewModel fornecedor, int index, DateTime agoraUtc)
        {
            var imageQuality = CalcularImageQualityScore(fornecedor.FotoPerfilUrl, "fornecedores", fornecedor.TipoPerfil, fornecedor.NomeExibicao);
            var bonusVisual = CalcularBonusVisual(fornecedor.FotoPerfilUrl, "fornecedores", fornecedor.TipoPerfil);
            var dataBase = agoraUtc.AddDays(-Math.Min(fornecedor.TotalPosts, 20));
            var visualizacoes = Math.Max(1, fornecedor.TotalPosts * 2);
            var score = CalcularScoreRelevancia(
                fornecedor.FotoPerfilUrl,
                dataBase,
                fornecedor.TotalPosts,
                0,
                visualizacoes,
                0,
                "fornecedores",
                index == 0,
                agoraUtc) + bonusVisual + (imageQuality * 0.75m);
            var internalFlags = DetectarFlagsConteudoInterno(fornecedor.NomeExibicao, fornecedor.TipoPerfil, fornecedor.Bio, fornecedor.FotoPerfilUrl);

            return new ExplorarVisualItemViewModel
            {
                Tipo = "fornecedor",
                Chave = $"fornecedor-{fornecedor.PerfilId}",
                Titulo = fornecedor.NomeExibicao,
                Subtitulo = string.IsNullOrWhiteSpace(fornecedor.Cidade) ? fornecedor.TipoPerfil : $"{fornecedor.TipoPerfil} · {fornecedor.Cidade}",
                ImagemUrl = string.IsNullOrWhiteSpace(fornecedor.FotoPerfilUrl) ? FotoPerfilPadrao : fornecedor.FotoPerfilUrl!,
                Link = Url.Action("Perfil", "Social", new { id = fornecedor.PerfilId }) ?? "#",
                Badge = "fornecedor",
                Score = (int)Math.Round(score),
                ScoreRelevancia = score,
                DataCriacao = dataBase,
                Categoria = "fornecedores",
                TotalCurtidas = fornecedor.TotalPosts,
                TotalComentarios = 0,
                TotalVisualizacoes = visualizacoes,
                TotalCompartilhamentos = 0,
                PossuiImagemValida = IsImagemExploravel(fornecedor.FotoPerfilUrl),
                HasVisualValue = imageQuality >= 46m,
                IsInternal = internalFlags.IsInternal,
                IsAuthScreen = internalFlags.IsAuthScreen,
                IsForm = internalFlags.IsForm,
                IsSystemScreen = internalFlags.IsSystemScreen,
                ImageQualityScore = imageQuality,
                DestaqueManual = index == 0,
                Destaque = index == 0
            };
        }

        private static decimal CalcularScoreRelevancia(
            string? imagemUrl,
            DateTime criadoEmUtc,
            int curtidas,
            int comentarios,
            int visualizacoes,
            int compartilhamentos,
            string? categoria,
            bool destaqueManual,
            DateTime agoraUtc)
        {
            decimal score = 0m;
            if (IsImagemExploravel(imagemUrl)) score += 40m;

            var horas = Math.Abs((agoraUtc - criadoEmUtc).TotalHours);
            if (horas <= 24) score += 25m;
            else if (horas <= 72) score += 15m;
            else if (horas <= 168) score += 8m;

            score += Math.Min(15m, curtidas * 1.2m);
            score += Math.Min(10m, comentarios * 1.8m);
            score += Math.Min(10m, visualizacoes / 4m);
            score += Math.Min(8m, compartilhamentos * 2m);
            if (destaqueManual) score += 20m;
            score += BonusCategoriaExplorar(categoria);

            return score;
        }

        private static decimal BonusCategoriaExplorar(string? categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria)) return 0m;
            var categoriaNorm = categoria.Trim().ToLowerInvariant();
            return categoriaNorm switch
            {
                "casamento" or "corporativo" or "decoração" or "fotografia" => 8m,
                "show" or "aniversário" or "fornecedores" or "templates" or "stories" => 6m,
                _ => 3m
            };
        }

        private static decimal CalcularBonusVisual(string? imagemUrl, string? categoria, string? subtipo)
        {
            if (!IsImagemExploravel(imagemUrl))
            {
                return -30m;
            }

            var categoriaNorm = categoria?.Trim().ToLowerInvariant() ?? string.Empty;
            var subtipoNorm = subtipo?.Trim().ToLowerInvariant() ?? string.Empty;
            decimal bonus = 0m;

            if (categoriaNorm.Contains("casamento") || categoriaNorm.Contains("decoração") || categoriaNorm.Contains("fotografia"))
            {
                bonus += 8m;
            }
            if (categoriaNorm.Contains("corporativo") || categoriaNorm.Contains("show") || categoriaNorm.Contains("evento"))
            {
                bonus += 6m;
            }
            if (subtipoNorm.Contains("video"))
            {
                bonus += 4m;
            }
            if (subtipoNorm.Contains("editor"))
            {
                bonus += 2m;
            }

            return bonus;
        }

        private static decimal CalcularImageQualityScore(string? imagemUrl, string? categoria, string? subtipo, string? texto)
        {
            if (!IsImagemExploravel(imagemUrl))
            {
                return 0m;
            }

            decimal score = 55m;
            var url = (imagemUrl ?? string.Empty).ToLowerInvariant();
            var categoriaNorm = (categoria ?? string.Empty).ToLowerInvariant();
            var subtipoNorm = (subtipo ?? string.Empty).ToLowerInvariant();
            var textoNorm = (texto ?? string.Empty).ToLowerInvariant();

            if (url.Contains("/uploads/social/posts/") || url.Contains("/uploads/stories/")) score += 14m;
            if (url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") || url.EndsWith(".webp")) score += 8m;
            if (ExploreStrongCategoryTokens.Any(t => categoriaNorm.Contains(t) || subtipoNorm.Contains(t))) score += 12m;
            if (!string.IsNullOrWhiteSpace(textoNorm) && textoNorm.Length is >= 10 and <= 120) score += 5m;
            if (ContemTermoBloqueado(categoriaNorm) || ContemTermoBloqueado(subtipoNorm) || ContemTermoBloqueado(textoNorm)) score -= 40m;
            if (url.Contains("thumb") || url.Contains("preview")) score -= 4m;
            if (url.Contains("template") && !categoriaNorm.Contains("template")) score -= 3m;

            return Math.Clamp(score, 0m, 100m);
        }

        private static bool IsImagemExploravel(string? imagemUrl)
        {
            if (string.IsNullOrWhiteSpace(imagemUrl)) return false;
            var url = imagemUrl.Trim().ToLowerInvariant();
            if (url.Contains("default-post") || url.Contains("default-profile") || url.Contains("placeholder")) return false;
            if (url.Contains("login") || url.Contains("dashboard") || url.Contains("admin") || url.Contains("interno")) return false;
            if (url.Contains("account") || url.Contains("signin") || url.Contains("auth")) return false;
            if (url.Contains("form") || url.Contains("formulario") || url.Contains("painel")) return false;
            if (url.Contains("sistema") || url.Contains("config") || url.Contains("settings")) return false;
            if (url.Contains("interface") || url.Contains("screen") || url.Contains("screenshot")) return false;
            if (url.Contains("captura") || url.Contains("print")) return false;
            if (url.Contains("swagger") || url.Contains("docs")) return false;
            if (url.Contains("uploads/social/defaults")) return false;
            if (url.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) && url.Contains("defaults")) return false;
            return true;
        }

        private static (bool IsInternal, bool IsAuthScreen, bool IsForm, bool IsSystemScreen) DetectarFlagsConteudoInterno(
            string? titulo,
            string? categoria,
            string? subtitulo,
            string? imagemUrl)
        {
            var t = (titulo ?? string.Empty).ToLowerInvariant();
            var c = (categoria ?? string.Empty).ToLowerInvariant();
            var s = (subtitulo ?? string.Empty).ToLowerInvariant();
            var i = (imagemUrl ?? string.Empty).ToLowerInvariant();
            var combinado = $"{t} {c} {s} {i}";

            var isAuth = combinado.Contains("login") || combinado.Contains("signin") || combinado.Contains("auth") || combinado.Contains("autentic");
            var isForm = combinado.Contains("form") || combinado.Contains("formulario");
            var isSystem = combinado.Contains("admin") || combinado.Contains("dashboard") || combinado.Contains("painel")
                || combinado.Contains("settings") || combinado.Contains("config") || combinado.Contains("swagger")
                || combinado.Contains("sistema") || combinado.Contains("interno") || combinado.Contains("interface");
            var isInternal = isAuth || isForm || isSystem || combinado.Contains("screenshot") || combinado.Contains("captura") || combinado.Contains("print");

            return (isInternal, isAuth, isForm, isSystem);
        }

        private static bool ContemTermoBloqueado(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return ExploreBlockedTokens.Any(value.Contains);
        }

        private static bool IsValidForExploreItem(ExplorarVisualItemViewModel item)
        {
            if (item == null) return false;
            if (!item.PossuiImagemValida) return false;
            if (!item.HasVisualValue) return false;
            if (item.ImageQualityScore < 42m) return false;
            if (item.IsInternal || item.IsAuthScreen || item.IsForm || item.IsSystemScreen) return false;
            if (string.IsNullOrWhiteSpace(item.Titulo)) return false;
            if (item.Titulo.Contains("@", StringComparison.OrdinalIgnoreCase)) return false;
            if (item.Subtitulo?.Contains("@", StringComparison.OrdinalIgnoreCase) == true) return false;
            if (ContemTermoBloqueado(item.Titulo.ToLowerInvariant())) return false;
            if (!string.IsNullOrWhiteSpace(item.Subtitulo) && ContemTermoBloqueado(item.Subtitulo.ToLowerInvariant())) return false;
            return true;
        }

        private static bool PostValidoParaExplore(FeedPostViewModel post)
        {
            if (post == null) return false;
            if (!IsImagemExploravel(post.ImagemUrl)) return false;
            var texto = $"{post.NomeAutor} {post.NomeEvento} {post.Categoria} {post.Legenda}".ToLowerInvariant();
            if (ContemTermoBloqueado(texto)) return false;
            return true;
        }

        private static bool EventoValidoParaExplore(ExplorarEventoCardViewModel evento)
        {
            if (evento == null) return false;
            if (!IsImagemExploravel(evento.ImagemCapa)) return false;
            var texto = $"{evento.NomeEvento} {evento.TipoEvento}".ToLowerInvariant();
            if (ContemTermoBloqueado(texto)) return false;
            return true;
        }

        private static bool TemplateValidoParaExplore(ExplorarTemplateCardViewModel template)
        {
            if (template == null) return false;
            if (!IsImagemExploravel(template.ThumbnailUrl)) return false;
            var texto = $"{template.NomeTemplate} {template.Categoria} {template.Estilo}".ToLowerInvariant();
            if (ContemTermoBloqueado(texto)) return false;
            return true;
        }

        private static bool FornecedorValidoParaExplore(ExplorarPerfilCardViewModel perfil)
        {
            if (perfil == null) return false;
            if (!IsImagemExploravel(perfil.FotoPerfilUrl)) return false;
            if (string.IsNullOrWhiteSpace(perfil.NomeExibicao)) return false;
            if (perfil.NomeExibicao.Contains("@", StringComparison.OrdinalIgnoreCase)) return false;
            var texto = $"{perfil.NomeExibicao} {perfil.TipoPerfil} {perfil.Bio} {perfil.Cidade}".ToLowerInvariant();
            if (ContemTermoBloqueado(texto)) return false;
            return true;
        }

        private static List<ExplorarVisualItemViewModel> MisturarItensExplorar(List<ExplorarVisualItemViewModel> itens)
        {
            var ordenados = itens
                .OrderByDescending(i => i.ScoreRelevancia)
                .ThenByDescending(i => i.ImageQualityScore)
                .ThenByDescending(i => i.DestaqueManual)
                .ToList();

            var restante = new List<ExplorarVisualItemViewModel>(ordenados);
            var resultado = new List<ExplorarVisualItemViewModel>(ordenados.Count);

            while (restante.Count > 0)
            {
                var ultimoTipo = resultado.Count > 0 ? resultado[^1].Tipo : null;
                var repeticoesTipo = 0;
                for (var i = resultado.Count - 1; i >= 0 && ultimoTipo != null; i--)
                {
                    if (resultado[i].Tipo != ultimoTipo) break;
                    repeticoesTipo++;
                }

                var ultimoCategoria = resultado.Count > 0 ? resultado[^1].Categoria : null;
                var repeticoesCategoria = 0;
                for (var i = resultado.Count - 1; i >= 0 && ultimoCategoria != null; i--)
                {
                    if (!string.Equals(resultado[i].Categoria, ultimoCategoria, StringComparison.OrdinalIgnoreCase)) break;
                    repeticoesCategoria++;
                }

                var proximo = restante.FirstOrDefault(item =>
                    (repeticoesTipo < 2 || item.Tipo != ultimoTipo)
                    && (repeticoesCategoria < 2 || !string.Equals(item.Categoria, ultimoCategoria, StringComparison.OrdinalIgnoreCase)))
                    ?? restante[0];

                var ultimosDois = resultado.Skip(Math.Max(0, resultado.Count - 2)).ToList();
                if (ultimosDois.Count == 2 && ultimosDois.All(x => x.ScoreRelevancia < 52m))
                {
                    var forte = restante
                        .Where(x => x.ScoreRelevancia >= 58m)
                        .OrderByDescending(x => x.ScoreRelevancia)
                        .FirstOrDefault();
                    if (forte != null)
                    {
                        proximo = forte;
                    }
                }

                resultado.Add(proximo);
                restante.Remove(proximo);
            }

            return resultado;
        }

        private static List<ExplorarVisualItemViewModel> SelecionarHeroItens(List<ExplorarVisualItemViewModel> itens)
        {
            var candidatosHero = itens
                .Where(i => i.ScoreRelevancia >= 72m
                    && i.ImageQualityScore >= 64m
                    && i.HasVisualValue
                    && (i.Tipo == "evento" || i.Tipo == "post" || i.Tipo == "story")
                    && IsImagemExploravel(i.ImagemUrl))
                .ToList();
            if (candidatosHero.Count < 3)
            {
                return new List<ExplorarVisualItemViewModel>();
            }

            var principal = candidatosHero
                .OrderByDescending(i => i.Tipo == "evento")
                .ThenByDescending(i => i.Tipo == "post")
                .ThenByDescending(i => i.ImageQualityScore)
                .ThenByDescending(i => i.ScoreRelevancia)
                .First();

            var secundarios = candidatosHero
                .Where(i => i.Chave != principal.Chave)
                .OrderByDescending(i => i.Tipo != principal.Tipo)
                .ThenByDescending(i => i.ImageQualityScore)
                .ThenByDescending(i => i.ScoreRelevancia)
                .Take(3)
                .ToList();

            return new[] { principal }.Concat(secundarios).ToList();
        }

        private static List<ExplorarInspiracaoCardViewModel> BuildInspiracoes(List<ExplorarVisualItemViewModel> itens)
        {
            if (itens.Count < 2) return new List<ExplorarInspiracaoCardViewModel>();

            var baseItens = itens.Take(4).ToList();
            return baseItens.Select((item, index) => new ExplorarInspiracaoCardViewModel
            {
                Titulo = index switch
                {
                    0 => "Inspiração do momento",
                    1 => "Curadoria visual",
                    2 => "Em alta agora",
                    _ => "Recomendado para você"
                },
                Categoria = string.IsNullOrWhiteSpace(item.Categoria) ? item.Badge : item.Categoria!,
                ImagemUrl = item.ImagemUrl,
                Link = item.Link
            }).ToList();
        }

        private static void AplicarVariantesLayout(List<ExplorarVisualItemViewModel> itens, bool layoutCompacto)
        {
            if (layoutCompacto)
            {
                for (var i = 0; i < itens.Count; i++)
                {
                    itens[i].LayoutVariant = "compact";
                }
                return;
            }

            for (var i = 0; i < itens.Count; i++)
            {
                itens[i].LayoutVariant = (i % 10) switch
                {
                    0 => "tall",
                    3 => "wide",
                    6 => "tall",
                    _ => "portrait"
                };
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Perfil(int id)
        {
            var perfil = await _context.PerfisSociais
                .FirstOrDefaultAsync(p => p.Id == id);

            if (perfil == null)
            {
                return NotFound();
            }

            var posts = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.PerfilSocialId == id
                    && p.Ativo
                    && !p.IsDeleted
                    && !p.IsArchived
                    && !string.IsNullOrWhiteSpace(p.ImagemUrl))
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync();

            posts = posts
                .Where(p => IsImagemExploravel(p.ImagemUrl))
                .ToList();

            var videos = posts
                .Where(p => !string.IsNullOrWhiteSpace(p.TipoConteudo)
                    && p.TipoConteudo!.Contains("video", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var totalCurtidas = await _context.SocialCurtidas
                .AsNoTracking()
                .Where(c => c.Post != null && c.Post.PerfilSocialId == id)
                .CountAsync();

            var eventosVinculados = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.PerfilSocialId == id && p.EventoId.HasValue && p.Evento != null)
                .Select(p => p.Evento!)
                .Distinct()
                .Take(8)
                .ToListAsync();

            var storiesAtivosRaw = await _context.Stories
                .AsNoTracking()
                .Where(s => s.UserId == perfil.UserId.ToString() && s.ExpireAt > DateTime.UtcNow && s.Ativo)
                .Include(s => s.Evento)
                .Include(s => s.SharedPost)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var storyIdsPerfil = storiesAtivosRaw.Select(s => s.Id).ToList();
            var storyViewsPerfil = await _context.StoryViews
                .AsNoTracking()
                .Where(v => storyIdsPerfil.Contains(v.StoryId))
                .ToListAsync();
            var storyReactionsPerfil = await _context.StoryReactions
                .AsNoTracking()
                .Where(r => storyIdsPerfil.Contains(r.StoryId))
                .ToListAsync();

            var perfilAtual = await ObterUsuarioAtualAsync();
            var ehDono = perfilAtual != null && perfilAtual.Id == perfil.UserId;
            var usuarioPerfil = perfilAtual != null
                ? await _context.PerfisSociais.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == perfilAtual.Id)
                : null;

            var totalSeguidores = await _context.PerfilSocialFollows
                .AsNoTracking()
                .CountAsync(f => f.SeguindoPerfilId == perfil.Id);
            var totalSeguindo = await _context.PerfilSocialFollows
                .AsNoTracking()
                .CountAsync(f => f.SeguidorPerfilId == perfil.Id);
            var usuarioSegue = usuarioPerfil != null && await _context.PerfilSocialFollows
                .AsNoTracking()
                .AnyAsync(f => f.SeguidorPerfilId == usuarioPerfil.Id && f.SeguindoPerfilId == perfil.Id);

            var storiesAtivos = storiesAtivosRaw.Select(s =>
            {
                var totalViews = storyViewsPerfil.Count(v => v.StoryId == s.Id);
                var totalReactions = storyReactionsPerfil.Count(r => r.StoryId == s.Id);
                return StoryToStatusItemViewModel(
                    s,
                    perfil,
                    false,
                    totalViews,
                    totalReactions,
                    storyReactionsPerfil
                        .Where(r => r.StoryId == s.Id)
                        .GroupBy(r => r.ReactionType.ToLowerInvariant())
                        .ToDictionary(g => g.Key, g => g.Count()),
                    isOwner: ehDono,
                    userReaction: null);
            }).ToList();

            var postsSalvos = new List<FeedPostViewModel>();
            if (ehDono)
            {
                postsSalvos = await _context.SocialPostsSalvos
                    .AsNoTracking()
                    .Where(s => s.UserId == perfil.UserId
                        && s.Post != null
                        && s.Post.Ativo
                        && !s.Post.IsDeleted
                        && !s.Post.IsArchived
                        && !string.IsNullOrWhiteSpace(s.Post.ImagemUrl))
                    .OrderByDescending(s => s.CriadoEm)
                    .Take(6)
                    .Select(s => new FeedPostViewModel
                    {
                        PostId = s.PostId,
                        NomeAutor = s.Post != null && s.Post.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.Post.PerfilSocial.NomeExibicao)
                            ? s.Post.PerfilSocial.NomeExibicao
                            : (s.Post != null && s.Post.User != null && !string.IsNullOrWhiteSpace(s.Post.User.UserName) ? s.Post.User.UserName! : "Participante"),
                        FotoPerfilUrl = s.Post != null && s.Post.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.Post.PerfilSocial.FotoPerfilUrl)
                            ? NormalizarUrlImagemPerfil(s.Post.PerfilSocial.FotoPerfilUrl)
                            : FotoPerfilPadrao,
                        Legenda = s.Post != null ? s.Post.Legenda : string.Empty,
                        ImagemUrl = s.Post == null ? ImagemPostPadrao : NormalizarUrlImagemPost(s.Post.ImagemUrl),
                        Categoria = s.Post != null ? s.Post.Categoria : null,
                        TipoConteudo = s.Post != null ? s.Post.TipoConteudo : null,
                        Localizacao = s.Post != null ? s.Post.Localizacao : null,
                        DataCriacao = s.Post != null ? s.Post.CriadoEm : DateTime.UtcNow,
                        TotalCurtidas = s.Post != null ? s.Post.Curtidas.Count : 0,
                        TotalComentarios = s.Post != null ? s.Post.Comentarios.Count(c => c.Ativo) : 0,
                        UsuarioCurtiu = false,
                        UsuarioSalvou = true,
                        EventoId = s.Post != null ? s.Post.EventoId : null,
                        NomeEvento = s.Post != null && s.Post.Evento != null ? s.Post.Evento.NomeEvento : null,
                        PerfilId = s.Post != null ? s.Post.PerfilSocialId : 0,
                        TipoPerfil = s.Post != null && s.Post.PerfilSocial != null ? s.Post.PerfilSocial.TipoPerfil : "Convidado",
                        Cidade = s.Post != null && s.Post.PerfilSocial != null ? s.Post.PerfilSocial.Cidade : null
                    })
                    .ToListAsync();

                postsSalvos = postsSalvos
                    .Where(p => IsImagemExploravel(p.ImagemUrl))
                    .ToList();
            }

            var highlights = await _context.StoryHighlights
                .AsNoTracking()
                .Where(h => h.UserId == perfil.UserId.ToString())
                .Select(h => new StoryHighlightViewModel
                {
                    Id = h.Id,
                    Nome = h.Nome,
                    CapaUrl = string.IsNullOrWhiteSpace(h.CapaUrl) ? ImagemPostPadrao : h.CapaUrl!,
                    TotalStories = _context.StoryHighlightItems.Count(i => i.HighlightId == h.Id),
                    PrimeiroStoryId = _context.StoryHighlightItems
                        .Where(i => i.HighlightId == h.Id)
                        .OrderBy(i => i.Ordem)
                        .Select(i => (int?)i.StoryId)
                        .FirstOrDefault(),
                    PerfilId = perfil.Id
                })
                .ToListAsync();

            var postsEventosIds = eventosVinculados.Select(e => e.Id).ToHashSet();
            var listaEventos = eventosVinculados
                .OrderByDescending(e => e.DataEvento)
                .ToList();
            var listaMarcados = posts
                .Where(p => p.EventoId.HasValue && postsEventosIds.Contains(p.EventoId.Value))
                .ToList();

            var usuarioPerfilIdentity = perfil.User;
            if (usuarioPerfilIdentity == null)
            {
                usuarioPerfilIdentity = await _userManager.FindByIdAsync(perfil.UserId.ToString());
            }

            var usernameRaw = !string.IsNullOrWhiteSpace(usuarioPerfilIdentity?.UserName)
                ? (string.IsNullOrWhiteSpace(perfil.Username) ? usuarioPerfilIdentity.UserName : perfil.Username)
                : (!string.IsNullOrWhiteSpace(perfil.Username)
                    ? perfil.Username
                    : (perfil.NomeExibicao ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", "."));
            var username = NormalizarUsername(usernameRaw);
            var nomeReal = string.Empty;
            if (!string.IsNullOrWhiteSpace(usuarioPerfilIdentity?.Email))
            {
                nomeReal = usuarioPerfilIdentity.Email.Split('@')[0].Replace('.', ' ').Replace('_', ' ');
                nomeReal = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nomeReal.ToLowerInvariant());
            }
            var linkPrincipal = !string.IsNullOrWhiteSpace(perfil.Site) ? perfil.Site : perfil.Instagram;
            if (!string.IsNullOrWhiteSpace(linkPrincipal)
                && !linkPrincipal.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !linkPrincipal.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                linkPrincipal = $"https://{linkPrincipal}";
            }

            var model = new PerfilSocialViewModel
            {
                Id = perfil.Id,
                NomeExibicao = perfil.NomeExibicao,
                Bio = perfil.Bio,
                FotoPerfilUrl = string.IsNullOrWhiteSpace(perfil.FotoPerfilUrl) ? FotoPerfilPadrao : perfil.FotoPerfilUrl,
                AvatarUrl = NormalizarUrlImagemPerfil(perfil.FotoPerfilUrl),
                TipoPerfil = perfil.TipoPerfil,
                CategoriaPerfil = perfil.TipoPerfil,
                Cidade = perfil.Cidade,
                Instagram = perfil.Instagram,
                Site = perfil.Site,
                Link = linkPrincipal,
                NomeReal = nomeReal,
                Username = username,
                NomeUsuario = ResolverIdentidadePrincipal(perfil, usuarioPerfilIdentity),
                TotalPosts = posts.Count,
                TotalCurtidasRecebidas = totalCurtidas,
                TotalStoriesAtivos = storiesAtivos.Count,
                TotalVisualizacoesStoriesRecentes = storyViewsPerfil.Count,
                TotalDestaques = highlights.Count,
                TotalEngajamentoStories = storiesAtivos.Sum(s => s.EngagementScore),
                TotalEventosRelacionados = eventosVinculados.Count,
                TotalSeguidores = totalSeguidores,
                TotalSeguindo = totalSeguindo,
                UsuarioSegue = usuarioSegue,
                EhPerfilDoUsuarioLogado = ehDono,
                UsernamePublico = username,
                ListaPosts = posts,
                ListaVideos = videos,
                ListaMarcados = listaMarcados,
                ListaEventos = listaEventos,
                EventosVinculados = eventosVinculados,
                StoriesAtivos = storiesAtivos,
                Highlights = highlights,
                Destaques = highlights,
                PostsSalvos = postsSalvos,
                ListaSalvos = postsSalvos
            };

            return View(model);
        }

        [HttpPost("Social/Perfil/AlternarFollow")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarFollowPerfil([FromForm] int perfilId)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var meuPerfil = await _context.PerfisSociais.FirstOrDefaultAsync(p => p.UserId == usuario.Id);
            var perfilAlvo = await _context.PerfisSociais.FirstOrDefaultAsync(p => p.Id == perfilId);
            if (meuPerfil == null || perfilAlvo == null)
            {
                return NotFound();
            }

            if (meuPerfil.Id == perfilAlvo.Id)
            {
                return BadRequest(new { ok = false, message = "Você não pode seguir o próprio perfil." });
            }

            var relacionamento = await _context.PerfilSocialFollows
                .FirstOrDefaultAsync(f => f.SeguidorPerfilId == meuPerfil.Id && f.SeguindoPerfilId == perfilAlvo.Id);

            bool seguindo;
            if (relacionamento == null)
            {
                _context.PerfilSocialFollows.Add(new PerfilSocialFollow
                {
                    SeguidorPerfilId = meuPerfil.Id,
                    SeguindoPerfilId = perfilAlvo.Id,
                    CriadoEm = DateTime.UtcNow
                });
                seguindo = true;
            }
            else
            {
                _context.PerfilSocialFollows.Remove(relacionamento);
                seguindo = false;
            }

            await _context.SaveChangesAsync();

            var totalSeguidores = await _context.PerfilSocialFollows
                .AsNoTracking()
                .CountAsync(f => f.SeguindoPerfilId == perfilAlvo.Id);

            return Json(new
            {
                ok = true,
                seguindo,
                totalSeguidores
            });
        }

        [HttpGet]
        public async Task<IActionResult> MeuPerfil()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var perfil = await GarantirPerfilSocialAsync(usuario);
            return RedirectToAction(nameof(Perfil), new { id = perfil.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var perfil = await GarantirPerfilSocialAsync(usuario);
            var model = new PerfilSocialViewModel
            {
                Id = perfil.Id,
                NomeExibicao = perfil.NomeExibicao,
                Username = NormalizarUsername(perfil.Username),
                Bio = perfil.Bio,
                FotoPerfilUrl = NormalizarUrlImagemPerfil(perfil.FotoPerfilUrl),
                TipoPerfil = perfil.TipoPerfil,
                Cidade = perfil.Cidade,
                Instagram = perfil.Instagram,
                Site = perfil.Site
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(PerfilSocialViewModel model)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var perfil = await _context.PerfisSociais.FirstOrDefaultAsync(p => p.Id == model.Id && p.UserId == usuario.Id);
            if (perfil == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            perfil.NomeExibicao = model.NomeExibicao;
            perfil.Username = string.IsNullOrWhiteSpace(model.Username)
                ? null
                : model.Username.Trim().TrimStart('@');
            perfil.Bio = model.Bio;
            perfil.Cidade = model.Cidade;
            perfil.Instagram = model.Instagram;
            perfil.Site = model.Site;
            perfil.AtualizadoEm = DateTime.UtcNow;

            if (model.NovaFotoPerfil != null)
                {
                    try
                    {
                        perfil.FotoPerfilUrl = await SalvarArquivoSocialAsync(model.NovaFotoPerfil, PastaUploadPerfis, FotoPerfilPadrao);
                    }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(nameof(model.NovaFotoPerfil), ex.Message);
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Perfil), new { id = perfil.Id });
        }

        [HttpGet]
        public async Task<IActionResult> CriarPost()
        {
            var model = new CriarPostViewModel
            {
                EventosDisponiveis = await ObterEventosSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarPost(CriarPostViewModel model)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                model.EventosDisponiveis = await ObterEventosSelectListAsync();
                return View(model);
            }

            var perfil = await GarantirPerfilSocialAsync(usuario);
            string imagemUrl;
            try
            {
                imagemUrl = model.Imagem != null
                    ? await SalvarArquivoSocialAsync(model.Imagem, PastaUploadPosts, ImagemPostPadrao)
                    : ImagemPostPadrao;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.Imagem), ex.Message);
                model.EventosDisponiveis = await ObterEventosSelectListAsync();
                return View(model);
            }

            var post = new SocialPost
            {
                UserId = usuario.Id,
                PerfilSocialId = perfil.Id,
                EventoId = model.EventoId,
                Titulo = model.Titulo,
                Legenda = model.Legenda,
                Categoria = model.Categoria,
                TipoConteudo = model.TipoConteudo,
                Localizacao = model.Localizacao,
                ImagemUrl = imagemUrl,
                CriadoEm = DateTime.UtcNow,
                AtualizadoEm = DateTime.UtcNow,
                Ativo = true
            };

            _context.SocialPosts.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Feed));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Post(int id)
        {
            var usuarioAtual = await ObterUsuarioAtualAsync();
            var post = await _context.SocialPosts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.PerfilSocial)
                .Include(p => p.Evento)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (post == null || post.User == null || post.PerfilSocial == null)
            {
                return NotFound();
            }

            var ehDonoPost = usuarioAtual != null && post.UserId == usuarioAtual.Id;
            if (!PostVisivelPublicamente(post) && !ehDonoPost)
            {
                return NotFound();
            }

            var comentarios = await _context.SocialComentarios
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.PerfilSocial)
                .Where(c => c.PostId == id && c.Ativo)
                .OrderByDescending(c => c.CriadoEm)
                .ToListAsync();

            var model = new PostDetalheViewModel
            {
                Post = post,
                Perfil = post.PerfilSocial,
                ListaComentarios = comentarios,
                TotalCurtidas = await _context.SocialCurtidas.CountAsync(c => c.PostId == id),
                UsuarioCurtiu = usuarioAtual != null && await _context.SocialCurtidas.AnyAsync(c => c.PostId == id && c.UserId == usuarioAtual.Id),
                UsuarioSalvou = usuarioAtual != null && await _context.SocialPostsSalvos.AnyAsync(c => c.PostId == id && c.UserId == usuarioAtual.Id),
                EventoRelacionado = post.Evento,
                NomeAutor = ResolverNomeExibicao(post.PerfilSocial, post.User),
                TipoPerfil = string.IsNullOrWhiteSpace(post.PerfilSocial.TipoPerfil) ? (post.User.TipoUsuario ?? "Convidado") : post.PerfilSocial.TipoPerfil,
                Cidade = post.PerfilSocial.Cidade,
                IsOwner = ehDonoPost,
                StatusAutorAtivos = await _context.SocialStatus
                    .AsNoTracking()
                    .Where(s => s.Ativo && s.PerfilSocialId == post.PerfilSocialId && s.ExpiraEm > DateTime.UtcNow)
                    .OrderByDescending(s => s.CriadoEm)
                    .Select(s => new SocialStatusItemViewModel
                    {
                        Id = s.Id,
                        PerfilId = s.PerfilSocialId,
                        NomePerfil = !string.IsNullOrWhiteSpace(s.PerfilSocial != null ? s.PerfilSocial.NomeExibicao : null)
                            ? s.PerfilSocial!.NomeExibicao
                            : (!string.IsNullOrWhiteSpace(s.User != null ? s.User.UserName : null) ? s.User!.UserName! : (s.User != null ? s.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = s.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.PerfilSocial.FotoPerfilUrl)
                            ? NormalizarUrlImagemPerfil(s.PerfilSocial.FotoPerfilUrl)
                            : FotoPerfilPadrao,
                        ImagemUrl = NormalizarUrlImagemPost(s.ImagemUrl),
                        TextoOverlay = s.TextoOverlay,
                        CriadoEm = s.CriadoEm,
                        Visualizado = usuarioAtual != null && s.Visualizacoes.Any(v => v.UserId == usuarioAtual.Id)
                })
                    .ToListAsync()
            };

            model.Post.ImagemUrl = NormalizarUrlImagemPost(model.Post.ImagemUrl);
            model.Perfil.FotoPerfilUrl = NormalizarUrlImagemPerfil(model.Perfil.FotoPerfilUrl);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MeusPosts()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var posts = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.UserId == usuario.Id && !p.IsDeleted)
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync();

            foreach (var post in posts)
            {
                post.ImagemUrl = NormalizarUrlImagemPost(post.ImagemUrl);
            }

            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirPost(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id);
            if (post == null)
            {
                return Forbid();
            }

            post.IsDeleted = true;
            post.Ativo = false;
            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (isAjax)
            {
                return Json(new { ok = true, message = "Publicação excluída." });
            }
            return RedirectToAction(nameof(MeusPosts));
        }

        [HttpGet]
        public async Task<IActionResult> EditarPost(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && !p.IsDeleted);
            if (post == null)
            {
                return NotFound();
            }

            var model = new CriarPostViewModel
            {
                Titulo = post.Titulo,
                Legenda = post.Legenda,
                Categoria = post.Categoria,
                TipoConteudo = post.TipoConteudo,
                Localizacao = post.Localizacao,
                EventoId = post.EventoId,
                EventosDisponiveis = await ObterEventosSelectListAsync()
            };

            ViewBag.PostId = post.Id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPost(int id, CriarPostViewModel model)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && !p.IsDeleted);
            if (post == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(model.Imagem));
            if (!ModelState.IsValid)
            {
                model.EventosDisponiveis = await ObterEventosSelectListAsync();
                ViewBag.PostId = id;
                return View(model);
            }

            post.Titulo = model.Titulo;
            post.Legenda = model.Legenda;
            post.Categoria = model.Categoria;
            post.TipoConteudo = model.TipoConteudo;
            post.Localizacao = model.Localizacao;
            post.EventoId = model.EventoId;
            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Post), new { id = post.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Curtir(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo && !p.IsArchived && !p.IsDeleted);
            if (post == null)
            {
                return NotFound();
            }
            if (!post.CommentsEnabled)
            {
                return RedirectToAction(nameof(Post), new { id });
            }

            var curtidaExiste = await _context.SocialCurtidas.AnyAsync(c => c.PostId == id && c.UserId == usuario.Id);
            if (!curtidaExiste)
            {
                _context.SocialCurtidas.Add(new SocialCurtida
                {
                    PostId = id,
                    UserId = usuario.Id,
                    CriadoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrWhiteSpace(referer) ? RedirectToAction(nameof(Post), new { id }) : Redirect(referer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarCurtida(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var postExiste = await _context.SocialPosts.AnyAsync(p => p.Id == id && p.Ativo && !p.IsArchived && !p.IsDeleted);
            if (!postExiste)
            {
                return NotFound();
            }

            var curtida = await _context.SocialCurtidas.FirstOrDefaultAsync(c => c.PostId == id && c.UserId == usuario.Id);
            var usuarioCurtiu = false;
            if (curtida == null)
            {
                _context.SocialCurtidas.Add(new SocialCurtida
                {
                    PostId = id,
                    UserId = usuario.Id,
                    CriadoEm = DateTime.UtcNow
                });
                usuarioCurtiu = true;
            }
            else
            {
                _context.SocialCurtidas.Remove(curtida);
            }

            await _context.SaveChangesAsync();
            var totalCurtidas = await _context.SocialCurtidas.CountAsync(c => c.PostId == id);
            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (!isAjax)
            {
                var referer = Request.Headers.Referer.ToString();
                return string.IsNullOrWhiteSpace(referer) ? RedirectToAction(nameof(Post), new { id }) : Redirect(referer);
            }

            return Json(new
            {
                ok = true,
                totalCurtidas,
                usuarioCurtiu
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Descurtir(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var curtida = await _context.SocialCurtidas.FirstOrDefaultAsync(c => c.PostId == id && c.UserId == usuario.Id);
            if (curtida != null)
            {
                _context.SocialCurtidas.Remove(curtida);
                await _context.SaveChangesAsync();
            }

            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrWhiteSpace(referer) ? RedirectToAction(nameof(Post), new { id }) : Redirect(referer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comentar(int id, string texto)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                return RedirectToAction(nameof(Post), new { id });
            }

            var postExiste = await _context.SocialPosts.AnyAsync(p => p.Id == id && p.Ativo && !p.IsArchived && !p.IsDeleted);
            if (!postExiste)
            {
                return NotFound();
            }

            _context.SocialComentarios.Add(new SocialComentario
            {
                PostId = id,
                UserId = usuario.Id,
                PerfilSocialId = (await GarantirPerfilSocialAsync(usuario)).Id,
                Texto = texto.Trim(),
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Post), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ComentarRapido(int id, string texto)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var textoLimpo = texto?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(textoLimpo))
            {
                return BadRequest(new { ok = false, mensagem = "Comentário vazio." });
            }

            var post = await _context.SocialPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo && !p.IsArchived && !p.IsDeleted);
            if (post == null)
            {
                return NotFound();
            }
            if (!post.CommentsEnabled)
            {
                return BadRequest(new { ok = false, mensagem = "Comentários desativados para esta publicação." });
            }

            var perfil = await GarantirPerfilSocialAsync(usuario);
            _context.SocialComentarios.Add(new SocialComentario
            {
                PostId = id,
                UserId = usuario.Id,
                PerfilSocialId = perfil.Id,
                Texto = textoLimpo,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            });
            await _context.SaveChangesAsync();

            var totalComentarios = await _context.SocialComentarios.CountAsync(c => c.PostId == id && c.Ativo);
            var isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (!isAjax)
            {
                return RedirectToAction(nameof(Post), new { id });
            }

            return Json(new
            {
                ok = true,
                nomeAutor = ResolverNomeExibicao(perfil, usuario),
                texto = textoLimpo,
                totalComentarios
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirComentario(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var comentario = await _context.SocialComentarios.FirstOrDefaultAsync(c => c.Id == id && c.Ativo);
            if (comentario == null)
            {
                return NotFound();
            }

            if (comentario.UserId != usuario.Id)
            {
                return Forbid();
            }

            comentario.Ativo = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Post), new { id = comentario.PostId });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Evento(int eventoId)
        {
            var evento = await _context.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventoId);
            if (evento == null)
            {
                return NotFound();
            }

            var posts = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo && !p.IsArchived && !p.IsDeleted && p.EventoId == eventoId)
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : (p.User != null ? p.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? NormalizarUrlImagemPerfil(p.PerfilSocial.FotoPerfilUrl)
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = NormalizarUrlImagemPost(p.ImagemUrl),
                    Categoria = p.Categoria,
                    TipoConteudo = p.TipoConteudo,
                    Localizacao = p.Localizacao,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    HideLikesCount = p.HideLikesCount,
                    HideSharesCount = p.HideSharesCount,
                    CommentsEnabled = p.CommentsEnabled,
                    IsPinned = p.IsPinned,
                    IsArchived = p.IsArchived,
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId,
                    TipoPerfil = p.PerfilSocial != null ? p.PerfilSocial.TipoPerfil : (p.User != null ? p.User.TipoUsuario ?? "Convidado" : "Convidado"),
                    Cidade = p.PerfilSocial != null ? p.PerfilSocial.Cidade : null,
                    IsOwner = false
                })
                .ToListAsync();

            ViewBag.Evento = evento;
            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarPost(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var postExiste = await _context.SocialPosts.AnyAsync(p => p.Id == id && p.Ativo && !p.IsArchived && !p.IsDeleted);
            if (!postExiste)
            {
                return NotFound();
            }

            var jaSalvo = await _context.SocialPostsSalvos.AnyAsync(s => s.PostId == id && s.UserId == usuario.Id);
            if (!jaSalvo)
            {
                _context.SocialPostsSalvos.Add(new SocialPostSalvo
                {
                    PostId = id,
                    UserId = usuario.Id,
                    CriadoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrWhiteSpace(referer) ? RedirectToAction(nameof(Post), new { id }) : Redirect(referer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverSalvo(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var salvo = await _context.SocialPostsSalvos.FirstOrDefaultAsync(s => s.PostId == id && s.UserId == usuario.Id);
            if (salvo != null)
            {
                _context.SocialPostsSalvos.Remove(salvo);
                await _context.SaveChangesAsync();
            }

            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrWhiteSpace(referer) ? RedirectToAction(nameof(Post), new { id }) : Redirect(referer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarFixadoPost(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && !p.IsDeleted);
            if (post == null)
            {
                return Forbid();
            }

            post.IsPinned = !post.IsPinned;
            post.PinnedOrder = post.IsPinned ? (post.PinnedOrder ?? 1) : null;
            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { ok = true, pinned = post.IsPinned, message = post.IsPinned ? "Publicação fixada no perfil." : "Fixação removida." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarArquivamentoPost(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && !p.IsDeleted);
            if (post == null)
            {
                return Forbid();
            }

            post.IsArchived = !post.IsArchived;
            if (post.IsArchived)
            {
                post.Ativo = false;
            }
            else
            {
                post.Ativo = true;
            }
            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { ok = true, archived = post.IsArchived, message = post.IsArchived ? "Publicação arquivada." : "Publicação desarquivada." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarOcultarCurtidasPost(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && !p.IsDeleted);
            if (post == null)
            {
                return Forbid();
            }

            post.HideLikesCount = !post.HideLikesCount;
            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { ok = true, hidden = post.HideLikesCount, message = post.HideLikesCount ? "Curtidas ocultadas." : "Curtidas visíveis." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarOcultarCompartilhamentosPost(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && !p.IsDeleted);
            if (post == null)
            {
                return Forbid();
            }

            post.HideSharesCount = !post.HideSharesCount;
            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { ok = true, hidden = post.HideSharesCount, message = post.HideSharesCount ? "Compartilhamentos ocultados." : "Compartilhamentos visíveis." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarComentariosPost(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && !p.IsDeleted);
            if (post == null)
            {
                return Forbid();
            }

            post.CommentsEnabled = !post.CommentsEnabled;
            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { ok = true, enabled = post.CommentsEnabled, message = post.CommentsEnabled ? "Comentários ativados." : "Comentários desativados." });
        }

        [HttpGet]
        public async Task<IActionResult> Salvos()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var salvos = await _context.SocialPostsSalvos
                .AsNoTracking()
                .Where(s => s.UserId == usuario.Id && s.Post != null && s.Post.Ativo && !s.Post.IsArchived && !s.Post.IsDeleted)
                .OrderByDescending(s => s.CriadoEm)
                .Select(s => new FeedPostViewModel
                {
                    PostId = s.PostId,
                    NomeAutor = !string.IsNullOrWhiteSpace(s.Post != null && s.Post.PerfilSocial != null ? s.Post.PerfilSocial.NomeExibicao : null)
                        ? s.Post!.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(s.Post != null && s.Post.User != null ? s.Post.User.UserName : null)
                            ? s.Post!.User!.UserName!
                            : (s.Post != null && s.Post.User != null ? s.Post.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = s.Post != null && s.Post.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.Post.PerfilSocial.FotoPerfilUrl)
                        ? NormalizarUrlImagemPerfil(s.Post.PerfilSocial.FotoPerfilUrl)
                        : FotoPerfilPadrao,
                    Legenda = s.Post != null ? s.Post.Legenda : string.Empty,
                    ImagemUrl = s.Post == null ? ImagemPostPadrao : NormalizarUrlImagemPost(s.Post.ImagemUrl),
                    Categoria = s.Post != null ? s.Post.Categoria : null,
                    TipoConteudo = s.Post != null ? s.Post.TipoConteudo : null,
                    Localizacao = s.Post != null ? s.Post.Localizacao : null,
                    DataCriacao = s.Post != null ? s.Post.CriadoEm : DateTime.UtcNow,
                    TotalCurtidas = s.Post != null ? s.Post.Curtidas.Count : 0,
                    TotalComentarios = s.Post != null ? s.Post.Comentarios.Count(c => c.Ativo) : 0,
                    HideLikesCount = s.Post != null && s.Post.HideLikesCount,
                    HideSharesCount = s.Post != null && s.Post.HideSharesCount,
                    CommentsEnabled = s.Post == null || s.Post.CommentsEnabled,
                    IsPinned = s.Post != null && s.Post.IsPinned,
                    IsArchived = s.Post != null && s.Post.IsArchived,
                    UsuarioCurtiu = s.Post != null && s.Post.Curtidas.Any(c => c.UserId == usuario.Id),
                    UsuarioSalvou = true,
                    EventoId = s.Post != null ? s.Post.EventoId : null,
                    NomeEvento = s.Post != null && s.Post.Evento != null ? s.Post.Evento.NomeEvento : null,
                    PerfilId = s.Post != null ? s.Post.PerfilSocialId : 0,
                    TipoPerfil = s.Post != null && s.Post.PerfilSocial != null ? s.Post.PerfilSocial.TipoPerfil : (s.Post != null && s.Post.User != null ? s.Post.User.TipoUsuario ?? "Convidado" : "Convidado"),
                    Cidade = s.Post != null && s.Post.PerfilSocial != null ? s.Post.PerfilSocial.Cidade : null,
                    IsOwner = true
                })
                .ToListAsync();

            return View(salvos);
        }

        [HttpGet]
        public async Task<IActionResult> FeedStories()
        {
            var usuario = await ObterUsuarioAtualAsync();
            var usuarioId = usuario?.Id;
            if (usuario != null)
            {
                await GarantirPerfilSocialAsync(usuario);
            }

            var stories = await ObterStoriesAtivosAsync(usuarioId);

            var agrupados = stories
                .GroupBy(s => s.PerfilId)
                .SelectMany(g =>
                {
                    var ordenados = g.OrderByDescending(x => x.CriadoEm).ToList();
                    for (var i = 0; i < ordenados.Count; i++)
                    {
                        ordenados[i].SequenciaNoPerfil = i + 1;
                        ordenados[i].TotalNoPerfil = ordenados.Count;
                    }
                    return ordenados;
                })
                .OrderBy(s => s.Visualizado)
                .ThenByDescending(s => s.CriadoEm)
                .ToList();

            return Json(agrupados);
        }

        [HttpGet("Social/Story/Create")]
        public async Task<IActionResult> StoryCreate()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var recentes = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.UserId == usuario.Id && p.Ativo && !string.IsNullOrWhiteSpace(p.ImagemUrl))
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => p.ImagemUrl)
                .Take(18)
                .ToListAsync();

            var model = new StoryCreateViewModel
            {
                MidiasRecentes = recentes
                    .Select(url => SocialImagemHelper.NormalizePublicImageUrl(url, ImagemPostPadrao))
                    .ToList()
            };

            return View("StoryCreate", model);
        }

        [HttpGet("Social/Story/SharePost/{postId:int}")]
        public async Task<IActionResult> StorySharePost(int postId)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var post = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Id == postId && p.Ativo && !p.IsArchived && !p.IsDeleted)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : "Participante"),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? NormalizarUrlImagemPerfil(p.PerfilSocial.FotoPerfilUrl)
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = NormalizarUrlImagemPost(p.ImagemUrl),
                    DataCriacao = p.CriadoEm,
                    HideLikesCount = p.HideLikesCount,
                    HideSharesCount = p.HideSharesCount,
                    CommentsEnabled = p.CommentsEnabled,
                    IsPinned = p.IsPinned,
                    IsArchived = p.IsArchived,
                    TipoPerfil = p.PerfilSocial != null ? p.PerfilSocial.TipoPerfil : "Convidado",
                    PerfilId = p.PerfilSocialId,
                    IsOwner = p.UserId == usuario.Id
                })
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound();
            }

            var model = new StoryEditorViewModel
            {
                MediaUrl = post.ImagemUrl,
                SharedPostId = post.PostId,
                SharedPost = post,
                EventosDisponiveis = await ObterEventosSelectListAsync()
            };

            return View("StoryEditor", model);
        }

        [HttpPost("Social/Story/Upload")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoryUpload(IFormFile? arquivo)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            if (arquivo == null)
            {
                TempData["ErrorMessage"] = "Selecione uma imagem para continuar.";
                return RedirectToAction(nameof(StoryCreate));
            }

            string mediaUrl;
            try
            {
                mediaUrl = await SalvarArquivoSocialAsync(arquivo, PastaUploadStories, ImagemPostPadrao);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(StoryCreate));
            }

            return RedirectToAction(nameof(StoryEditor), new { mediaUrl });
        }

        [HttpGet("Social/Story/Editor")]
        public async Task<IActionResult> StoryEditor(string mediaUrl)
        {
            if (string.IsNullOrWhiteSpace(mediaUrl))
            {
                return RedirectToAction(nameof(StoryCreate));
            }

            var model = new StoryEditorViewModel
            {
                MediaUrl = SocialImagemHelper.NormalizePublicImageUrl(mediaUrl, ImagemPostPadrao),
                EventosDisponiveis = await ObterEventosSelectListAsync()
            };

            return View("StoryEditor", model);
        }

        [HttpPost("Social/Story/Publish")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoryPublish(StoryEditorViewModel model)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(model.MediaUrl))
            {
                TempData["ErrorMessage"] = "Mídia inválida para publicar o story.";
                return RedirectToAction(nameof(StoryCreate));
            }

            var perfil = await GarantirPerfilSocialAsync(usuario);

            if (model.EventoId.HasValue)
            {
                var eventoExiste = await _context.Eventos.AnyAsync(e => e.Id == model.EventoId.Value);
                if (!eventoExiste)
                {
                    TempData["ErrorMessage"] = "Evento selecionado é inválido.";
                    return RedirectToAction(nameof(StoryCreate));
                }
            }

            var story = new Story
            {
                UserId = usuario.Id.ToString(),
                PerfilSocialId = perfil.Id,
                MediaUrl = SocialImagemHelper.NormalizePublicImageUrl(model.MediaUrl, ImagemPostPadrao),
                TextOverlay = string.IsNullOrWhiteSpace(model.TextOverlay) ? string.Empty : model.TextOverlay.Trim(),
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddHours(24),
                EventoId = model.EventoId,
                SharedPostId = model.SharedPostId,
                Ativo = true
            };
            _context.Stories.Add(story);
            await _context.SaveChangesAsync();

            var storyId = story.Id;

            if (!string.IsNullOrWhiteSpace(model.TextOverlay))
            {
                var normalized = model.TextOverlay!;
                var explicitMentions = new List<string>();
                if (!string.IsNullOrWhiteSpace(model.MentionsSerialized))
                {
                    try
                    {
                        explicitMentions = JsonSerializer.Deserialize<List<string>>(model.MentionsSerialized) ?? new List<string>();
                    }
                    catch (JsonException)
                    {
                        explicitMentions = new List<string>();
                    }
                }

                var allMentionTokens = MentionRegex.Matches(normalized)
                    .Select(m => m.Groups[1].Value)
                    .Concat(explicitMentions)
                    .Select(x => x.Trim().TrimStart('@').ToLowerInvariant())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .Take(12)
                    .ToList();

                if (allMentionTokens.Count > 0)
                {
                    var mencionados = await _context.PerfisSociais
                        .Where(p => allMentionTokens.Contains(p.NomeExibicao.ToLower()))
                        .Take(12)
                        .ToListAsync();

                    foreach (var perfilMencionado in mencionados)
                    {
                        _context.StoryMentions.Add(new StoryMention
                        {
                            StoryId = storyId,
                            MentionedUserId = perfilMencionado.UserId.ToString(),
                            MentionedPerfilId = perfilMencionado.Id,
                            MentionText = $"@{perfilMencionado.NomeExibicao}",
                            CreatedAt = DateTime.UtcNow
                        });

                        if (perfilMencionado.UserId != usuario.Id)
                        {
                            await CriarNotificacaoSocialAsync(
                                perfilMencionado.UserId,
                                "Você foi mencionado em um story",
                                $"{perfil.NomeExibicao} mencionou você em um story.",
                                "StoryMention",
                                Url.Action(nameof(Stories), "Social"));
                        }
                    }
                }
            }

            if (model.SharedPostId.HasValue)
            {
                var ownerPostUserId = await _context.SocialPosts
                    .AsNoTracking()
                    .Where(p => p.Id == model.SharedPostId.Value)
                    .Select(p => p.UserId)
                    .FirstOrDefaultAsync();

                if (ownerPostUserId > 0 && ownerPostUserId != usuario.Id)
                {
                    await CriarNotificacaoSocialAsync(
                        ownerPostUserId,
                        "Seu post foi compartilhado em um story",
                        $"{perfil.NomeExibicao} compartilhou seu post em um story.",
                        "StoryShare",
                        Url.Action(nameof(Post), "Social", new { id = model.SharedPostId.Value }));
                }
            }

            return RedirectToAction(nameof(Feed));
        }

        [HttpGet]
        public async Task<IActionResult> Stories()
        {
            var usuarioAtual = await ObterUsuarioAtualAsync();
            if (usuarioAtual != null)
            {
                await GarantirPerfilSocialAsync(usuarioAtual);
            }
            var stories = await ObterStoriesAtivosAsync(usuarioAtual?.Id);

            foreach (var story in stories)
            {
                story.ImagemUrl = NormalizarUrlImagemPost(story.ImagemUrl);
                story.FotoPerfilUrl = NormalizarUrlImagemPerfil(story.FotoPerfilUrl);
            }
            return View(stories);
        }

        [HttpGet("Social/Story/Views/{storyId:int}")]
        public async Task<IActionResult> StoryViews(int storyId)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var storyOwner = await _context.Stories
                .AsNoTracking()
                .Where(s => s.Id == storyId)
                .Select(s => s.UserId)
                .FirstOrDefaultAsync();

            if (storyOwner == null)
            {
                return NotFound();
            }

            if (storyOwner != usuario.Id.ToString())
            {
                return Forbid();
            }

            var viewsRaw = await _context.StoryViews
                .AsNoTracking()
                .Where(v => v.StoryId == storyId)
                .OrderByDescending(v => v.ViewedAt)
                .ToListAsync();

            var viewerIds = viewsRaw
                .Select(v => int.TryParse(v.UserId, out var parsed) ? parsed : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .Distinct()
                .ToList();
            var perfis = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => viewerIds.Contains(p.UserId))
                .ToListAsync();
            var perfisPorUserId = perfis.ToDictionary(p => p.UserId.ToString(), p => p);

            var views = viewsRaw.Select(v =>
            {
                perfisPorUserId.TryGetValue(v.UserId, out var perfilViewer);
                return new StoryViewerViewModel
                {
                    NomeExibicao = string.IsNullOrWhiteSpace(perfilViewer?.NomeExibicao) ? "Participante" : perfilViewer.NomeExibicao,
                    FotoPerfilUrl = NormalizarUrlImagemPerfil(perfilViewer?.FotoPerfilUrl),
                    TipoPerfil = string.IsNullOrWhiteSpace(perfilViewer?.TipoPerfil) ? "Convidado" : perfilViewer!.TipoPerfil,
                    ViewedAt = v.ViewedAt
                };
            }).ToList();

            return PartialView("Partials/_StoryViewsModal", views);
        }

        [HttpPost("Social/Story/React/{storyId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoryReact(int storyId, [FromForm] string reactionType)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var normalizedReaction = (reactionType ?? string.Empty).Trim().ToLowerInvariant();
            if (!TiposReacaoPermitidos.Contains(normalizedReaction))
            {
                return BadRequest(new { ok = false, message = "Reação inválida." });
            }

            var story = await _context.Stories
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storyId && s.ExpireAt > DateTime.UtcNow && s.Ativo);
            if (story == null)
            {
                return NotFound(new { ok = false, message = "Story não encontrado." });
            }

            var reaction = await _context.StoryReactions
                .FirstOrDefaultAsync(r => r.StoryId == storyId && r.UserId == usuario.Id.ToString());

            if (reaction == null)
            {
                _context.StoryReactions.Add(new StoryReaction
                {
                    StoryId = storyId,
                    UserId = usuario.Id.ToString(),
                    ReactionType = normalizedReaction,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (!string.Equals(reaction.ReactionType, normalizedReaction, StringComparison.OrdinalIgnoreCase))
            {
                reaction.ReactionType = normalizedReaction;
                reaction.CreatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var grouped = await _context.StoryReactions
                .AsNoTracking()
                .Where(r => r.StoryId == storyId)
                .GroupBy(r => r.ReactionType.ToLower())
                .Select(g => new { Tipo = g.Key, Total = g.Count() })
                .ToListAsync();

            return Json(new
            {
                ok = true,
                userReaction = normalizedReaction,
                totalReactions = grouped.Sum(x => x.Total),
                byType = grouped.ToDictionary(x => x.Tipo, x => x.Total)
            });
        }

        [HttpPost("Social/Story/Reply/{storyId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoryReply(int storyId, [FromForm] string message)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var texto = (message ?? string.Empty).Trim();
            if (texto.Length is < 1 or > 1000)
            {
                return BadRequest(new { ok = false, message = "Mensagem inválida." });
            }

            var story = await _context.Stories
                .AsNoTracking()
                .Include(s => s.PerfilSocial)
                .FirstOrDefaultAsync(s => s.Id == storyId && s.ExpireAt > DateTime.UtcNow && s.Ativo);
            if (story == null)
            {
                return NotFound(new { ok = false, message = "Story não encontrado." });
            }

            if (story.UserId == usuario.Id.ToString())
            {
                return BadRequest(new { ok = false, message = "Não é possível responder seu próprio story." });
            }

            _context.StoryReplies.Add(new StoryReply
            {
                StoryId = storyId,
                FromUserId = usuario.Id.ToString(),
                ToUserId = story.UserId,
                Message = texto,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
            await _context.SaveChangesAsync();

            var perfilRemetente = await GarantirPerfilSocialAsync(usuario);
            if (int.TryParse(story.UserId, out var ownerUserId))
            {
                await CriarNotificacaoSocialAsync(
                    ownerUserId,
                    "Responderam seu story",
                    $"{perfilRemetente.NomeExibicao} respondeu seu story.",
                    "StoryReply",
                    Url.Action(nameof(Stories), "Social"));
            }

            return Json(new { ok = true });
        }

        [HttpGet("Social/Story/RepliesInbox")]
        public async Task<IActionResult> StoryRepliesInbox()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var replies = await _context.StoryReplies
                .AsNoTracking()
                .Where(r => r.ToUserId == usuario.Id.ToString())
                .OrderByDescending(r => r.CreatedAt)
                .Take(120)
                .Select(r => new
                {
                    r.Id,
                    r.StoryId,
                    r.FromUserId,
                    r.Message,
                    r.CreatedAt,
                    r.IsRead
                })
                .ToListAsync();

            var fromIds = replies
                .Select(r => int.TryParse(r.FromUserId, out var parsed) ? parsed : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var perfis = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => fromIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId);

            var payload = replies.Select(r =>
            {
                var parsed = int.TryParse(r.FromUserId, out var uid) ? uid : 0;
                perfis.TryGetValue(parsed, out var perfil);
                return new
                {
                    r.Id,
                    r.StoryId,
                    nome = string.IsNullOrWhiteSpace(perfil?.NomeExibicao) ? "Participante" : perfil.NomeExibicao,
                    foto = NormalizarUrlImagemPerfil(perfil?.FotoPerfilUrl),
                    tipo = string.IsNullOrWhiteSpace(perfil?.TipoPerfil) ? "Convidado" : perfil!.TipoPerfil,
                    r.Message,
                    r.CreatedAt,
                    r.IsRead
                };
            }).ToList();

            return Json(payload);
        }

        [HttpGet("Social/Story/MentionSearch")]
        public async Task<IActionResult> StoryMentionSearch([FromQuery] string q)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var termo = (q ?? string.Empty).Trim();
            if (termo.Length < 1)
            {
                return Json(Array.Empty<object>());
            }

            var results = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => !string.IsNullOrWhiteSpace(p.NomeExibicao) && p.NomeExibicao.ToLower().Contains(termo.ToLower()))
                .OrderBy(p => p.NomeExibicao)
                .Take(8)
                .Select(p => new
                {
                    perfilId = p.Id,
                    userId = p.UserId,
                    nome = p.NomeExibicao,
                    tipo = p.TipoPerfil,
                    foto = NormalizarUrlImagemPerfil(p.FotoPerfilUrl)
                })
                .ToListAsync();

            return Json(results);
        }

        [HttpPost("Social/Story/Highlight/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoryHighlightCreate([FromForm] string nome, [FromForm] string? capaUrl)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var nomeLimpo = (nome ?? string.Empty).Trim();
            if (nomeLimpo.Length is < 2 or > 100)
            {
                return BadRequest(new { ok = false, message = "Nome do destaque inválido." });
            }

            var highlight = new StoryHighlight
            {
                UserId = usuario.Id.ToString(),
                Nome = nomeLimpo,
                CapaUrl = string.IsNullOrWhiteSpace(capaUrl) ? null : SocialImagemHelper.NormalizePublicImageUrl(capaUrl, ImagemPostPadrao),
                CreatedAt = DateTime.UtcNow
            };

            _context.StoryHighlights.Add(highlight);
            await _context.SaveChangesAsync();
            return Json(new { ok = true, highlightId = highlight.Id });
        }

        [HttpPost("Social/Story/Highlight/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoryHighlightAdd([FromForm] int highlightId, [FromForm] int storyId)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var highlight = await _context.StoryHighlights
                .FirstOrDefaultAsync(h => h.Id == highlightId && h.UserId == usuario.Id.ToString());
            if (highlight == null)
            {
                return Forbid();
            }

            var story = await _context.Stories
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storyId && s.UserId == usuario.Id.ToString());
            if (story == null)
            {
                return Forbid();
            }

            var exists = await _context.StoryHighlightItems
                .AnyAsync(i => i.HighlightId == highlightId && i.StoryId == storyId);
            if (exists)
            {
                return Json(new { ok = true, alreadyExists = true });
            }

            var ordem = await _context.StoryHighlightItems
                .Where(i => i.HighlightId == highlightId)
                .Select(i => (int?)i.Ordem)
                .MaxAsync() ?? 0;

            _context.StoryHighlightItems.Add(new StoryHighlightItem
            {
                HighlightId = highlightId,
                StoryId = storyId,
                Ordem = ordem + 1
            });

            if (string.IsNullOrWhiteSpace(highlight.CapaUrl))
            {
                highlight.CapaUrl = SocialImagemHelper.NormalizePublicImageUrl(story.MediaUrl, ImagemPostPadrao);
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true });
        }

        [HttpGet("Social/Story/Analytics")]
        public async Task<IActionResult> StoryAnalytics()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var userStories = await _context.Stories
                .AsNoTracking()
                .Include(s => s.Evento)
                .Where(s => s.UserId == usuario.Id.ToString())
                .OrderByDescending(s => s.CreatedAt)
                .Take(120)
                .ToListAsync();

            var storyIds = userStories.Select(s => s.Id).ToList();
            var allViews = await _context.StoryViews
                .AsNoTracking()
                .Where(v => storyIds.Contains(v.StoryId))
                .ToListAsync();

            var allReactions = await _context.StoryReactions
                .AsNoTracking()
                .Where(r => storyIds.Contains(r.StoryId))
                .ToListAsync();

            var viewCountByStory = allViews
                .GroupBy(v => v.StoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            var reactionCountByStory = allReactions
                .GroupBy(r => r.StoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            var perfil = await _context.PerfisSociais.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == usuario.Id);

            var baseCards = userStories.Select(s =>
            {
                var totalViews = viewCountByStory.TryGetValue(s.Id, out var tv) ? tv : 0;
                var totalReactions = reactionCountByStory.TryGetValue(s.Id, out var tr) ? tr : 0;
                return StoryToStatusItemViewModel(
                    s,
                    perfil ?? new PerfilSocial { Id = 0, UserId = usuario.Id, NomeExibicao = usuario.UserName ?? "Participante", TipoPerfil = usuario.TipoUsuario ?? "Convidado", FotoPerfilUrl = FotoPerfilPadrao },
                    true,
                    totalViews,
                    totalReactions,
                    allReactions.Where(r => r.StoryId == s.Id)
                        .GroupBy(r => r.ReactionType.ToLowerInvariant())
                        .ToDictionary(g => g.Key, g => g.Count()),
                    true,
                    null);
            }).ToList();

            var highlights = await _context.StoryHighlights
                .AsNoTracking()
                .Where(h => h.UserId == usuario.Id.ToString())
                .Select(h => new StoryHighlightViewModel
                {
                    Id = h.Id,
                    Nome = h.Nome,
                    CapaUrl = string.IsNullOrWhiteSpace(h.CapaUrl) ? ImagemPostPadrao : h.CapaUrl!,
                    TotalStories = _context.StoryHighlightItems.Count(i => i.HighlightId == h.Id)
                })
                .ToListAsync();

            var lastViewerRaw = await _context.StoryViews
                .AsNoTracking()
                .Where(v => storyIds.Contains(v.StoryId))
                .OrderByDescending(v => v.ViewedAt)
                .Take(12)
                .ToListAsync();

            var lastViewerIds = lastViewerRaw
                .Select(v => int.TryParse(v.UserId, out var parsed) ? parsed : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .Distinct()
                .ToList();
            var viewerPerfis = await _context.PerfisSociais
                .AsNoTracking()
                .Where(p => lastViewerIds.Contains(p.UserId))
                .ToListAsync();
            var viewerPerfisPorId = viewerPerfis.ToDictionary(p => p.UserId.ToString(), p => p);

            var lastViewers = lastViewerRaw.Select(v =>
            {
                viewerPerfisPorId.TryGetValue(v.UserId, out var perfilViewer);
                return new StoryViewerViewModel
                {
                    NomeExibicao = string.IsNullOrWhiteSpace(perfilViewer?.NomeExibicao) ? "Participante" : perfilViewer.NomeExibicao,
                    FotoPerfilUrl = NormalizarUrlImagemPerfil(perfilViewer?.FotoPerfilUrl),
                    TipoPerfil = string.IsNullOrWhiteSpace(perfilViewer?.TipoPerfil) ? "Convidado" : perfilViewer!.TipoPerfil,
                    ViewedAt = v.ViewedAt
                };
            }).ToList();

            var model = new StoryAnalyticsViewModel
            {
                TotalStoriesPostados = userStories.Count,
                TotalVisualizacoes = allViews.Count,
                TotalReacoes = allReactions.Count,
                StoryMaisVisto = baseCards.OrderByDescending(x => x.TotalViews).FirstOrDefault(),
                StoryMaisEngajado = baseCards.OrderByDescending(x => x.EngagementScore).FirstOrDefault(),
                UltimosViewers = lastViewers,
                Destaques = highlights
            };

            return View("Analytics", model);
        }

        [HttpGet]
        public async Task<IActionResult> VisualizarStory(int id)
        {
            var status = await _context.Stories
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.ExpireAt > DateTime.UtcNow && s.Ativo);
            if (status == null)
            {
                return NotFound();
            }

            var usuario = await ObterUsuarioAtualAsync();
            if (usuario != null)
            {
                var ownerId = int.TryParse(status.UserId, out var parsedOwnerId) ? parsedOwnerId : 0;
                if (ownerId != usuario.Id)
                {
                    var jaViu = await _context.StoryViews.AnyAsync(v => v.StoryId == id && v.UserId == usuario.Id.ToString());
                    if (!jaViu)
                    {
                        _context.StoryViews.Add(new StoryView
                        {
                            StoryId = id,
                            UserId = usuario.Id.ToString(),
                            ViewedAt = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction(nameof(Stories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarStatusComoVisualizado(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var statusExiste = await _context.Stories.AnyAsync(s => s.Id == id && s.ExpireAt > DateTime.UtcNow && s.Ativo);
            if (!statusExiste)
            {
                return NotFound();
            }

            var statusOwner = await _context.Stories
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => s.UserId)
                .FirstOrDefaultAsync();

            if (statusOwner == null)
            {
                return NotFound();
            }

            if (statusOwner == usuario.Id.ToString())
            {
                return Ok();
            }

            var jaViu = await _context.StoryViews.AnyAsync(v => v.StoryId == id && v.UserId == usuario.Id.ToString());
            if (!jaViu)
            {
                _context.StoryViews.Add(new StoryView
                {
                    StoryId = id,
                    UserId = usuario.Id.ToString(),
                    ViewedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirStatus(int id)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            var status = await _context.Stories.FirstOrDefaultAsync(s => s.Id == id && s.ExpireAt > DateTime.UtcNow && s.Ativo);
            if (status == null)
            {
                return NotFound();
            }

            if (status.UserId != usuario.Id.ToString())
            {
                return Forbid();
            }

            status.ExpireAt = DateTime.UtcNow;
            status.Ativo = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Stories));
        }

        [HttpGet]
        public async Task<IActionResult> CriarStatus()
        {
            return RedirectToAction(nameof(StoryCreate));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarStatus(IFormFile imagem, string? textoOverlay)
        {
            return await StoryUpload(imagem);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> EventosEmAlta()
        {
            var eventos = await _context.Eventos
                .AsNoTracking()
                .OrderByDescending(e => e.DataEvento)
                .Take(24)
                .ToListAsync();
            return View(eventos);
        }

        private async Task<string> SalvarArquivoSocialAsync(IFormFile arquivo, string pastaRelativa, string fallbackUrl)
        {
            if (arquivo.Length <= 0)
            {
                throw new InvalidOperationException("arquivo inválido para upload");
            }

            if (arquivo.Length > TamanhoMaximoUpload)
            {
                throw new InvalidOperationException("arquivo excede o limite de 8MB");
            }

            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            if (!ExtensoesPermitidas.Contains(extensao))
            {
                throw new InvalidOperationException("extensão de arquivo não permitida");
            }

            var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
            var pastaFisica = Path.Combine(_environment.WebRootPath, pastaRelativa);
            Directory.CreateDirectory(pastaFisica);

            var caminhoFisico = Path.Combine(pastaFisica, nomeArquivo);
            await using var stream = new FileStream(caminhoFisico, FileMode.Create);
            await arquivo.CopyToAsync(stream);

            var relativeUrl = $"/{pastaRelativa.Replace('\\', '/')}/{nomeArquivo}";
            return SocialImagemHelper.NormalizePublicImageUrl(relativeUrl, fallbackUrl);
        }
    }
}
