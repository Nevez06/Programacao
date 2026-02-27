using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.Models;
using System.Security.Claims;

namespace ProjetoEventX.Controllers
{
    [Authorize]
    public class TemplateConviteController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TemplateConviteController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Listar templates do organizador
        public async Task<IActionResult> Index(int? eventoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Verificar se é organizador
            var organizador = await _context.Organizadores
                .FirstOrDefaultAsync(o => o.Id == userId);

            if (organizador == null)
            {
                return Forbid("Apenas organizadores podem gerenciar templates de convite.");
            }

            IQueryable<TemplateConvite> templatesQuery = _context.TemplatesConvites
                .Include(t => t.Evento)
                .Where(t => t.OrganizadorId == userId);

            if (eventoId.HasValue)
            {
                templatesQuery = templatesQuery.Where(t => t.EventoId == eventoId.Value);
            }

            var templates = await templatesQuery
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.EventoId = eventoId;
            return View(templates);
        }

        // Criar novo template
        public async Task<IActionResult> Create(int eventoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Verificar se é organizador do evento
            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == eventoId && e.OrganizadorId == userId);

            if (evento == null)
            {
                return Forbid("Você não tem permissão para criar templates para este evento.");
            }

            var template = new TemplateConvite
            {
                EventoId = eventoId,
                Evento = evento,
                OrganizadorId = userId,
                NomeTemplate = $"Template - {evento.NomeEvento}",
                TituloConvite = "Você está convidado!",
                MensagemPrincipal = "Você está cordialmente convidado para participar do nosso evento especial.",
                CorFundo = "#ffffff",
                CorTexto = "#333333",
                CorPrimaria = "#007bff",
                TamanhoFonteTitulo = 32,
                TamanhoFonteTexto = 16,
                MostrarLogo = true,
                MostrarFotoEvento = true,
                MostrarMapa = true,
                MostrarQRCode = false,
                EstiloLayout = "Moderno"
            };

            ViewBag.EventoNome = evento.NomeEvento;
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventoId,NomeTemplate,TituloConvite,MensagemPrincipal,MensagemSecundaria,CorFundo,CorTexto,CorPrimaria,FonteTitulo,FonteTexto,TamanhoFonteTitulo,TamanhoFonteTexto,MostrarLogo,MostrarFotoEvento,MostrarMapa,MostrarQRCode,ImagemCabecalho,ImagemRodape,EstiloLayout,CSSPersonalizado")] TemplateConvite template)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Verificar se é organizador do evento
            var evento = await _context.Eventos
                .FirstOrDefaultAsync(e => e.Id == template.EventoId && e.OrganizadorId == userId);

            if (evento == null)
            {
                return Forbid("Você não tem permissão para criar templates para este evento.");
            }

            if (ModelState.IsValid)
            {
                template.OrganizadorId = userId;
                template.CreatedAt = DateTime.Now;
                template.UpdatedAt = DateTime.Now;
                template.Ativo = true;

                _context.TemplatesConvites.Add(template);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Template criado com sucesso!";
                return RedirectToAction(nameof(Index), new { eventoId = template.EventoId });
            }

            ViewBag.EventoNome = evento.NomeEvento;
            return View(template);
        }

        // Editar template
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var template = await _context.TemplatesConvites
                .Include(t => t.Evento)
                .FirstOrDefaultAsync(t => t.Id == id && t.OrganizadorId == userId);

            if (template == null)
            {
                return NotFound();
            }

            ViewBag.EventoNome = template.Evento.NomeEvento;
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EventoId,NomeTemplate,TituloConvite,MensagemPrincipal,MensagemSecundaria,CorFundo,CorTexto,CorPrimaria,FonteTitulo,FonteTexto,TamanhoFonteTitulo,TamanhoFonteTexto,MostrarLogo,MostrarFotoEvento,MostrarMapa,MostrarQRCode,ImagemCabecalho,ImagemRodape,EstiloLayout,CSSPersonalizado")] TemplateConvite template)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (id != template.Id)
            {
                return NotFound();
            }

            // Verificar se é o dono do template
            var templateExistente = await _context.TemplatesConvites
                .FirstOrDefaultAsync(t => t.Id == id && t.OrganizadorId == userId);

            if (templateExistente == null)
            {
                return Forbid("Você não tem permissão para editar este template.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Atualizar apenas as propriedades permitidas
                    templateExistente.NomeTemplate = template.NomeTemplate;
                    templateExistente.TituloConvite = template.TituloConvite;
                    templateExistente.MensagemPrincipal = template.MensagemPrincipal;
                    templateExistente.MensagemSecundaria = template.MensagemSecundaria;
                    templateExistente.CorFundo = template.CorFundo;
                    templateExistente.CorTexto = template.CorTexto;
                    templateExistente.CorPrimaria = template.CorPrimaria;
                    templateExistente.FonteTitulo = template.FonteTitulo;
                    templateExistente.FonteTexto = template.FonteTexto;
                    templateExistente.TamanhoFonteTitulo = template.TamanhoFonteTitulo;
                    templateExistente.TamanhoFonteTexto = template.TamanhoFonteTexto;
                    templateExistente.MostrarLogo = template.MostrarLogo;
                    templateExistente.MostrarFotoEvento = template.MostrarFotoEvento;
                    templateExistente.MostrarMapa = template.MostrarMapa;
                    templateExistente.MostrarQRCode = template.MostrarQRCode;
                    templateExistente.ImagemCabecalho = template.ImagemCabecalho;
                    templateExistente.ImagemRodape = template.ImagemRodape;
                    templateExistente.EstiloLayout = template.EstiloLayout;
                    templateExistente.CSSPersonalizado = template.CSSPersonalizado;
                    templateExistente.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Template atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TemplateConviteExists(template.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { eventoId = templateExistente.EventoId });
            }

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == templateExistente.EventoId);
            ViewBag.EventoNome = evento?.NomeEvento ?? "Evento";
            return View(template);
        }

        // Preview do template
        public async Task<IActionResult> Preview(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var template = await _context.TemplatesConvites
                .Include(t => t.Evento)
                .ThenInclude(e => e.Local)
                .FirstOrDefaultAsync(t => t.Id == id && t.OrganizadorId == userId);

            if (template == null)
            {
                return NotFound();
            }

            // Gerar preview com dados de exemplo
            var htmlConvite = template.GerarHTMLConvite("Nome do Convidado", "https://seu-site.com/confirmar/12345");
            
            ViewBag.HTMLConvite = htmlConvite;
            return View(template);
        }

        // Definir como template padrão
        [HttpPost]
        public async Task<IActionResult> DefinirPadrao(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var template = await _context.TemplatesConvites
                .FirstOrDefaultAsync(t => t.Id == id && t.OrganizadorId == userId);

            if (template == null)
            {
                return NotFound();
            }

            // Remover flag de padrão de outros templates do mesmo evento
            var outrosTemplates = await _context.TemplatesConvites
                .Where(t => t.EventoId == template.EventoId && t.Id != id)
                .ToListAsync();

            foreach (var outroTemplate in outrosTemplates)
            {
                outroTemplate.PadraoSistema = false;
            }

            // Definir este como padrão
            template.PadraoSistema = true;
            template.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Template definido como padrão!";
            return RedirectToAction(nameof(Index), new { eventoId = template.EventoId });
        }

        // Desativar template
        [HttpPost]
        public async Task<IActionResult> Desativar(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var template = await _context.TemplatesConvites
                .FirstOrDefaultAsync(t => t.Id == id && t.OrganizadorId == userId);

            if (template == null)
            {
                return NotFound();
            }

            template.Ativo = false;
            template.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Template desativado com sucesso!";
            return RedirectToAction(nameof(Index), new { eventoId = template.EventoId });
        }

        // Obter template padrão para um evento
        public async Task<TemplateConvite?> ObterTemplatePadrao(int eventoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            return await _context.TemplatesConvites
                .Include(t => t.Evento)
                .FirstOrDefaultAsync(t => t.EventoId == eventoId && t.OrganizadorId == userId && t.PadraoSistema && t.Ativo);
        }

        private bool TemplateConviteExists(int id)
        {
            return _context.TemplatesConvites.Any(e => e.Id == id);
        }
    }
}