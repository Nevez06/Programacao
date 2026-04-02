using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using ProjetoEventX.Security;
using ProjetoEventX.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SecurityActionFilter))]
    public class TemplateConviteController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditoriaService _auditoriaService;
        private readonly EmailService _emailService;
        private readonly NotificationService _notificationService;
        private readonly EventLogService _eventLogService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TemplateConviteController(
            EventXContext context,
            UserManager<ApplicationUser> userManager,
            AuditoriaService auditoriaService,
            EmailService emailService,
            NotificationService notificationService,
            EventLogService eventLogService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _auditoriaService = auditoriaService;
            _emailService = emailService;
            _notificationService = notificationService;
            _eventLogService = eventLogService;
            _webHostEnvironment = webHostEnvironment;
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

            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
            {
                await _auditoriaService.RegistrarAcaoAsync(
                    "Evento",
                    eventoId,
                    "VIEW",
                    $"Tentativa não autorizada de criar convite por: {user.UserName}",
                    null,
                    null,
                    false,
                    "Acesso negado");

                return RedirectToAction("AccessDenied", "Auth");
            }

            var evento = await _context.Eventos
                .Include(e => e.Local)
                .FirstOrDefaultAsync(e => e.Id == eventoId);

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

            var templates = await _context.TemplatesConvites
                .Where(t => t.EventoId == eventoId && t.Ativo)
                .OrderByDescending(t => t.Id)
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
                    HoraInicio = !string.IsNullOrEmpty(evento.HoraInicio)
                        ? evento.HoraInicio
                        : DateTime.UtcNow.ToString("HH:mm"),
                    HoraFim = !string.IsNullOrEmpty(evento.HoraFim)
                        ? evento.HoraFim
                        : DateTime.UtcNow.AddHours(1).ToString("HH:mm"),
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

            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            try
            {
                var evento = await _context.Eventos
                    .Include(e => e.Local)
                    .FirstOrDefaultAsync(e => e.Id == eventoId);

                if (evento == null)
                {
                    TempData["ErrorMessage"] = "❌ Evento não encontrado.";
                    return RedirectToAction("Index", "Eventos");
                }

                void PrepararViewBag()
                {
                    ViewBag.EventoId = eventoId;
                    ViewBag.NomeEvento = evento.NomeEvento;
                    ViewBag.EnderecoLocal = evento.Local?.EnderecoLocal ?? "Local não informado";
                }

                var nomeConvidado = Request.Form["Convidado.Pessoa.Nome"].ToString();
                var emailConvidado = Request.Form["Convidado.Pessoa.Email"].ToString();

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

                nomeConvidado = SecurityValidator.SanitizeInput(nomeConvidado);
                emailConvidado = SecurityValidator.SanitizeInput(emailConvidado);

                var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Email == emailConvidado);
                Convidado? convidado = null;

                if (pessoa != null)
                {
                    convidado = await _context.Convidados.FirstOrDefaultAsync(c => c.PessoaId == pessoa.Id);

                    if (convidado == null)
                    {
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

                            await _auditoriaService.RegistrarAcaoAsync(
                                "Convidado",
                                convidado.Id,
                                "CREATE",
                                $"Convidado criado: {pessoa.Nome}",
                                null,
                                new { convidado.Id, pessoa.Nome, pessoa.Email });
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

                        await _auditoriaService.RegistrarAcaoAsync(
                            "Convidado",
                            convidado.Id,
                            "CREATE",
                            $"Convidado criado: {pessoa.Nome}",
                            null,
                            new { convidado.Id, pessoa.Nome, pessoa.Email });
                    }
                    else
                    {
                        ModelState.AddModelError("", "❌ Erro ao criar usuário convidado.");
                        PrepararViewBag();
                        return View(convite);
                    }
                }

                var listaConvidado = new ListaConvidado
                {
                    ConvidadoId = convidado!.Id,
                    Convidado = convidado,
                    EventoId = eventoId,
                    Evento = evento,
                    DataInclusao = DateTime.UtcNow,
                    ConfirmaPresenca = "Pendente",
                    CodigoQR = $"EVTX-{eventoId}-{convidado.Id}-{Guid.NewGuid().ToString("N")[..8]}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var jaConvidado = await _context.ListasConvidados
                    .AnyAsync(l => l.ConvidadoId == convidado.Id && l.EventoId == eventoId);

                if (jaConvidado)
                {
                    TempData["InfoMessage"] = $"ℹ️ {pessoa.Nome} já está na lista de convidados deste evento.";
                    return RedirectToAction("Detalhes", "Eventos", new { id = eventoId });
                }

                _context.ListasConvidados.Add(listaConvidado);
                await _context.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    "ListaConvidado",
                    listaConvidado.Id,
                    "CREATE",
                    $"Convidado adicionado ao evento: {pessoa.Nome} -> {evento.NomeEvento}",
                    null,
                    new { listaConvidado.Id, Convidado = pessoa.Nome, Evento = evento.NomeEvento });

                TempData["SuccessMessage"] = $"✅ Convidado '{pessoa.Nome}' adicionado com sucesso!";

                await _eventLogService.LogAsync(
                    eventoId,
                    user.Id,
                    "ConviteEnviado",
                    $"Convidado '{pessoa.Nome}' adicionado à lista do evento.");

                return RedirectToAction("Detalhes", "Eventos", new { id = eventoId });
            }
            catch (Exception ex)
            {
                await _auditoriaService.RegistrarAcaoAsync(
                    "Convidado",
                    0,
                    "CREATE",
                    $"Erro ao adicionar convidado: {ex.Message}",
                    null,
                    null,
                    false,
                    ex.Message);

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

            await _auditoriaService.RegistrarVisualizacaoAsync(
                "ListaConvidado",
                eventoId,
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

                var linkConfirmacao = Url.Action(
                    "ConfirmarPresenca",
                    "Convite",
                    new { eventoId, convidadoId },
                    protocol: Request.Scheme) ?? "";

                var templateConvite = await _context.TemplatesConvites
                    .FirstOrDefaultAsync(t => t.EventoId == eventoId && t.Ativo);

                var htmlConvite = templateConvite != null
                    ? $"<h1>{templateConvite.Titulo ?? $"Convite para {evento.NomeEvento}"}</h1>" +
                      $"<p>{templateConvite.Saudacao ?? $"Olá {convidado.Pessoa!.Nome},"}</p>" +
                      $"<p>{templateConvite.Mensagem ?? "Você está convidado!"}</p>"
                    : $"<h1>Convite para {evento.NomeEvento}</h1><p>Olá {convidado.Pessoa!.Nome}, você está convidado!</p>";

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

                await _auditoriaService.RegistrarAcaoAsync(
                    "Convite",
                    0,
                    "SEND",
                    $"Convite enviado para {convidado.Pessoa!.Nome} no evento {evento.NomeEvento}",
                    null,
                    new
                    {
                        Convidado = convidado.Pessoa!.Nome,
                        Evento = evento.NomeEvento,
                        TemplateUsado = templateConvite != null
                    });

                var emailEnviado = await _emailService.EnviarEmailAsync(
                    convidado.Pessoa!.Email!,
                    $"Convite: {evento.NomeEvento}",
                    htmlCompleto);

                if (emailEnviado)
                {
                    TempData["SuccessMessage"] =
                        $"✅ Convite enviado com sucesso para {convidado.Pessoa!.Nome} ({convidado.Pessoa!.Email})!";
                }
                else
                {
                    TempData["SuccessMessage"] =
                        $"✅ Convite registrado para {convidado.Pessoa!.Nome}! (Email SMTP não configurado - configure SMTP_USER e SMTP_PASS)";
                }

                await _eventLogService.LogAsync(
                    eventoId,
                    user.Id,
                    "ConviteEnviado",
                    $"Convite enviado para {convidado.Pessoa!.Nome} ({convidado.Pessoa!.Email}).");

                return RedirectToAction("Listar", new { eventoId });
            }
            catch (Exception ex)
            {
                await _auditoriaService.RegistrarAcaoAsync(
                    "Convite",
                    0,
                    "SEND",
                    $"Erro ao enviar convite: {ex.Message}",
                    null,
                    null,
                    false,
                    ex.Message);

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
                await _auditoriaService.RegistrarAcaoAsync(
                    "ListaConvidado",
                    0,
                    "VIEW",
                    $"Erro ao exibir RSVP: {ex.Message}",
                    null,
                    null,
                    false,
                    ex.Message);

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
                string novoStatus;

                if (resposta == "Confirmado")
                    novoStatus = "Confirmado";
                else if (resposta == "NaoIra")
                    novoStatus = "Não irá";
                else
                    novoStatus = "Recusado";

                listaConvidado.ConfirmaPresenca = novoStatus;
                listaConvidado.UpdatedAt = DateTime.UtcNow;

                _context.Update(listaConvidado);
                await _context.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    "ListaConvidado",
                    listaConvidado.Id,
                    "UPDATE",
                    $"RSVP: {listaConvidado.Convidado?.Pessoa?.Nome} respondeu '{novoStatus}' no evento {listaConvidado.Evento?.NomeEvento}",
                    new { listaConvidado.Id, ConfirmaPresenca = statusAnterior },
                    new { listaConvidado.Id, ConfirmaPresenca = novoStatus });

                if (listaConvidado.Evento != null && novoStatus == "Confirmado")
                {
                    var nomeConvidado = listaConvidado.Convidado?.Pessoa?.Nome ?? "Um convidado";

                    await _notificationService.CreateAsync(
                        listaConvidado.Evento.OrganizadorId,
                        "Presença confirmada",
                        $"{nomeConvidado} confirmou presença no evento '{listaConvidado.Evento.NomeEvento}'.",
                        "ConfirmacaoPresenca",
                        $"/Convite/Gerenciar?eventoId={listaConvidado.EventoId}");

                    await _eventLogService.LogAsync(
                        listaConvidado.EventoId,
                        null,
                        "ConfirmacaoPresenca",
                        $"{nomeConvidado} confirmou presença no evento.");
                }

                ViewBag.NomeConvidado = listaConvidado.Convidado?.Pessoa?.Nome ?? "Convidado";
                ViewBag.NomeEvento = listaConvidado.Evento?.NomeEvento ?? "Evento";
                ViewBag.DataEvento = listaConvidado.Evento?.DataEvento.ToString("dd/MM/yyyy") ?? "";
                ViewBag.Resposta = novoStatus;

                return View("RespostaRSVP");
            }
            catch (Exception ex)
            {
                await _auditoriaService.RegistrarAcaoAsync(
                    "ListaConvidado",
                    0,
                    "UPDATE",
                    $"Erro no RSVP: {ex.Message}",
                    null,
                    null,
                    false,
                    ex.Message);

                TempData["ErrorMessage"] = "❌ Erro ao registrar resposta.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editor(int eventoId)
        {
            if (eventoId <= 0)
                return RedirectToAction("Index", "Eventos");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("LoginOrganizador", "Auth");

            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
                return RedirectToAction("AccessDenied", "Auth");

            var evento = await _context.Eventos
                .Include(e => e.Local)
                .FirstOrDefaultAsync(e => e.Id == eventoId);

            if (evento == null)
                return RedirectToAction("Index", "Eventos");

            ViewBag.EventoId = eventoId;
            ViewBag.NomeEvento = evento.NomeEvento;
            ViewBag.DataEvento = evento.DataEvento.ToString("dd/MM/yyyy");
            ViewBag.HoraInicio = evento.HoraInicio ?? "";
            ViewBag.HoraFim = evento.HoraFim ?? "";
            ViewBag.NomeLocal = evento.Local?.NomeLocal ?? "Local não informado";
            ViewBag.EnderecoLocal = evento.Local?.EnderecoLocal ?? "";
            ViewBag.TipoEvento = evento.TipoEvento ?? "Outro";
            ViewBag.DescricaoEvento = evento.DescricaoEvento ?? "";

            var template = await _context.TemplatesConvites
                .Where(t => t.EventoId == eventoId && t.Ativo)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                template = new TemplateConvite
                {
                    EventoId = eventoId,
                    Nome = "Convite personalizado",
                    Titulo = evento.NomeEvento,
                    Mensagem = evento.DescricaoEvento ?? "Seu evento começa em breve!",
                    LayoutJson = BuildDefaultLayoutJson(evento),
                    Estilo = "Canva",
                    Ativo = true,
                    CorFundo = "#ffffff",
                    CorTexto = "#0f172a",
                    CorPrimaria = "#992008",
                    Fonte = "'Inter', sans-serif",
                    Saudacao = "Olá!",
                    TextoBotao = "Confirmar Presença"
                };

                _context.TemplatesConvites.Add(template);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = template.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Index", "Eventos");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            var template = await _context.TemplatesConvites
                .Include(t => t.Evento)
                .FirstOrDefaultAsync(t => t.Id == id && t.Ativo);

            if (template == null || !template.EventoId.HasValue)
            {
                TempData["ErrorMessage"] = "❌ Template de convite não encontrado.";
                return RedirectToAction("Index", "Eventos");
            }

            if (!await User.IsOwnerOfEventoAsync(_userManager, template.EventoId.Value, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var layoutJson = template.LayoutJson;
            if (string.IsNullOrWhiteSpace(layoutJson))
            {
                layoutJson = BuildDefaultLayoutJson(template.Evento);
            }

            var model = new TemplateConviteEditorViewModel
            {
                TemplateId = template.Id,
                Nome = template.Nome,
                NomeEvento = template.Evento?.NomeEvento ?? "Evento",
                LayoutJson = layoutJson
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLayout([FromBody] SaveTemplateLayoutRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dados inválidos para salvar o layout." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Não autenticado." });
            }

            var template = await _context.TemplatesConvites.FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.Ativo);
            if (template == null || !template.EventoId.HasValue)
            {
                return Json(new { success = false, message = "Template de convite não encontrado." });
            }

            if (!await User.IsOwnerOfEventoAsync(_userManager, template.EventoId.Value, _context))
            {
                return Json(new { success = false, message = "Sem permissão para editar este template." });
            }

            if (!IsValidJson(request.LayoutJson))
            {
                return Json(new { success = false, message = "LayoutJson inválido." });
            }

            template.LayoutJson = request.LayoutJson;
            _context.Update(template);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile file, int templateId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Não autenticado." });
            }

            var template = await _context.TemplatesConvites.FirstOrDefaultAsync(t => t.Id == templateId && t.Ativo);
            if (template == null || !template.EventoId.HasValue)
            {
                return Json(new { success = false, message = "Template inválido para upload." });
            }

            if (!await User.IsOwnerOfEventoAsync(_userManager, template.EventoId.Value, _context))
            {
                return Json(new { success = false, message = "Sem permissão para upload neste template." });
            }

            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Nenhuma imagem enviada." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Formato inválido. Use JPG, PNG ou WEBP." });
            }

            var uploadsRoot = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "convites");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = Url.Content($"~/uploads/convites/{fileName}");
            return Json(new { success = true, url = imageUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarDesignCanvas(int eventoId, string canvasJson, string nomeTemplate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Não autenticado" });

            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
                return Json(new { success = false, message = "Sem permissão" });

            var template = new TemplateConvite
            {
                EventoId = eventoId,
                Nome = nomeTemplate ?? "Convite personalizado",
                Titulo = "Convite",
                Mensagem = "Convite criado no editor visual",
                LayoutJson = canvasJson,
                Estilo = "Canvas",
                Ativo = true,
                CorFundo = "#ffffff",
                CorTexto = "#333333",
                CorPrimaria = "#992008",
                Fonte = "'Inter', sans-serif",
                Saudacao = "Olá Nome do Convidado,",
                TextoBotao = "Confirmar Presença"
            };

            _context.TemplatesConvites.Add(template);
            await _context.SaveChangesAsync();

            return Json(new { success = true, templateId = template.Id });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Publico(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
                return NotFound();

            var lista = await _context.ListasConvidados
                .Include(l => l.Convidado).ThenInclude(c => c.Pessoa)
                .Include(l => l.Evento).ThenInclude(e => e.Local)
                .FirstOrDefaultAsync(l => l.CodigoQR == codigo);

            if (lista == null)
                return NotFound();

            ViewBag.NomeConvidado = lista.Convidado?.Pessoa?.Nome ?? "Convidado";
            ViewBag.NomeEvento = lista.Evento?.NomeEvento ?? "Evento";
            ViewBag.DataEvento = lista.Evento?.DataEvento.ToString("dd/MM/yyyy") ?? "";
            ViewBag.HoraInicio = lista.Evento?.HoraInicio ?? "";
            ViewBag.HoraFim = lista.Evento?.HoraFim ?? "";
            ViewBag.NomeLocal = lista.Evento?.Local?.NomeLocal ?? "";
            ViewBag.EnderecoLocal = lista.Evento?.Local?.EnderecoLocal ?? "";
            ViewBag.StatusAtual = lista.ConfirmaPresenca;
            ViewBag.CodigoQR = codigo;
            ViewBag.EventoId = lista.EventoId;
            ViewBag.ConvidadoId = lista.ConvidadoId;

            var template = await _context.TemplatesConvites
                .Where(t => t.EventoId == lista.EventoId && t.Ativo)
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            ViewBag.Template = template;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GaleriaTemplates(int eventoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("LoginOrganizador", "Auth");

            if (!await User.IsOwnerOfEventoAsync(_userManager, eventoId, _context))
                return RedirectToAction("AccessDenied", "Auth");

            var evento = await _context.Eventos
                .Include(e => e.Local)
                .FirstOrDefaultAsync(e => e.Id == eventoId);

            if (evento == null)
                return RedirectToAction("Index", "Eventos");

            ViewBag.EventoId = eventoId;
            ViewBag.NomeEvento = evento.NomeEvento;
            ViewBag.TipoEvento = evento.TipoEvento;

            var templatesSalvos = await  _context.TemplatesConvites
                .Where(t => t.EventoId == eventoId && t.Ativo)
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            ViewBag.TemplatesSalvos = templatesSalvos;

            return View();
        }

        private static bool IsValidJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildDefaultLayoutJson(Evento? evento)
        {
            var defaultLayout = new
            {
                background = "#ffffff",
                width = 760,
                height = 1080,
                elements = new List<object>
                {
                    new
                    {
                        id = "el-title",
                        type = "title",
                        text = evento?.NomeEvento ?? "Seu Evento",
                        x = 70,
                        y = 90,
                        width = 620,
                        height = 90,
                        zIndex = 1,
                        color = "#0f172a",
                        background = "transparent",
                        fontSize = 46,
                        fontFamily = "'Inter', sans-serif",
                        fontWeight = "800",
                        textAlign = "center",
                        imageUrl = ""
                    },
                    new
                    {
                        id = "el-text",
                        type = "text",
                        text = evento?.DescricaoEvento ?? "Você está convidado para um momento especial.",
                        x = 80,
                        y = 240,
                        width = 600,
                        height = 130,
                        zIndex = 2,
                        color = "#334155",
                        background = "transparent",
                        fontSize = 24,
                        fontFamily = "'Inter', sans-serif",
                        fontWeight = "500",
                        textAlign = "center",
                        imageUrl = ""
                    },
                    new
                    {
                        id = "el-button",
                        type = "button",
                        text = "Confirmar Presença",
                        x = 225,
                        y = 930,
                        width = 310,
                        height = 62,
                        zIndex = 3,
                        color = "#ffffff",
                        background = "#992008",
                        fontSize = 24,
                        fontFamily = "'Inter', sans-serif",
                        fontWeight = "700",
                        textAlign = "center",
                        imageUrl = ""
                    }
                }
            };

            return JsonSerializer.Serialize(defaultLayout);
        }
    }
}
