using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
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
            var evento = await _context.Eventos.Include(e => e.Local).FirstOrDefaultAsync(e => e.Id == eventoId);
            if (evento == null)
            {
                return NotFound();
            }

            ViewBag.EventoId = eventoId;
            ViewBag.NomeEvento = evento.NomeEvento;
            ViewBag.EnderecoLocal = evento.Local?.EnderecoLocal ?? "Local não informado";

            var listaConvidado = new ListaConvidado
            {
                Evento = new Evento
                {
                    NomeEvento = evento.NomeEvento,
                    DescricaoEvento = evento.DescricaoEvento ?? "Descrição não informada",
                    TipoEvento = evento.TipoEvento ?? "Outro",
                    DataEvento = evento.DataEvento,
                    Local = evento.Local ?? new Local
                    {
                        NomeLocal = "Local não informado",
                        EnderecoLocal = "Endereço não informado",
                        Capacidade = 0,
                        TipoLocal = "Outro",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    HoraInicio = !string.IsNullOrEmpty(evento.HoraInicio) ? evento.HoraInicio : DateTime.Now.ToString("HH:mm"),
                    HoraFim = !string.IsNullOrEmpty(evento.HoraFim) ? evento.HoraFim : DateTime.Now.AddHours(1).ToString("HH:mm"),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                Convidado = new Convidado
                {
                    Pessoa = new Pessoa
                    {
                        Nome = "Nome temporário",
                        Email = "email@temporario.com",
                        Cpf = "00000000000",
                        Endereco = "Endereço não informado",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    UserName = "tempUser",
                    Email = "email@temporario.com",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                DataInclusao = DateTime.Now,
                ConfirmaPresenca = "Pendente",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return View(listaConvidado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(int eventoId, ListaConvidado convite)
        {
            if (ModelState.IsValid)
            {
                var evento = await _context.Eventos.Include(e => e.Local).FirstOrDefaultAsync(e => e.Id == eventoId);
                if (evento == null)
                {
                    return NotFound();
                }

                // Obter dados do formulário
                var nomeConvidado = Request.Form["Convidado.Pessoa.Nome"].ToString();
                var emailConvidado = Request.Form["Convidado.Pessoa.Email"].ToString();

                // Verificar se o convidado já existe
                var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Email == emailConvidado);

                Convidado? convidado = null;
                if (pessoa != null)
                {
                    convidado = await _context.Convidados.FirstOrDefaultAsync(c => c.PessoaId == pessoa.Id);

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
                                Pessoa = pessoa,
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
                        UpdatedAt = DateTime.Now,
                        Cpf = "00000000000",
                        Endereco = "Endereço não informado"
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
                            Pessoa = pessoa,
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

                        return View(new ListaConvidado
                        {
                            Evento = new Evento
                            {
                                NomeEvento = evento.NomeEvento,
                                DescricaoEvento = evento.DescricaoEvento ?? "Descrição não informada",
                                TipoEvento = evento.TipoEvento ?? "Outro",
                                DataEvento = evento.DataEvento,
                                Local = evento.Local ?? new Local
                                {
                                    NomeLocal = "Local não informado",
                                    EnderecoLocal = "Endereço não informado",
                                    Capacidade = 0,
                                    TipoLocal = "Outro",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                },
                                HoraInicio = evento.HoraInicio,
                                HoraFim = evento.HoraFim,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            },
                            Convidado = new Convidado
                            {
                                Pessoa = new Pessoa
                                {
                                    Nome = "Nome temporário",
                                    Email = "email@temporario.com",
                                    Cpf = "00000000000",
                                    Endereco = "Endereço não informado",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                },
                                UserName = "tempUser",
                                Email = "email@temporario.com",
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            }
                        });
                    }
                }

                if (convidado == null)
                {
                    ModelState.AddModelError("", "Erro ao criar ou encontrar convidado");
                    ViewBag.EventoId = eventoId;
                    ViewBag.NomeEvento = evento.NomeEvento;

                    return View(new ListaConvidado
                    {
                        Evento = new Evento
                        {
                            NomeEvento = evento.NomeEvento,
                            DescricaoEvento = evento.DescricaoEvento ?? "Descrição não informada",
                            TipoEvento = evento.TipoEvento ?? "Outro",
                            DataEvento = evento.DataEvento,
                            Local = evento.Local ?? new Local
                            {
                                NomeLocal = "Local não informado",
                                EnderecoLocal = "Endereço não informado",
                                Capacidade = 0,
                                TipoLocal = "Outro",
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            },
                            HoraInicio = evento.HoraInicio,
                            HoraFim = evento.HoraFim,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        },
                        Convidado = new Convidado
                        {
                            Pessoa = new Pessoa
                            {
                                Nome = "Nome temporário",
                                Email = "email@temporario.com",
                                Cpf = "00000000000",
                                Endereco = "Endereço não informado",
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            },
                            UserName = "tempUser",
                            Email = "email@temporario.com",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        }
                    });
                }

                var listaConvidado = new ListaConvidado
                {
                    EventoId = eventoId,
                    ConvidadoId = convidado.Id,
                    Evento = evento,
                    Convidado = convidado,
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
            var eventoParaView = await _context.Eventos.Include(e => e.Local).FirstOrDefaultAsync(e => e.Id == eventoId);
            ViewBag.NomeEvento = eventoParaView?.NomeEvento ?? "Evento";

            return View(new ListaConvidado
            {
                Evento = new Evento
                {
                    NomeEvento = eventoParaView?.NomeEvento ?? "Novo Evento",
                    DescricaoEvento = eventoParaView?.DescricaoEvento ?? "Descrição padrão",
                    TipoEvento = eventoParaView?.TipoEvento ?? "Outro",
                    DataEvento = eventoParaView?.DataEvento ?? DateTime.Today,
                    Local = eventoParaView?.Local ?? new Local
                    {
                        NomeLocal = "Local não informado",
                        EnderecoLocal = "Endereço não informado",
                        Capacidade = 0,
                        TipoLocal = "Outro",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    HoraInicio = !string.IsNullOrEmpty(eventoParaView?.HoraInicio) ? eventoParaView.HoraInicio : DateTime.Now.ToString("HH:mm"),
                    HoraFim = !string.IsNullOrEmpty(eventoParaView?.HoraFim) ? eventoParaView.HoraFim : DateTime.Now.AddHours(1).ToString("HH:mm"),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                Convidado = new Convidado
                {
                    Pessoa = new Pessoa
                    {
                        Nome = "Convidado temporário",
                        Email = "email@temporario.com",
                        Cpf = "00000000000",
                        Endereco = "Endereço não informado",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    UserName = "tempUser",
                    Email = "email@temporario.com",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            });
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
