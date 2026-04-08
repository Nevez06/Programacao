using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
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
        private const string FotoPerfilPadrao = "/uploads/social/defaults/default-profile.svg";
        private const string ImagemPostPadrao = "/uploads/social/defaults/default-post.svg";
        private static readonly string[] CategoriasExplorar =
        [
            "casamento", "aniversário", "corporativo", "formatura", "show", "infantil",
            "decoração", "buffet", "fotografia"
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
            return string.IsNullOrWhiteSpace(perfil?.FotoPerfilUrl) ? FotoPerfilPadrao : perfil.FotoPerfilUrl;
        }

        private static string TempoRelativo(DateTime dataUtc)
        {
            var diff = DateTime.UtcNow - dataUtc;
            if (diff.TotalMinutes < 1) return "agora";
            if (diff.TotalHours < 1) return $"{Math.Max(1, (int)diff.TotalMinutes)} min";
            if (diff.TotalDays < 1) return $"{Math.Max(1, (int)diff.TotalHours)} h";
            return $"{Math.Max(1, (int)diff.TotalDays)} d";
        }

        private async Task<List<SocialStatusItemViewModel>> ObterStoriesAtivosAsync(int? usuarioAtualId)
        {
            var agora = DateTime.UtcNow;
            return await _context.SocialStatus
                .AsNoTracking()
                .Where(s => s.Ativo && s.ExpiraEm > agora)
                .OrderByDescending(s => s.CriadoEm)
                .Select(s => new SocialStatusItemViewModel
                {
                    Id = s.Id,
                    PerfilId = s.PerfilSocialId,
                    UserId = s.UserId,
                    NomePerfil = !string.IsNullOrWhiteSpace(s.PerfilSocial != null ? s.PerfilSocial.NomeExibicao : null)
                        ? s.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(s.User != null ? s.User.UserName : null) ? s.User!.UserName! : (s.User != null ? s.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = s.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.PerfilSocial.FotoPerfilUrl)
                        ? s.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    ImagemUrl = s.ImagemUrl,
                    TextoOverlay = s.TextoOverlay,
                    CriadoEm = s.CriadoEm,
                    Visualizado = usuarioAtualId.HasValue && s.Visualizacoes.Any(v => v.UserId == usuarioAtualId.Value),
                    SequenciaNoPerfil = 1,
                    TotalNoPerfil = 1
                })
                .Take(32)
                .ToListAsync();
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
                .Where(p => p.Ativo)
                .Where(p => !string.IsNullOrWhiteSpace(p.ImagemUrl))
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    AutorUserId = p.UserId,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : (p.User != null ? p.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? p.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = string.IsNullOrWhiteSpace(p.ImagemUrl) ? ImagemPostPadrao : p.ImagemUrl,
                    Categoria = p.Categoria,
                    TipoConteudo = p.TipoConteudo,
                    Localizacao = p.Localizacao,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    UsuarioCurtiu = usuarioId.HasValue && p.Curtidas.Any(c => c.UserId == usuarioId.Value),
                    UsuarioSalvou = usuarioId.HasValue && p.Salvos.Any(s => s.UserId == usuarioId.Value),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId,
                    TipoPerfil = p.PerfilSocial != null ? p.PerfilSocial.TipoPerfil : (p.User != null ? p.User.TipoUsuario ?? "Convidado" : "Convidado"),
                    Cidade = p.PerfilSocial != null ? p.PerfilSocial.Cidade : null,
                    PostRelacionadoAoUsuario = usuarioId.HasValue && (p.UserId == usuarioId.Value || (p.Evento != null && p.Evento.OrganizadorId == usuarioId.Value))
                });

            queryPosts = ordemNormalizada == "populares"
                ? queryPosts.OrderByDescending(p => p.PostRelacionadoAoUsuario)
                    .ThenByDescending(p => p.TotalCurtidas)
                    .ThenByDescending(p => p.TotalComentarios)
                    .ThenByDescending(p => p.DataCriacao)
                : queryPosts.OrderByDescending(p => p.PostRelacionadoAoUsuario)
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

            var model = new FeedSocialViewModel
            {
                Posts = posts,
                Stories = await ObterStoriesAtivosAsync(usuarioId),
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

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Explorar()
        {
            var usuarioAtual = await ObterUsuarioAtualAsync();
            var recentes = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo)
                .OrderByDescending(p => p.CriadoEm)
                .Take(12)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : (p.User != null ? p.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? p.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = string.IsNullOrWhiteSpace(p.ImagemUrl) ? ImagemPostPadrao : p.ImagemUrl,
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
                .Take(12)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : (p.User != null ? p.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? p.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = string.IsNullOrWhiteSpace(p.ImagemUrl) ? ImagemPostPadrao : p.ImagemUrl,
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

            var model = new ExplorarViewModel
            {
                PostsRecentes = recentes,
                PostsPopulares = populares,
                PerfisOrganizadores = await _context.PerfisSociais
                    .AsNoTracking()
                    .Where(p => p.TipoPerfil == "Organizador")
                    .OrderByDescending(p => p.Posts.Count)
                    .Take(8)
                    .Select(p => new ExplorarPerfilCardViewModel
                    {
                        PerfilId = p.Id,
                        NomeExibicao = p.NomeExibicao,
                        TipoPerfil = p.TipoPerfil,
                        Cidade = p.Cidade,
                        Bio = p.Bio,
                        FotoPerfilUrl = p.FotoPerfilUrl,
                        TotalPosts = p.Posts.Count
                    })
                    .ToListAsync(),
                PerfisFornecedores = await _context.PerfisSociais
                    .AsNoTracking()
                    .Where(p => p.TipoPerfil == "Fornecedor")
                    .OrderByDescending(p => p.Posts.Count)
                    .Take(8)
                    .Select(p => new ExplorarPerfilCardViewModel
                    {
                        PerfilId = p.Id,
                        NomeExibicao = p.NomeExibicao,
                        TipoPerfil = p.TipoPerfil,
                        Cidade = p.Cidade,
                        Bio = p.Bio,
                        FotoPerfilUrl = p.FotoPerfilUrl,
                        TotalPosts = p.Posts.Count
                    })
                    .ToListAsync(),
                EventosDestaque = await _context.Eventos
                    .AsNoTracking()
                    .OrderByDescending(e => _context.SocialPosts.Count(p => p.Ativo && p.EventoId == e.Id))
                    .ThenByDescending(e => e.DataEvento)
                    .Take(8)
                    .Select(e => new ExplorarEventoCardViewModel
                    {
                        EventoId = e.Id,
                        NomeEvento = e.NomeEvento,
                        TipoEvento = e.TipoEvento,
                        DataEvento = e.DataEvento,
                        ImagemCapa = e.ImagemCapa,
                        TotalPostsRelacionados = _context.SocialPosts.Count(p => p.Ativo && p.EventoId == e.Id)
                    })
                    .ToListAsync(),
                CategoriasDestaque = new List<string>(CategoriasExplorar)
                {
                    "música", "making of", "bastidores"
                },
                TotalPostsRecentes = recentes.Count,
                TotalPostsPopulares = populares.Count
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Perfil(int id)
        {
            var perfil = await _context.PerfisSociais
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (perfil == null)
            {
                return NotFound();
            }

            var posts = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.PerfilSocialId == id && p.Ativo)
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync();

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

            var storiesAtivos = await _context.SocialStatus
                .AsNoTracking()
                .Where(s => s.Ativo && s.PerfilSocialId == id && s.ExpiraEm > DateTime.UtcNow)
                .OrderByDescending(s => s.CriadoEm)
                .Select(s => new SocialStatusItemViewModel
                {
                    Id = s.Id,
                    PerfilId = s.PerfilSocialId,
                    UserId = s.UserId,
                    NomePerfil = !string.IsNullOrWhiteSpace(s.PerfilSocial != null ? s.PerfilSocial.NomeExibicao : null)
                        ? s.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(s.User != null ? s.User.UserName : null) ? s.User!.UserName! : (s.User != null ? s.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = s.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.PerfilSocial.FotoPerfilUrl)
                        ? s.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    ImagemUrl = s.ImagemUrl,
                    TextoOverlay = s.TextoOverlay,
                    CriadoEm = s.CriadoEm,
                    Visualizado = false,
                    SequenciaNoPerfil = 1,
                    TotalNoPerfil = 1
                })
                .ToListAsync();

            var perfilAtual = await ObterUsuarioAtualAsync();
            var ehDono = perfilAtual != null && perfilAtual.Id == perfil.UserId;

            var postsSalvos = new List<FeedPostViewModel>();
            if (ehDono)
            {
                postsSalvos = await _context.SocialPostsSalvos
                    .AsNoTracking()
                    .Where(s => s.UserId == perfil.UserId && s.Post != null && s.Post.Ativo)
                    .OrderByDescending(s => s.CriadoEm)
                    .Take(6)
                    .Select(s => new FeedPostViewModel
                    {
                        PostId = s.PostId,
                        NomeAutor = s.Post != null && s.Post.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.Post.PerfilSocial.NomeExibicao)
                            ? s.Post.PerfilSocial.NomeExibicao
                            : (s.Post != null && s.Post.User != null && !string.IsNullOrWhiteSpace(s.Post.User.UserName) ? s.Post.User.UserName! : "Participante"),
                        FotoPerfilUrl = s.Post != null && s.Post.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.Post.PerfilSocial.FotoPerfilUrl)
                            ? s.Post.PerfilSocial.FotoPerfilUrl
                            : FotoPerfilPadrao,
                        Legenda = s.Post != null ? s.Post.Legenda : string.Empty,
                        ImagemUrl = s.Post == null || string.IsNullOrWhiteSpace(s.Post.ImagemUrl) ? ImagemPostPadrao : s.Post.ImagemUrl,
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
            }

            var model = new PerfilSocialViewModel
            {
                Id = perfil.Id,
                NomeExibicao = perfil.NomeExibicao,
                Bio = perfil.Bio,
                FotoPerfilUrl = string.IsNullOrWhiteSpace(perfil.FotoPerfilUrl) ? FotoPerfilPadrao : perfil.FotoPerfilUrl,
                TipoPerfil = perfil.TipoPerfil,
                Cidade = perfil.Cidade,
                Instagram = perfil.Instagram,
                Site = perfil.Site,
                TotalPosts = posts.Count,
                TotalCurtidasRecebidas = totalCurtidas,
                TotalStoriesAtivos = storiesAtivos.Count,
                TotalEventosRelacionados = eventosVinculados.Count,
                EhPerfilDoUsuarioLogado = ehDono,
                UsernamePublico = $"@{(perfil.NomeExibicao ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", ".")}",
                ListaPosts = posts,
                EventosVinculados = eventosVinculados,
                StoriesAtivos = storiesAtivos,
                PostsSalvos = postsSalvos
            };

            return View(model);
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
                Bio = perfil.Bio,
                FotoPerfilUrl = perfil.FotoPerfilUrl,
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
            perfil.Bio = model.Bio;
            perfil.Cidade = model.Cidade;
            perfil.Instagram = model.Instagram;
            perfil.Site = model.Site;
            perfil.AtualizadoEm = DateTime.UtcNow;

            if (model.NovaFotoPerfil != null)
            {
                try
                {
                    perfil.FotoPerfilUrl = await SalvarArquivoSocialAsync(model.NovaFotoPerfil, PastaUploadPerfis);
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
                    ? await SalvarArquivoSocialAsync(model.Imagem, PastaUploadPosts)
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
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

            if (post == null || post.User == null || post.PerfilSocial == null)
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
                            ? s.PerfilSocial.FotoPerfilUrl
                            : FotoPerfilPadrao,
                        ImagemUrl = s.ImagemUrl,
                        TextoOverlay = s.TextoOverlay,
                        CriadoEm = s.CriadoEm,
                        Visualizado = usuarioAtual != null && s.Visualizacoes.Any(v => v.UserId == usuarioAtual.Id)
                    })
                    .ToListAsync()
            };

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
                .Where(p => p.UserId == usuario.Id && p.Ativo)
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync();

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

            post.Ativo = false;
            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
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

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && p.Ativo);
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

            var post = await _context.SocialPosts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == usuario.Id && p.Ativo);
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

            var postExiste = await _context.SocialPosts.AnyAsync(p => p.Id == id && p.Ativo);
            if (!postExiste)
            {
                return NotFound();
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

            var postExiste = await _context.SocialPosts.AnyAsync(p => p.Id == id && p.Ativo);
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

            var postExiste = await _context.SocialPosts.AnyAsync(p => p.Id == id && p.Ativo);
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
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
            if (post == null)
            {
                return NotFound();
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
                .Where(p => p.Ativo && p.EventoId == eventoId)
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = !string.IsNullOrWhiteSpace(p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null)
                        ? p.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(p.User != null ? p.User.UserName : null) ? p.User!.UserName! : (p.User != null ? p.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = p.PerfilSocial != null && !string.IsNullOrWhiteSpace(p.PerfilSocial.FotoPerfilUrl)
                        ? p.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = string.IsNullOrWhiteSpace(p.ImagemUrl) ? ImagemPostPadrao : p.ImagemUrl,
                    Categoria = p.Categoria,
                    TipoConteudo = p.TipoConteudo,
                    Localizacao = p.Localizacao,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId,
                    TipoPerfil = p.PerfilSocial != null ? p.PerfilSocial.TipoPerfil : (p.User != null ? p.User.TipoUsuario ?? "Convidado" : "Convidado"),
                    Cidade = p.PerfilSocial != null ? p.PerfilSocial.Cidade : null
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

            var postExiste = await _context.SocialPosts.AnyAsync(p => p.Id == id && p.Ativo);
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
                .Where(s => s.UserId == usuario.Id && s.Post != null && s.Post.Ativo)
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
                        ? s.Post.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    Legenda = s.Post != null ? s.Post.Legenda : string.Empty,
                    ImagemUrl = s.Post == null || string.IsNullOrWhiteSpace(s.Post.ImagemUrl) ? ImagemPostPadrao : s.Post.ImagemUrl,
                    Categoria = s.Post != null ? s.Post.Categoria : null,
                    TipoConteudo = s.Post != null ? s.Post.TipoConteudo : null,
                    Localizacao = s.Post != null ? s.Post.Localizacao : null,
                    DataCriacao = s.Post != null ? s.Post.CriadoEm : DateTime.UtcNow,
                    TotalCurtidas = s.Post != null ? s.Post.Curtidas.Count : 0,
                    TotalComentarios = s.Post != null ? s.Post.Comentarios.Count(c => c.Ativo) : 0,
                    UsuarioCurtiu = s.Post != null && s.Post.Curtidas.Any(c => c.UserId == usuario.Id),
                    UsuarioSalvou = true,
                    EventoId = s.Post != null ? s.Post.EventoId : null,
                    NomeEvento = s.Post != null && s.Post.Evento != null ? s.Post.Evento.NomeEvento : null,
                    PerfilId = s.Post != null ? s.Post.PerfilSocialId : 0,
                    TipoPerfil = s.Post != null && s.Post.PerfilSocial != null ? s.Post.PerfilSocial.TipoPerfil : (s.Post != null && s.Post.User != null ? s.Post.User.TipoUsuario ?? "Convidado" : "Convidado"),
                    Cidade = s.Post != null && s.Post.PerfilSocial != null ? s.Post.PerfilSocial.Cidade : null
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

            var stories = await _context.SocialStatus
                .AsNoTracking()
                .Where(s => s.Ativo && s.ExpiraEm > DateTime.UtcNow)
                .OrderByDescending(s => s.CriadoEm)
                .Select(s => new SocialStatusItemViewModel
                {
                    Id = s.Id,
                    PerfilId = s.PerfilSocialId,
                    UserId = s.UserId,
                    NomePerfil = !string.IsNullOrWhiteSpace(s.PerfilSocial != null ? s.PerfilSocial.NomeExibicao : null)
                        ? s.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(s.User != null ? s.User.UserName : null) ? s.User!.UserName! : (s.User != null ? s.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = s.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.PerfilSocial.FotoPerfilUrl)
                        ? s.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    ImagemUrl = s.ImagemUrl,
                    TextoOverlay = s.TextoOverlay,
                    CriadoEm = s.CriadoEm,
                    Visualizado = usuarioId.HasValue && s.Visualizacoes.Any(v => v.UserId == usuarioId.Value),
                    SequenciaNoPerfil = 1,
                    TotalNoPerfil = 1
                })
                .ToListAsync();

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

        [HttpGet]
        public async Task<IActionResult> Stories()
        {
            var usuarioAtual = await ObterUsuarioAtualAsync();
            if (usuarioAtual != null)
            {
                await GarantirPerfilSocialAsync(usuarioAtual);
            }
            var stories = await _context.SocialStatus
                .AsNoTracking()
                .Where(s => s.Ativo && s.ExpiraEm > DateTime.UtcNow)
                .OrderByDescending(s => s.CriadoEm)
                .Select(s => new SocialStatusItemViewModel
                {
                    Id = s.Id,
                    PerfilId = s.PerfilSocialId,
                    UserId = s.UserId,
                    NomePerfil = !string.IsNullOrWhiteSpace(s.PerfilSocial != null ? s.PerfilSocial.NomeExibicao : null)
                        ? s.PerfilSocial!.NomeExibicao
                        : (!string.IsNullOrWhiteSpace(s.User != null ? s.User.UserName : null) ? s.User!.UserName! : (s.User != null ? s.User.Email ?? "Participante" : "Participante")),
                    FotoPerfilUrl = s.PerfilSocial != null && !string.IsNullOrWhiteSpace(s.PerfilSocial.FotoPerfilUrl)
                        ? s.PerfilSocial.FotoPerfilUrl
                        : FotoPerfilPadrao,
                    ImagemUrl = s.ImagemUrl,
                    TextoOverlay = s.TextoOverlay,
                    CriadoEm = s.CriadoEm,
                    Visualizado = usuarioAtual != null && s.Visualizacoes.Any(v => v.UserId == usuarioAtual.Id),
                    SequenciaNoPerfil = 1,
                    TotalNoPerfil = 1
                })
                .ToListAsync();
            return View(stories);
        }

        [HttpGet]
        public async Task<IActionResult> VisualizarStory(int id)
        {
            var status = await _context.SocialStatus
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Ativo && s.ExpiraEm > DateTime.UtcNow);
            if (status == null)
            {
                return NotFound();
            }

            var usuario = await ObterUsuarioAtualAsync();
            if (usuario != null)
            {
                var jaViu = await _context.SocialStatusVisualizacoes.AnyAsync(v => v.SocialStatusId == id && v.UserId == usuario.Id);
                if (!jaViu)
                {
                    _context.SocialStatusVisualizacoes.Add(new SocialStatusVisualizacao
                    {
                        SocialStatusId = id,
                        UserId = usuario.Id,
                        VisualizadoEm = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
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

            var statusExiste = await _context.SocialStatus.AnyAsync(s => s.Id == id && s.Ativo && s.ExpiraEm > DateTime.UtcNow);
            if (!statusExiste)
            {
                return NotFound();
            }

            var jaViu = await _context.SocialStatusVisualizacoes.AnyAsync(v => v.SocialStatusId == id && v.UserId == usuario.Id);
            if (!jaViu)
            {
                _context.SocialStatusVisualizacoes.Add(new SocialStatusVisualizacao
                {
                    SocialStatusId = id,
                    UserId = usuario.Id,
                    VisualizadoEm = DateTime.UtcNow
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

            var status = await _context.SocialStatus.FirstOrDefaultAsync(s => s.Id == id && s.Ativo);
            if (status == null)
            {
                return NotFound();
            }

            if (status.UserId != usuario.Id)
            {
                return Forbid();
            }

            status.Ativo = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Stories));
        }

        [HttpGet]
        public async Task<IActionResult> CriarStatus()
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            await GarantirPerfilSocialAsync(usuario);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarStatus(IFormFile imagem, string? textoOverlay)
        {
            var usuario = await ObterUsuarioAtualAsync();
            if (usuario == null)
            {
                return Challenge();
            }

            if (imagem == null)
            {
                ModelState.AddModelError("imagem", "A imagem do status é obrigatória.");
                return View();
            }

            string imagemUrl;
            try
            {
                imagemUrl = await SalvarArquivoSocialAsync(imagem, PastaUploadStatus);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("imagem", ex.Message);
                return View();
            }

            var perfil = await GarantirPerfilSocialAsync(usuario);
            _context.SocialStatus.Add(new SocialStatus
            {
                UserId = usuario.Id,
                PerfilSocialId = perfil.Id,
                ImagemUrl = imagemUrl,
                TextoOverlay = string.IsNullOrWhiteSpace(textoOverlay) ? null : textoOverlay.Trim(),
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddHours(24),
                Ativo = true
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Feed));
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

        private async Task<string> SalvarArquivoSocialAsync(IFormFile arquivo, string pastaRelativa)
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

            return $"/{pastaRelativa.Replace('\\', '/')}/{nomeArquivo}";
        }
    }
}
