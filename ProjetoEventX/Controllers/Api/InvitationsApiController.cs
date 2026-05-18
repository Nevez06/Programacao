using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Invitations;
using ProjetoEventX.Models;
using ProjetoEventX.Security;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/invitations")]
    public class InvitationsApiController : ControllerBase
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvitationsApiController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("templates")]
        public async Task<ActionResult<IEnumerable<InvitationTemplateDto>>> GetTemplates([FromQuery] int? eventId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            IQueryable<TemplateConvite> query = _context.TemplatesConvites
                .AsNoTracking()
                .Where(t => t.Ativo);

            if (eventId.HasValue)
            {
                query = query.Where(t => t.EventoId == eventId.Value || t.PadraoSistema);
            }

            if (string.Equals(currentUser.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(t => t.PadraoSistema || t.OrganizadorId == currentUser.Id || t.OrganizadorId == 0);
            }

            var templates = await query
                .OrderByDescending(t => t.PadraoSistema)
                .ThenBy(t => t.Nome)
                .Take(200)
                .Select(t => new InvitationTemplateDto
                {
                    Id = t.Id,
                    Name = t.Nome,
                    Style = t.Estilo,
                    BackgroundColor = t.CorFundo,
                    PrimaryColor = t.CorPrimaria,
                    TextColor = t.CorTexto,
                    Font = t.Fonte,
                    Title = t.Titulo,
                    Message = t.Mensagem,
                    PreviewUrl = null,
                    DefaultSystem = t.PadraoSistema,
                    OrganizerId = t.OrganizadorId,
                    EventId = t.EventoId
                })
                .ToListAsync();

            return Ok(templates);
        }

        [HttpGet("drafts")]
        public async Task<ActionResult<IEnumerable<InvitationDraftDto>>> GetDrafts([FromQuery] int? eventId = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!string.Equals(currentUser.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var query = _context.ConvitesRascunhos
                .AsNoTracking()
                .Where(d => d.OrganizadorId == currentUser.Id);

            if (eventId.HasValue)
            {
                query = query.Where(d => d.EventoId == eventId.Value);
            }

            var drafts = await query
                .OrderByDescending(d => d.UpdatedAt)
                .Take(200)
                .Select(d => new InvitationDraftDto
                {
                    Id = d.Id,
                    EventId = d.EventoId,
                    OrganizerId = d.OrganizadorId,
                    TemplateId = d.TemplateId,
                    Name = d.NomeRascunho,
                    LayoutJson = d.LayoutJson,
                    PreviewHtml = d.PreviewHtml,
                    PreviewUrl = d.PreviewUrl,
                    UpdatedAt = d.UpdatedAt
                })
                .ToListAsync();

            return Ok(drafts);
        }

        [HttpPost("drafts")]
        public async Task<ActionResult<InvitationDraftDto>> SaveDraft([FromBody] SaveInvitationDraftDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!string.Equals(currentUser.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var eventExists = await _context.Eventos.AnyAsync(e => e.Id == request.EventId && e.OrganizadorId == currentUser.Id);
            if (!eventExists)
            {
                return BadRequest(new { message = "Evento inválido para este organizador." });
            }

            ConviteRascunho draft;
            if (request.Id.HasValue)
            {
                draft = await _context.ConvitesRascunhos
                    .FirstOrDefaultAsync(d => d.Id == request.Id.Value && d.OrganizadorId == currentUser.Id)
                    ?? new ConviteRascunho();

                if (draft.Id == 0)
                {
                    return NotFound(new { message = "Rascunho não encontrado." });
                }
            }
            else
            {
                draft = new ConviteRascunho
                {
                    OrganizadorId = currentUser.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ConvitesRascunhos.Add(draft);
            }

            draft.EventoId = request.EventId;
            draft.TemplateId = request.TemplateId;
            draft.NomeRascunho = string.IsNullOrWhiteSpace(request.Name)
                ? "Convite sem título"
                : SecurityValidator.SanitizeInput(request.Name);
            draft.LayoutJson = request.LayoutJson;
            draft.PreviewHtml = request.PreviewHtml;
            draft.PreviewUrl = request.PreviewUrl;
            draft.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new InvitationDraftDto
            {
                Id = draft.Id,
                EventId = draft.EventoId,
                OrganizerId = draft.OrganizadorId,
                TemplateId = draft.TemplateId,
                Name = draft.NomeRascunho,
                LayoutJson = draft.LayoutJson,
                PreviewHtml = draft.PreviewHtml,
                PreviewUrl = draft.PreviewUrl,
                UpdatedAt = draft.UpdatedAt
            });
        }

        [HttpPost("create-from-template")]
        public async Task<ActionResult<InvitationDraftDto>> CreateFromTemplate([FromBody] CreateInvitationFromTemplateDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (!string.Equals(currentUser.TipoUsuario, "Organizador", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var eventExists = await _context.Eventos.AnyAsync(e => e.Id == request.EventId && e.OrganizadorId == currentUser.Id);
            if (!eventExists)
            {
                return BadRequest(new { message = "Evento inválido para este organizador." });
            }

            var template = await _context.TemplatesConvites
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.Ativo);

            if (template == null)
            {
                return NotFound(new { message = "Template não encontrado." });
            }

            var draft = new ConviteRascunho
            {
                EventoId = request.EventId,
                OrganizadorId = currentUser.Id,
                TemplateId = template.Id,
                NomeRascunho = string.IsNullOrWhiteSpace(request.Name)
                    ? $"Rascunho - {template.Nome}"
                    : SecurityValidator.SanitizeInput(request.Name),
                LayoutJson = string.IsNullOrWhiteSpace(template.LayoutJson) ? "{}" : template.LayoutJson,
                PreviewHtml = null,
                PreviewUrl = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ConvitesRascunhos.Add(draft);
            await _context.SaveChangesAsync();

            return Ok(new InvitationDraftDto
            {
                Id = draft.Id,
                EventId = draft.EventoId,
                OrganizerId = draft.OrganizadorId,
                TemplateId = draft.TemplateId,
                Name = draft.NomeRascunho,
                LayoutJson = draft.LayoutJson,
                PreviewHtml = draft.PreviewHtml,
                PreviewUrl = draft.PreviewUrl,
                UpdatedAt = draft.UpdatedAt
            });
        }
    }
}
