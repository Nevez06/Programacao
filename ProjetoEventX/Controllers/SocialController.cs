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
        private const string FotoPerfilPadrao = "/uploads/social/defaults/default-profile.svg";
        private const string ImagemPostPadrao = "/uploads/social/defaults/default-post.svg";

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
        public async Task<IActionResult> Feed()
        {
            var usuarioAtual = await ObterUsuarioAtualAsync();
            var usuarioId = usuarioAtual?.Id;

            var posts = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo)
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : (p.User != null ? p.User.UserName ?? "Usuário" : "Usuário"),
                    FotoPerfilUrl = p.PerfilSocial != null ? p.PerfilSocial.FotoPerfilUrl : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = string.IsNullOrWhiteSpace(p.ImagemUrl) ? ImagemPostPadrao : p.ImagemUrl,
                    Categoria = p.Categoria,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    UsuarioCurtiu = usuarioId.HasValue && p.Curtidas.Any(c => c.UserId == usuarioId.Value),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId
                })
                .ToListAsync();

            return View(posts);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Explorar()
        {
            var recentes = await _context.SocialPosts
                .AsNoTracking()
                .Where(p => p.Ativo)
                .OrderByDescending(p => p.CriadoEm)
                .Take(12)
                .Select(p => new FeedPostViewModel
                {
                    PostId = p.Id,
                    NomeAutor = p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : (p.User != null ? p.User.UserName ?? "Usuário" : "Usuário"),
                    FotoPerfilUrl = p.PerfilSocial != null ? p.PerfilSocial.FotoPerfilUrl : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = string.IsNullOrWhiteSpace(p.ImagemUrl) ? ImagemPostPadrao : p.ImagemUrl,
                    Categoria = p.Categoria,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId
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
                    NomeAutor = p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : (p.User != null ? p.User.UserName ?? "Usuário" : "Usuário"),
                    FotoPerfilUrl = p.PerfilSocial != null ? p.PerfilSocial.FotoPerfilUrl : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = string.IsNullOrWhiteSpace(p.ImagemUrl) ? ImagemPostPadrao : p.ImagemUrl,
                    Categoria = p.Categoria,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId
                })
                .ToListAsync();

            var model = new ExplorarViewModel
            {
                PostsRecentes = recentes,
                PostsPopulares = populares,
                PerfisOrganizadores = await _context.PerfisSociais.AsNoTracking().Where(p => p.TipoPerfil == "Organizador").Take(8).ToListAsync(),
                PerfisFornecedores = await _context.PerfisSociais.AsNoTracking().Where(p => p.TipoPerfil == "Fornecedor").Take(8).ToListAsync(),
                EventosDestaque = await _context.Eventos.AsNoTracking().OrderByDescending(e => e.DataEvento).Take(6).ToListAsync()
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
                ListaPosts = posts,
                EventosVinculados = eventosVinculados
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
                .Where(c => c.PostId == id && c.Ativo)
                .OrderByDescending(c => c.CriadoEm)
                .ToListAsync();

            var model = new PostDetalheViewModel
            {
                Post = post,
                Autor = post.User,
                Perfil = post.PerfilSocial,
                ListaComentarios = comentarios,
                TotalCurtidas = await _context.SocialCurtidas.CountAsync(c => c.PostId == id),
                UsuarioCurtiu = usuarioAtual != null && await _context.SocialCurtidas.AnyAsync(c => c.PostId == id && c.UserId == usuarioAtual.Id),
                EventoRelacionado = post.Evento
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
                Texto = texto.Trim(),
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Post), new { id });
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
                    NomeAutor = p.PerfilSocial != null ? p.PerfilSocial.NomeExibicao : (p.User != null ? p.User.UserName ?? "Usuário" : "Usuário"),
                    FotoPerfilUrl = p.PerfilSocial != null ? p.PerfilSocial.FotoPerfilUrl : FotoPerfilPadrao,
                    Legenda = p.Legenda,
                    ImagemUrl = string.IsNullOrWhiteSpace(p.ImagemUrl) ? ImagemPostPadrao : p.ImagemUrl,
                    Categoria = p.Categoria,
                    DataCriacao = p.CriadoEm,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count(c => c.Ativo),
                    EventoId = p.EventoId,
                    NomeEvento = p.Evento != null ? p.Evento.NomeEvento : null,
                    PerfilId = p.PerfilSocialId
                })
                .ToListAsync();

            ViewBag.Evento = evento;
            return View(posts);
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
