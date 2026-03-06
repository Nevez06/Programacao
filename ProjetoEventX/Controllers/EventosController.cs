using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using ProjetoEventX.Security;
using ProjetoEventX.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(SecurityActionFilter))]
    public class EventosController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditoriaService _auditoriaService;

        public EventosController(EventXContext context, UserManager<ApplicationUser> userManager, AuditoriaService auditoriaService)
        {
            _context = context;
            _userManager = userManager;
            _auditoriaService = auditoriaService;
        }

        // GET: Eventos
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _auditoriaService.RegistrarAcaoAsync("ApplicationUser", 0, "VIEW", "Usuário não encontrado - Redirecionado para login");
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Registrar visualização
            await _auditoriaService.RegistrarVisualizacaoAsync("Evento", 0, $"Listagem de eventos do usuário: {user.UserName}");

            // Buscar apenas eventos do organizador logado
            var eventos = await _context.Eventos
                .Where(e => e.OrganizadorId == user.Id)
                .OrderByDescending(e => e.DataEvento)
                .ToListAsync();

            return View(eventos);
        }

        // GET: Eventos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                TempData["ErrorMessage"] = "❌ ID do evento inválido.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar se o usuário é dono do evento
            if (!await User.IsOwnerOfEventoAsync(_userManager, id.Value, _context))
            {
                TempData["ErrorMessage"] = "❌ Você não tem permissão para visualizar este evento.";
                await _auditoriaService.RegistrarAcaoAsync("Evento", id.Value, "VIEW", 
                    $"Tentativa não autorizada de visualizar evento por: {user.UserName}", null, null, false, "Acesso negado");
                return RedirectToAction("AccessDenied", "Auth");
            }

            var evento = await _context.Eventos
                .Include(e => e.Organizador)
                .ThenInclude(o => o.Pessoa)
                .Include(e => e.Local)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (evento == null)
            {
                TempData["ErrorMessage"] = "❌ Evento não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Registrar visualização bem-sucedida
            await _auditoriaService.RegistrarVisualizacaoAsync("Evento", evento.Id, $"Visualização do evento: {evento.NomeEvento}");

            return View(evento);
        }

        // GET: Eventos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Eventos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomeEvento,DescricaoEvento,DataEvento,TipoEvento,StatusEvento,HoraInicio,HoraFim,PublicoEstimado,CustoEstimado,LocalId,ImagemCapa")] Evento evento)
        {
            try
            {
                // OrganizadorId é definido pelo controller, não pelo form
                ModelState.Remove("OrganizadorId");

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "❌ Dados inválidos. Verifique os campos.";
                    return View(evento);
                }

                // Validações de segurança
                if (!SecurityValidator.IsValidInput(evento.NomeEvento))
                {
                    ModelState.AddModelError("", "❌ Nome do evento contém caracteres inválidos.");
                    return View(evento);
                }

                if (!SecurityValidator.IsValidInput(evento.DescricaoEvento, true)) // Permitir HTML básico
                {
                    ModelState.AddModelError("", "❌ Descrição do evento contém conteúdo suspeito.");
                    return View(evento);
                }

                // Validar data do evento
                if (evento.DataEvento < DateTime.UtcNow.Date)
                {
                    ModelState.AddModelError("", "❌ A data do evento não pode ser anterior à data atual.");
                    return View(evento);
                }

                // Buscar o organizador logado
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "❌ Usuário não encontrado. Faça login novamente.";
                    return RedirectToAction("LoginOrganizador", "Auth");
                }

                // Buscar o organizador no banco de dados
                var organizador = await _context.Organizadores
                    .FirstOrDefaultAsync(o => o.Id == user.Id);

                if (organizador == null)
                {
                    TempData["ErrorMessage"] = "❌ Organizador não encontrado. Verifique seu cadastro.";
                    return RedirectToAction("Dashboard", "Organizador");
                }

                // Definir dados do evento
                evento.OrganizadorId = organizador.Id;
                evento.CreatedAt = DateTime.UtcNow;
                evento.UpdatedAt = DateTime.UtcNow;
                evento.StatusEvento = "Planejado";

                // Sanitizar dados
                evento.NomeEvento = SecurityValidator.SanitizeInput(evento.NomeEvento);
                evento.DescricaoEvento = SecurityValidator.SanitizeHtml(evento.DescricaoEvento);
                evento.TipoEvento = SecurityValidator.SanitizeInput(evento.TipoEvento);

                // Gerar slug único
                evento.Slug = await GerarSlugUnico(evento.NomeEvento);

                _context.Eventos.Add(evento);
                await _context.SaveChangesAsync();

                // Registrar criação
                await _auditoriaService.RegistrarAcaoAsync("Evento", evento.Id, "CREATE", 
                    $"Evento criado: {evento.NomeEvento}", null, new { evento.Id, evento.NomeEvento, evento.DataEvento });

                TempData["SuccessMessage"] = $"✅ Evento '{evento.NomeEvento}' criado com sucesso!";
                return RedirectToAction("Index", new { id = evento.Id });
            }
            catch (Exception ex)
            {
                // Registrar erro
                await _auditoriaService.RegistrarAcaoAsync("Evento", 0, "CREATE", 
                    $"Erro ao criar evento: {ex.Message}", null, null, false, ex.Message);

                TempData["ErrorMessage"] = "❌ Erro ao criar evento. Tente novamente.";
                return View(evento);
            }
        }

        // GET: Eventos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar permissão
            if (!await User.IsOwnerOfEventoAsync(_userManager, id.Value, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null)
            {
                return NotFound();
            }

            return View(evento);
        }

        // POST: Eventos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NomeEvento,DescricaoEvento,DataEvento,TipoEvento,StatusEvento,HoraInicio,HoraFim,PublicoEstimado,CustoEstimado,LocalId,OrganizadorId,CreatedAt,UpdatedAt,ImagemCapa")] Evento evento)
        {
            if (id != evento.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar permissão
            if (!await User.IsOwnerOfEventoAsync(_userManager, id, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            // Limpar validação de propriedades de navegação não vindas do form
            ModelState.Remove("Organizador");
            ModelState.Remove("Local");

            if (ModelState.IsValid)
            {
                try
                {
                    // Buscar evento existente no banco
                    var eventoExistente = await _context.Eventos.FindAsync(id);
                    if (eventoExistente == null)
                    {
                        return NotFound();
                    }

                    // Registrar dados antigos para auditoria (cópia antes da alteração)
                    var eventoAntigoNome = eventoExistente.NomeEvento;

                    // Atualizar apenas os campos editáveis (sanitizados)
                    eventoExistente.NomeEvento = SecurityValidator.SanitizeInput(evento.NomeEvento);
                    eventoExistente.DescricaoEvento = SecurityValidator.SanitizeHtml(evento.DescricaoEvento);
                    eventoExistente.TipoEvento = SecurityValidator.SanitizeInput(evento.TipoEvento);
                    eventoExistente.StatusEvento = evento.StatusEvento;
                    eventoExistente.DataEvento = evento.DataEvento;
                    eventoExistente.HoraInicio = evento.HoraInicio;
                    eventoExistente.HoraFim = evento.HoraFim;
                    eventoExistente.PublicoEstimado = evento.PublicoEstimado;
                    eventoExistente.CustoEstimado = evento.CustoEstimado;
                    eventoExistente.ImagemCapa = evento.ImagemCapa;
                    eventoExistente.UpdatedAt = DateTime.UtcNow;

                    // Gerar slug se não existir
                    if (string.IsNullOrWhiteSpace(eventoExistente.Slug))
                    {
                        eventoExistente.Slug = await GerarSlugUnico(eventoExistente.NomeEvento, eventoExistente.Id);
                    }

                    await _context.SaveChangesAsync();

                    // Registrar atualização
                    await _auditoriaService.RegistrarAcaoAsync("Evento", eventoExistente.Id, "UPDATE", 
                        $"Evento atualizado: {eventoExistente.NomeEvento}");

                    TempData["SuccessMessage"] = $"✅ Evento '{eventoExistente.NomeEvento}' atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventoExists(evento.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    await _auditoriaService.RegistrarAcaoAsync("Evento", id, "UPDATE", 
                        $"Erro ao atualizar evento: {ex.Message}", null, null, false, ex.Message);
                    
                    TempData["ErrorMessage"] = "❌ Erro ao atualizar evento.";
                }
            }
            return View(evento);
        }

        // GET: Eventos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar permissão
            if (!await User.IsOwnerOfEventoAsync(_userManager, id.Value, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var evento = await _context.Eventos
                .Include(e => e.Organizador)
                .ThenInclude(o => o.Pessoa)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (evento == null)
            {
                return NotFound();
            }

            return View(evento);
        }

        // POST: Eventos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            // Verificar permissão
            if (!await User.IsOwnerOfEventoAsync(_userManager, id, _context))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            var evento = await _context.Eventos.FindAsync(id);
            if (evento != null)
            {
                // Registrar dados antes da exclusão
                await _auditoriaService.RegistrarAcaoAsync("Evento", evento.Id, "DELETE", 
                    $"Evento excluído: {evento.NomeEvento}", evento, null);

                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Evento '{evento.NomeEvento}' excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventoExists(int id)
        {
            return _context.Eventos.Any(e => e.Id == id);
        }

        private async Task<string> GerarSlugUnico(string nome, int? eventoIdExcluir = null)
        {
            var slug = GerarSlug(nome);
            var slugBase = slug;
            var contador = 1;

            while (await _context.Eventos.AnyAsync(e => e.Slug == slug && (eventoIdExcluir == null || e.Id != eventoIdExcluir)))
            {
                slug = $"{slugBase}-{contador}";
                contador++;
            }

            return slug;
        }

        private static string GerarSlug(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            var semAcentos = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var slug = Regex.Replace(semAcentos, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');
            return slug.Length > 280 ? slug.Substring(0, 280) : slug;
        }
    }
}