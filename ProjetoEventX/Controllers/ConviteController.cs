using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class ConviteController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConviteController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Criar(int eventoId)
        {
            var evento = await _context.Eventos.FindAsync(eventoId);
            if (evento == null)
            {
                return NotFound();
            }

            ViewBag.EventoId = eventoId;
            ViewBag.NomeEvento = evento.NomeEvento;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(int eventoId, ListaConvidado convite)
        {
            if (ModelState.IsValid)
            {
                var evento = await _context.Eventos.FindAsync(eventoId);
                if (evento == null)
                {
                    return NotFound();
                }

                // Obter dados do formulário
                var nomeConvidado = Request.Form["Convidado.Pessoa.Nome"].ToString();
                var emailConvidado = Request.Form["Convidado.Pessoa.Email"].ToString();

                // Verificar se o convidado já existe
                var pessoa = await _context.Pessoas
                    .FirstOrDefaultAsync(p => p.Email == emailConvidado);

                Convidado? convidado;
                if (pessoa != null)
                {
                    convidado = await _context.Convidados
                        .FirstOrDefaultAsync(c => c.PessoaId == pessoa.Id);
                    
                    if (convidado == null)
                    {
                        // Criar convidado para pessoa existente
                        var user = new ApplicationUser
                        {
                            UserName = pessoa.Email,
                            Email = pessoa.Email,
                            TipoUsuario = "Convidado"
                        };

                        var password = Guid.NewGuid().ToString().Substring(0, 8);
                        var result = await _userManager.CreateAsync(user, password);

                        if (result.Succeeded)
                        {
                            convidado = new Convidado
                            {
                                Id = user.Id,
                                UserName = user.UserName,
                                Email = user.Email,
                                PessoaId = pessoa.Id,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            _context.Convidados.Add(convidado);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    // Criar nova pessoa e convidado
                    pessoa = new Pessoa
                    {
                        Nome = nomeConvidado,
                        Email = emailConvidado,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Pessoas.Add(pessoa);
                    await _context.SaveChangesAsync();

                    var user = new ApplicationUser
                    {
                        UserName = pessoa.Email,
                        Email = pessoa.Email,
                        TipoUsuario = "Convidado"
                    };

                    var password = Guid.NewGuid().ToString().Substring(0, 8);
                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        convidado = new Convidado
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            PessoaId = pessoa.Id,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        _context.Convidados.Add(convidado);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Erro ao criar convidado");
                        ViewBag.EventoId = eventoId;
                        ViewBag.NomeEvento = evento.NomeEvento;
                        return View(new ListaConvidado());
                    }
                }

                if (convidado == null)
                {
                    ModelState.AddModelError("", "Erro ao criar ou encontrar convidado");
                    ViewBag.EventoId = eventoId;
                    ViewBag.NomeEvento = evento.NomeEvento;
                    return View(new ListaConvidado());
                }

                // Criar lista de convidado
                var listaConvidado = new ListaConvidado
                {
                    EventoId = eventoId,
                    ConvidadoId = convidado.Id,
                    DataInclusao = DateTime.Now,
                    ConfirmaPresenca = "Pendente",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.ListasConvidados.Add(listaConvidado);
                await _context.SaveChangesAsync();

                return RedirectToAction("DetalhesEvento", "Organizador", new { id = eventoId });
            }

            ViewBag.EventoId = eventoId;
            var eventoParaView = await _context.Eventos.FindAsync(eventoId);
            ViewBag.NomeEvento = eventoParaView?.NomeEvento ?? "Evento";
            return View(new ListaConvidado());
        }

        [HttpGet]
        public async Task<IActionResult> Listar(int eventoId)
        {
            var convidados = await _context.ListasConvidados
                .Include(l => l.Convidado)
                .ThenInclude(c => c.Pessoa)
                .Where(l => l.EventoId == eventoId)
                .ToListAsync();

            ViewBag.EventoId = eventoId;
            return View(convidados);
        }
    }
}

