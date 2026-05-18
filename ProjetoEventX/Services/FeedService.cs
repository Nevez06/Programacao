using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Feed;
using ProjetoEventX.Helpers;
using ProjetoEventX.Models;
using ProjetoEventX.Security;

namespace ProjetoEventX.Services
{
    public class FeedService
    {
        private const string FotoPerfilPadrao = "/uploads/social/defaults/default-profile.svg";
        private const string ImagemPostPadrao = "/uploads/social/defaults/default-post.svg";

        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedService(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }

        public async Task<PerfilSocial> EnsurePerfilSocialAsync(ApplicationUser user)
        {
            var perfil = await _context.PerfisSociais.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (perfil != null)
            {
                return perfil;
            }

            perfil = new PerfilSocial
            {
                UserId = user.Id,
                NomeExibicao = user.UserName ?? $"Usuario {user.Id}",
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

        public async Task<IReadOnlyList<FeedPostDto>> GetFeedAsync(ApplicationUser currentUser, int take = 40)
        {
            var normalizedTake = Math.Clamp(take, 1, 100);
            var rawPosts = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo && !p.IsArchived && !p.IsDeleted)
                .OrderByDescending(p => p.IsPinned)
                .ThenByDescending(p => p.CriadoEm)
                .Take(normalizedTake)
                .Select(p => new
                {
                    Id = p.Id,
                    AutorUserId = p.UserId,
                    PerfilNomeExibicao = p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : null,
                    PerfilFotoUrl = p.PerfilSocial != null ? p.PerfilSocial.FotoPerfilUrl : null,
                    UserName = p.User != null ? p.User.UserName : null,
                    UserEmail = p.User != null ? p.User.Email : null,
                    Titulo = p.Titulo,
                    Conteudo = p.Legenda,
                    ImagemUrl = p.ImagemUrl,
                    Categoria = p.Categoria,
                    TipoConteudo = p.TipoConteudo,
                    Localizacao = p.Localizacao,
                    DataCriacao = p.CriadoEm,
                    DataAtualizacao = p.AtualizadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    UsuarioCurtiu = p.Curtidas.Any(c => c.UserId == currentUser.Id),
                    IsOwner = p.UserId == currentUser.Id,
                    CommentsEnabled = p.CommentsEnabled,
                    IsPinned = p.IsPinned,
                    IsArchived = p.IsArchived,
                    HideLikesCount = p.HideLikesCount,
                    HideSharesCount = p.HideSharesCount,
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null
                })
                .ToListAsync();

            if (rawPosts.Count == 0)
            {
                return Array.Empty<FeedPostDto>();
            }

            var posts = rawPosts.Select(p => new FeedPostDto
            {
                Id = p.Id,
                AutorUserId = p.AutorUserId,
                AutorNome = ResolveDisplayName(p.PerfilNomeExibicao, p.UserName, p.UserEmail),
                AutorFotoUrl = NormalizeProfileImageUrl(p.PerfilFotoUrl),
                Titulo = p.Titulo,
                Conteudo = p.Conteudo,
                ImagemUrl = NormalizePostImageUrl(p.ImagemUrl),
                Categoria = p.Categoria,
                TipoConteudo = p.TipoConteudo,
                Localizacao = p.Localizacao,
                DataCriacao = p.DataCriacao,
                DataAtualizacao = p.DataAtualizacao,
                TotalCurtidas = p.TotalCurtidas,
                TotalComentarios = p.TotalComentarios,
                UsuarioCurtiu = p.UsuarioCurtiu,
                IsOwner = p.IsOwner,
                CommentsEnabled = p.CommentsEnabled,
                IsPinned = p.IsPinned,
                IsArchived = p.IsArchived,
                HideLikesCount = p.HideLikesCount,
                HideSharesCount = p.HideSharesCount,
                EventoId = p.EventoId,
                NomeEvento = p.NomeEvento
            }).ToList();

            var postIds = posts.Select(p => p.Id).ToList();
            var rawComments = await _context.SocialComentarios
                .AsNoTracking()
                .Where(c => c.Ativo && postIds.Contains(c.PostId))
                .OrderByDescending(c => c.CriadoEm)
                .Select(c => new
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    PerfilNomeExibicao = c.PerfilSocial != null ? c.PerfilSocial.NomeExibicao : null,
                    UserName = c.User != null ? c.User.UserName : null,
                    UserEmail = c.User != null ? c.User.Email : null,
                    Texto = c.Texto,
                    CriadoEm = c.CriadoEm,
                })
                .ToListAsync();

            var comments = rawComments.Select(c => new PostCommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                UserId = c.UserId,
                NomeAutor = ResolveDisplayName(c.PerfilNomeExibicao, c.UserName, c.UserEmail),
                Texto = c.Texto,
                CriadoEm = c.CriadoEm,
                IsOwner = c.UserId == currentUser.Id
            }).ToList();

            var commentsByPost = comments
                .GroupBy(c => c.PostId)
                .ToDictionary(g => g.Key, g => g.Take(2).ToList());

            foreach (var post in posts)
            {
                if (commentsByPost.TryGetValue(post.Id, out var preview))
                {
                    post.Comentarios = preview;
                }
            }

            return posts;
        }

        public async Task<FeedPostDto?> GetPostDetailsAsync(ApplicationUser currentUser, int postId)
        {
            if (postId <= 0)
            {
                return null;
            }

            var post = await _context.SocialPosts
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.PerfilSocial)
                .Include(p => p.Evento)
                .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);

            if (post == null)
            {
                return null;
            }

            var isOwner = post.UserId == currentUser.Id;
            if (!IsVisibleToPublic(post) && !isOwner)
            {
                return null;
            }

            var rawComments = await _context.SocialComentarios
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.PerfilSocial)
                .Where(c => c.PostId == postId && c.Ativo)
                .OrderByDescending(c => c.CriadoEm)
                .Select(c => new
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    PerfilNomeExibicao = c.PerfilSocial != null ? c.PerfilSocial.NomeExibicao : null,
                    UserName = c.User != null ? c.User.UserName : null,
                    UserEmail = c.User != null ? c.User.Email : null,
                    Texto = c.Texto,
                    CriadoEm = c.CriadoEm,
                })
                .ToListAsync();

            var comments = rawComments.Select(c => new PostCommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                UserId = c.UserId,
                NomeAutor = ResolveDisplayName(c.PerfilNomeExibicao, c.UserName, c.UserEmail),
                Texto = c.Texto,
                CriadoEm = c.CriadoEm,
                IsOwner = c.UserId == currentUser.Id
            }).ToList();

            return new FeedPostDto
            {
                Id = post.Id,
                AutorUserId = post.UserId,
                AutorNome = ResolveDisplayName(post.PerfilSocial, post.User),
                AutorFotoUrl = NormalizeProfileImageUrl(post.PerfilSocial?.FotoPerfilUrl),
                Titulo = post.Titulo,
                Conteudo = post.Legenda,
                ImagemUrl = NormalizePostImageUrl(post.ImagemUrl),
                Categoria = post.Categoria,
                TipoConteudo = post.TipoConteudo,
                Localizacao = post.Localizacao,
                DataCriacao = post.CriadoEm,
                DataAtualizacao = post.AtualizadoEm,
                TotalCurtidas = await _context.SocialCurtidas.CountAsync(c => c.PostId == post.Id),
                TotalComentarios = comments.Count,
                UsuarioCurtiu = await _context.SocialCurtidas.AnyAsync(c => c.PostId == post.Id && c.UserId == currentUser.Id),
                IsOwner = isOwner,
                CommentsEnabled = post.CommentsEnabled,
                IsPinned = post.IsPinned,
                IsArchived = post.IsArchived,
                HideLikesCount = post.HideLikesCount,
                HideSharesCount = post.HideSharesCount,
                EventoId = post.EventoId,
                NomeEvento = post.Evento?.NomeEvento,
                Comentarios = comments
            };
        }

        public async Task<FeedPostDto> CreatePostAsync(ApplicationUser currentUser, CreatePostDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Conteudo))
            {
                throw new InvalidOperationException("Conteudo do post e obrigatorio.");
            }

            if (!SecurityValidator.IsValidInput(request.Conteudo, allowHtml: true))
            {
                throw new InvalidOperationException("Conteudo do post contem caracteres invalidos.");
            }

            if (request.EventoId.HasValue)
            {
                var eventoExiste = await _context.Eventos.AnyAsync(e => e.Id == request.EventoId.Value);
                if (!eventoExiste)
                {
                    throw new InvalidOperationException("Evento informado nao existe.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.ImagemUrl) && !SecurityValidator.IsValidInput(request.ImagemUrl))
            {
                throw new InvalidOperationException("ImagemUrl invalida.");
            }

            var perfil = await EnsurePerfilSocialAsync(currentUser);
            var post = new SocialPost
            {
                UserId = currentUser.Id,
                PerfilSocialId = perfil.Id,
                EventoId = request.EventoId,
                Titulo = SanitizeOptional(request.Titulo),
                Legenda = SecurityValidator.SanitizeHtml(request.Conteudo),
                Categoria = SanitizeOptional(request.Categoria),
                TipoConteudo = SanitizeOptional(request.TipoConteudo),
                Localizacao = SanitizeOptional(request.Localizacao),
                ImagemUrl = string.IsNullOrWhiteSpace(request.ImagemUrl)
                    ? ImagemPostPadrao
                    : SecurityValidator.SanitizeInput(request.ImagemUrl),
                CommentsEnabled = request.CommentsEnabled ?? true,
                CriadoEm = DateTime.UtcNow,
                AtualizadoEm = DateTime.UtcNow,
                Ativo = true
            };

            _context.SocialPosts.Add(post);
            await _context.SaveChangesAsync();

            return await GetPostDetailsAsync(currentUser, post.Id)
                ?? throw new InvalidOperationException("Nao foi possivel carregar o post criado.");
        }

        public async Task<FeedPostDto?> UpdatePostAsync(ApplicationUser currentUser, int postId, UpdatePostDto request)
        {
            if (postId <= 0)
            {
                return null;
            }

            var post = await _context.SocialPosts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == currentUser.Id && !p.IsDeleted);

            if (post == null)
            {
                return null;
            }

            if (request.EventoId.HasValue)
            {
                var eventoExiste = await _context.Eventos.AnyAsync(e => e.Id == request.EventoId.Value);
                if (!eventoExiste)
                {
                    throw new InvalidOperationException("Evento informado nao existe.");
                }
                post.EventoId = request.EventoId;
            }

            if (request.Titulo != null)
            {
                post.Titulo = SanitizeOptional(request.Titulo);
            }

            if (request.Conteudo != null)
            {
                if (string.IsNullOrWhiteSpace(request.Conteudo))
                {
                    throw new InvalidOperationException("Conteudo nao pode ficar vazio.");
                }

                if (!SecurityValidator.IsValidInput(request.Conteudo, allowHtml: true))
                {
                    throw new InvalidOperationException("Conteudo do post contem caracteres invalidos.");
                }

                post.Legenda = SecurityValidator.SanitizeHtml(request.Conteudo);
            }

            if (request.ImagemUrl != null)
            {
                if (!string.IsNullOrWhiteSpace(request.ImagemUrl) && !SecurityValidator.IsValidInput(request.ImagemUrl))
                {
                    throw new InvalidOperationException("ImagemUrl invalida.");
                }

                post.ImagemUrl = string.IsNullOrWhiteSpace(request.ImagemUrl)
                    ? ImagemPostPadrao
                    : SecurityValidator.SanitizeInput(request.ImagemUrl);
            }

            if (request.Categoria != null)
            {
                post.Categoria = SanitizeOptional(request.Categoria);
            }

            if (request.TipoConteudo != null)
            {
                post.TipoConteudo = SanitizeOptional(request.TipoConteudo);
            }

            if (request.Localizacao != null)
            {
                post.Localizacao = SanitizeOptional(request.Localizacao);
            }

            if (request.CommentsEnabled.HasValue)
            {
                post.CommentsEnabled = request.CommentsEnabled.Value;
            }

            post.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetPostDetailsAsync(currentUser, post.Id);
        }

        public async Task<bool> DeletePostAsync(ApplicationUser currentUser, int postId)
        {
            if (postId <= 0)
            {
                return false;
            }

            var post = await _context.SocialPosts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == currentUser.Id && !p.IsDeleted);

            if (post == null)
            {
                return false;
            }

            post.IsDeleted = true;
            post.Ativo = false;
            post.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool UsuarioCurtiu, int TotalCurtidas)?> ToggleLikeAsync(
            ApplicationUser currentUser,
            int postId,
            bool? forceLike)
        {
            if (postId <= 0)
            {
                return null;
            }

            var post = await _context.SocialPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId && p.Ativo && !p.IsArchived && !p.IsDeleted);

            if (post == null)
            {
                return null;
            }

            var existingLike = await _context.SocialCurtidas
                .FirstOrDefaultAsync(c => c.PostId == postId && c.UserId == currentUser.Id);

            var shouldLike = forceLike ?? existingLike == null;
            if (shouldLike && existingLike == null)
            {
                _context.SocialCurtidas.Add(new SocialCurtida
                {
                    PostId = postId,
                    UserId = currentUser.Id,
                    CriadoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            else if (!shouldLike && existingLike != null)
            {
                _context.SocialCurtidas.Remove(existingLike);
                await _context.SaveChangesAsync();
            }

            var totalCurtidas = await _context.SocialCurtidas.CountAsync(c => c.PostId == postId);
            var usuarioCurtiu = await _context.SocialCurtidas.AnyAsync(c => c.PostId == postId && c.UserId == currentUser.Id);
            return (usuarioCurtiu, totalCurtidas);
        }

        public async Task<PostCommentDto?> AddCommentAsync(ApplicationUser currentUser, int postId, string texto)
        {
            if (postId <= 0)
            {
                return null;
            }

            var textoLimpo = texto?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(textoLimpo))
            {
                throw new InvalidOperationException("Comentario vazio.");
            }

            if (!SecurityValidator.IsValidInput(textoLimpo))
            {
                throw new InvalidOperationException("Comentario contem caracteres invalidos.");
            }

            var post = await _context.SocialPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId && p.Ativo && !p.IsArchived && !p.IsDeleted);

            if (post == null)
            {
                return null;
            }

            if (!post.CommentsEnabled)
            {
                throw new InvalidOperationException("Comentarios desativados para esta publicacao.");
            }

            var perfil = await EnsurePerfilSocialAsync(currentUser);
            var comment = new SocialComentario
            {
                PostId = postId,
                UserId = currentUser.Id,
                PerfilSocialId = perfil.Id,
                Texto = SecurityValidator.SanitizeInput(textoLimpo),
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            };

            _context.SocialComentarios.Add(comment);
            await _context.SaveChangesAsync();

            return new PostCommentDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserId = comment.UserId,
                NomeAutor = ResolveDisplayName(perfil, currentUser),
                Texto = comment.Texto,
                CriadoEm = comment.CriadoEm,
                IsOwner = true
            };
        }

        private static string ResolveDisplayName(string? perfilNomeExibicao, string? userName, string? userEmail)
        {
            if (!string.IsNullOrWhiteSpace(perfilNomeExibicao))
            {
                return perfilNomeExibicao;
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName;
            }

            return string.IsNullOrWhiteSpace(userEmail) ? "Participante" : userEmail;
        }

        private static string ResolveDisplayName(PerfilSocial? perfil, ApplicationUser? user)
        {
            return ResolveDisplayName(perfil?.NomeExibicao, user?.UserName, user?.Email);
        }

        private static string NormalizePostImageUrl(string? imagemUrl)
        {
            return SocialImagemHelper.NormalizePublicImageUrl(imagemUrl, ImagemPostPadrao);
        }

        private static string NormalizeProfileImageUrl(string? imagemUrl)
        {
            return SocialImagemHelper.NormalizePublicImageUrl(imagemUrl, FotoPerfilPadrao);
        }

        private static bool IsVisibleToPublic(SocialPost post)
        {
            return post.Ativo && !post.IsArchived && !post.IsDeleted;
        }

        private static string? SanitizeOptional(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return SecurityValidator.SanitizeInput(value);
        }
    }
}
