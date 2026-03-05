using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using ProjetoEventX.Security;
using ProjetoEventX.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SecurityActionFilter))]
    public class ConviteController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditoriaService _auditoriaService;

        public ConviteController(EventXContext context, UserManager<ApplicationUser> userManager, AuditoriaService auditoriaService)
        {
            _context = context;
            _userManager = userManager;
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Criar(int eventoId)
        {
            if (eventoId <= 0)
            {
                TempData["ErrorMessage"] = "❌ ID do evento inválido.";
                return RedirectToAction("Index", "Eventos");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar se o usuário é dono do evento
            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
            {
                await _auditoriaService.RegistrarAcaoAsync("Evento", eventoId, "VIEW", 
                    $"Tentativa não autorizada de criar convite por: {user.UserName}", null, null, false, "Acesso negado");
                return RedirectToAction("AccessDenied", "Auth");
            }

            var evento = await _context.Eventos.Include(e => e.Local).FirstOrDefaultAsync(e => e.Id == eventoId);
            if (evento == null)
            {
                TempData["ErrorMessage"] = "❌ Evento não encontrado.";
                return RedirectToAction("Index", "Eventos");
            }

            ViewBag.EventoId = eventoId;
            ViewBag.NomeEvento = evento.NomeEvento;
            ViewBag.DescricaoEvento = evento.DescricaoEvento ?? "Descrição não informada";
            ViewBag.TipoEvento = evento.TipoEvento ?? "Outro";
            ViewBag.DataEvento = evento.DataEvento.ToString("dd/MM/yyyy");
            ViewBag.HoraInicio = evento.HoraInicio ?? "";
            ViewBag.HoraFim = evento.HoraFim ?? "";
            ViewBag.NomeLocal = evento.Local?.NomeLocal ?? "Local não informado";
            ViewBag.EnderecoLocal = evento.Local?.EnderecoLocal ?? "Local não informado";

            // Buscar templates do organizador para este evento
            var templates = await _context.TemplatesConvites
                .Where(t => t.OrganizadorId == user.Id && t.EventoId == eventoId && t.Ativo)
                .OrderByDescending(t => t.PadraoSistema)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
            ViewBag.Templates = templates;

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
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    HoraInicio = !string.IsNullOrEmpty(evento.HoraInicio) ? evento.HoraInicio : DateTime.UtcNow.ToString("HH:mm"),
                    HoraFim = !string.IsNullOrEmpty(evento.HoraFim) ? evento.HoraFim : DateTime.UtcNow.AddHours(1).ToString("HH:mm"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                Convidado = new Convidado
                {
                    Pessoa = new Pessoa
                    {
                        Nome = "Nome temporário",
                        Email = "email@temporario.com",
                        Cpf = "00000000000",
                        Endereco = "Endereço não informado",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    UserName = "tempUser",
                    Email = "email@temporario.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                DataInclusao = DateTime.UtcNow,
                ConfirmaPresenca = "Pendente",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return View(listaConvidado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(int eventoId, ListaConvidado convite)
        {
            // Limpar erros de validação de propriedades de navegação (preenchidas pelo controller, não pelo form)
            ModelState.Remove("Evento");
            ModelState.Remove("Evento.NomeEvento");
            ModelState.Remove("Evento.DescricaoEvento");
            ModelState.Remove("Evento.TipoEvento");
            ModelState.Remove("Evento.Local");
            ModelState.Remove("Evento.HoraInicio");
            ModelState.Remove("Evento.HoraFim");
            ModelState.Remove("Convidado");
            ModelState.Remove("Convidado.Pessoa");
            ModelState.Remove("Convidado.Pessoa.Nome");
            ModelState.Remove("Convidado.Pessoa.Email");
            ModelState.Remove("Convidado.Pessoa.Cpf");
            ModelState.Remove("Convidado.Pessoa.Endereco");
            ModelState.Remove("Convidado.UserName");
            ModelState.Remove("Convidado.Email");

            // Validar apenas os campos do form
            var nomeForm = Request.Form["Convidado.Pessoa.Nome"].ToString();
            var emailForm = Request.Form["Convidado.Pessoa.Email"].ToString();
            if (string.IsNullOrWhiteSpace(nomeForm) || string.IsNullOrWhiteSpace(emailForm))
            {
                TempData["ErrorMessage"] = "❌ Nome e email do convidado são obrigatórios.";
                ViewBag.EventoId = eventoId;
                return View(convite);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar permissão
            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                var evento = await _context.Eventos.Include(e => e.Local).FirstOrDefaultAsync(e => e.Id == eventoId);
                if (evento == null)
                {
                    TempData["ErrorMessage"] = "❌ Evento não encontrado.";
                    return RedirectToAction("Index", "Eventos");
                }

                // Helper local para repopular ViewBag ao retornar à view
                void PrepararViewBag()
                {
                    ViewBag.EventoId = eventoId;
                    ViewBag.NomeEvento = evento.NomeEvento;
                    ViewBag.EnderecoLocal = evento.Local?.EnderecoLocal ?? "Local não informado";
                }

                // Obter e validar dados do formulário
                var nomeConvidado = Request.Form["Convidado.Pessoa.Nome"].ToString();
                var emailConvidado = Request.Form["Convidado.Pessoa.Email"].ToString();

                // Validações de segurança
                if (!SecurityValidator.IsValidInput(nomeConvidado))
                {
                    ModelState.AddModelError("", "❌ Nome do convidado contém caracteres inválidos.");
                    PrepararViewBag();
                    return View(convite);
                }

                if (!SecurityValidator.IsValidEmail(emailConvidado))
                {
                    ModelState.AddModelError("", "❌ Email do convidado inválido ou suspeito.");
                    PrepararViewBag();
                    return View(convite);
                }

                // Sanitizar dados
                nomeConvidado = SecurityValidator.SanitizeInput(nomeConvidado);
                emailConvidado = SecurityValidator.SanitizeInput(emailConvidado);

                // Verificar se o convidado já existe
                var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Email == emailConvidado);
                Convidado convidado = null;

                if (pessoa != null)
                {
                    convidado = await _context.Convidados.FirstOrDefaultAsync(c => c.PessoaId == pessoa.Id);

                    if (convidado == null)
                    {
                        // Criar convidado para pessoa existente
                        var userConvidado = new ApplicationUser
                        {
                            UserName = emailConvidado,
                            Email = emailConvidado,
                            TipoUsuario = "Convidado",
                            EmailConfirmed = true
                        };

                        var password = Guid.NewGuid().ToString().Substring(0, 8) + "@Aa1";
                        var result = await _userManager.CreateAsync(userConvidado, password);

                        if (result.Succeeded)
                        {
                            convidado = new Convidado
                            {
                                Id = userConvidado.Id,
                                UserName = userConvidado.UserName,
                                Email = userConvidado.Email,
                                PessoaId = pessoa.Id,
                                Pessoa = pessoa,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.Convidados.Add(convidado);
                            await _context.SaveChangesAsync();

                            // Registrar criação de convidado
                            await _auditoriaService.RegistrarAcaoAsync("Convidado", convidado.Id, "CREATE", 
                                $"Convidado criado: {pessoa.Nome}", null, new { convidado.Id, pessoa.Nome, pessoa.Email });
                        }
                        else
                        {
                            ModelState.AddModelError("", "❌ Erro ao criar usuário convidado.");
                            PrepararViewBag();
                            return View(convite);
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
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Cpf = "00000000000",
                        Endereco = "Endereço não informado"
                    };
                    _context.Pessoas.Add(pessoa);
                    await _context.SaveChangesAsync();

                    var userConvidado = new ApplicationUser
                    {
                        UserName = emailConvidado,
                        Email = emailConvidado,
                        TipoUsuario = "Convidado",
                        EmailConfirmed = true
                    };

                    var password = Guid.NewGuid().ToString().Substring(0, 8) + "@Aa1";
                    var result = await _userManager.CreateAsync(userConvidado, password);

                    if (result.Succeeded)
                    {
                        convidado = new Convidado
                        {
                            Id = userConvidado.Id,
                            UserName = userConvidado.UserName,
                            Email = userConvidado.Email,
                            PessoaId = pessoa.Id,
                            Pessoa = pessoa,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Convidados.Add(convidado);
                        await _context.SaveChangesAsync();

                        await _auditoriaService.RegistrarAcaoAsync("Convidado", convidado.Id, "CREATE", 
                            $"Convidado criado: {pessoa.Nome}", null, new { convidado.Id, pessoa.Nome, pessoa.Email });
                    }
                    else
                    {
                        ModelState.AddModelError("", "❌ Erro ao criar usuário convidado.");
                        PrepararViewBag();
                        return View(convite);
                    }
                }

                // Criar lista de convidado
                var listaConvidado = new ListaConvidado
                {
                    ConvidadoId = convidado.Id,
                    Convidado = convidado,
                    EventoId = eventoId,
                    Evento = evento!,
                    DataInclusao = DateTime.UtcNow,
                    ConfirmaPresenca = "Pendente",
                    CodigoQR = $"EVTX-{eventoId}-{convidado.Id}-{Guid.NewGuid().ToString("N")[..8]}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Verificar se já não está convidado
                var jaConvidado = await _context.ListasConvidados
                    .AnyAsync(l => l.ConvidadoId == convidado.Id && l.EventoId == eventoId);

                if (jaConvidado)
                {
                    TempData["InfoMessage"] = $"ℹ️ {pessoa.Nome} já está na lista de convidados deste evento.";
                    return RedirectToAction("Detalhes", "Eventos", new { id = eventoId });
                }

                _context.ListasConvidados.Add(listaConvidado);
                await _context.SaveChangesAsync();

                // Registrar convite
                await _auditoriaService.RegistrarAcaoAsync("ListaConvidado", listaConvidado.Id, "CREATE", 
                    $"Convidado adicionado ao evento: {pessoa.Nome} -> {evento.NomeEvento}", null, 
                    new { listaConvidado.Id, Convidado = pessoa.Nome, Evento = evento.NomeEvento });

                TempData["SuccessMessage"] = $"✅ Convidado '{pessoa.Nome}' adicionado com sucesso!";
                return RedirectToAction("Detalhes", "Eventos", new { id = eventoId });
            }
            catch (Exception ex)
            {
                await _auditoriaService.RegistrarAcaoAsync("Convidado", 0, "CREATE", 
                    $"Erro ao adicionar convidado: {ex.Message}", null, null, false, ex.Message);

                TempData["ErrorMessage"] = "❌ Erro ao adicionar convidado.";
                ViewBag.EventoId = eventoId;
                return View(convite);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Listar(int eventoId)
        {
            if (eventoId <= 0)
            {
                TempData["ErrorMessage"] = "❌ ID do evento inválido.";
                return RedirectToAction("Index", "Eventos");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar permissão
            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var convidados = await _context.ListasConvidados
                .Include(l => l.Convidado)
                .ThenInclude(c => c.Pessoa)
                .Where(l => l.EventoId == eventoId)
                .ToListAsync();

            ViewBag.EventoId = eventoId;

            // Registrar visualização
            await _auditoriaService.RegistrarVisualizacaoAsync("ListaConvidado", eventoId, 
                $"Listagem de convidados do evento ID: {eventoId}");

            return View(convidados);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarConvite(int eventoId, int convidadoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar permissão
            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                var convidado = await _context.Convidados
                    .Include(c => c.Pessoa)
                    .FirstOrDefaultAsync(c => c.Id == convidadoId);

                var evento = await _context.Eventos
                    .Include(e => e.Local)
                    .FirstOrDefaultAsync(e => e.Id == eventoId);

                if (convidado == null || evento == null)
                {
                    TempData["ErrorMessage"] = "❌ Convidado ou evento não encontrado.";
                    return RedirectToAction("Listar", new { eventoId });
                }

                var linkConfirmacao = Url.Action("ConfirmarPresenca", "Convite", 
                    new { eventoId = eventoId, convidadoId = convidadoId }, 
                    protocol: Request.Scheme) ?? "";

                var templateConvite = await _context.TemplatesConvites
                    .FirstOrDefaultAsync(t => t.OrganizadorId == user.Id && t.Ativo);

                var htmlConvite = templateConvite != null 
                    ? templateConvite.GerarHTMLConvite(convidado.Pessoa.Nome, linkConfirmacao)
                    : $"<h1>Convite para {evento.NomeEvento}</h1><p>Olá {convidado.Pessoa.Nome}, você está convidado!</p>";

                var htmlCompleto = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        {htmlConvite}
                        <div style='text-align: center; margin-top: 30px;'>
                            <a href='{linkConfirmacao}' style='background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                CONFIRMAR PRESENÇA
                            </a>
                        </div>
                        <p style='margin-top: 20px; font-size: 12px; color: #666; text-align: center;'>
                            Se você não pode visualizar este email corretamente, copie e cole este link no seu navegador: {linkConfirmacao}
                        </p>
                    </div>";

                // Registrar envio do convite
                await _auditoriaService.RegistrarAcaoAsync("Convite", 0, "SEND", 
                    $"Convite enviado para {convidado.Pessoa.Nome} no evento {evento.NomeEvento}", null, 
                    new { Convidado = convidado.Pessoa.Nome, Evento = evento.NomeEvento, TemplateUsado = templateConvite != null });

                TempData["SuccessMessage"] = $"✅ Convite enviado com sucesso para {convidado.Pessoa.Nome}!";
                return RedirectToAction("Listar", new { eventoId });
            }
            catch (Exception ex)
            {
                await _auditoriaService.RegistrarAcaoAsync("Convite", 0, "SEND", 
                    $"Erro ao enviar convite: {ex.Message}", null, null, false, ex.Message);

                TempData["ErrorMessage"] = "❌ Erro ao enviar convite.";
                return RedirectToAction("Listar", new { eventoId });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarPresenca(int eventoId, int convidadoId)
        {
            try
            {
                var listaConvidado = await _context.ListasConvidados
                    .Include(l => l.Convidado)
                    .ThenInclude(c => c.Pessoa)
                    .Include(l => l.Evento)
                    .FirstOrDefaultAsync(l => l.EventoId == eventoId && l.ConvidadoId == convidadoId);

                if (listaConvidado == null)
                {
                    TempData["ErrorMessage"] = "❌ Convite não encontrado.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.NomeConvidado = listaConvidado.Convidado?.Pessoa?.Nome ?? "Convidado";
                ViewBag.NomeEvento = listaConvidado.Evento?.NomeEvento ?? "Evento";
                ViewBag.DataEvento = listaConvidado.Evento?.DataEvento.ToString("dd/MM/yyyy") ?? "";
                ViewBag.HoraInicio = listaConvidado.Evento?.HoraInicio ?? "";
                ViewBag.StatusAtual = listaConvidado.ConfirmaPresenca;
                ViewBag.EventoId = eventoId;
                ViewBag.ConvidadoId = convidadoId;

                return View();
            }
            catch (Exception ex)
            {
                await _auditoriaService.RegistrarAcaoAsync("ListaConvidado", 0, "VIEW", 
                    $"Erro ao exibir RSVP: {ex.Message}", null, null, false, ex.Message);

                TempData["ErrorMessage"] = "❌ Erro ao carregar convite.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResponderRSVP(int eventoId, int convidadoId, string resposta)
        {
            try
            {
                var listaConvidado = await _context.ListasConvidados
                    .Include(l => l.Convidado)
                    .ThenInclude(c => c.Pessoa)
                    .Include(l => l.Evento)
                    .FirstOrDefaultAsync(l => l.EventoId == eventoId && l.ConvidadoId == convidadoId);

                if (listaConvidado == null)
                {
                    TempData["ErrorMessage"] = "❌ Convite não encontrado.";
                    return RedirectToAction("Index", "Home");
                }

                var statusAnterior = listaConvidado.ConfirmaPresenca;
                var novoStatus = resposta == "Confirmado" ? "Confirmado" : "Recusado";

                listaConvidado.ConfirmaPresenca = novoStatus;
                listaConvidado.UpdatedAt = DateTime.UtcNow;

                _context.Update(listaConvidado);
                await _context.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync("ListaConvidado", listaConvidado.Id, "UPDATE",
                    $"RSVP: {listaConvidado.Convidado?.Pessoa?.Nome} respondeu '{novoStatus}' no evento {listaConvidado.Evento?.NomeEvento}",
                    new { listaConvidado.Id, ConfirmaPresenca = statusAnterior },
                    new { listaConvidado.Id, ConfirmaPresenca = novoStatus });

                ViewBag.NomeConvidado = listaConvidado.Convidado?.Pessoa?.Nome ?? "Convidado";
                ViewBag.NomeEvento = listaConvidado.Evento?.NomeEvento ?? "Evento";
                ViewBag.DataEvento = listaConvidado.Evento?.DataEvento.ToString("dd/MM/yyyy") ?? "";
                ViewBag.Resposta = novoStatus;

                return View("RespostaRSVP");
            }
            catch (Exception ex)
            {
                await _auditoriaService.RegistrarAcaoAsync("ListaConvidado", 0, "UPDATE",
                    $"Erro no RSVP: {ex.Message}", null, null, false, ex.Message);

                TempData["ErrorMessage"] = "❌ Erro ao registrar resposta.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}