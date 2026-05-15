using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Data;
using ProjetoEventX.DTOs.Events;
using ProjetoEventX.Models;
using ProjetoEventX.Security;
using ProjetoEventX.Services;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/events")]
    public class EventsApiController : ControllerBase
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditoriaService _auditoriaService;
        private readonly EventoSlugService _eventoSlugService;

        public EventsApiController(
            EventXContext context,
            UserManager<ApplicationUser> userManager,
            AuditoriaService auditoriaService,
            EventoSlugService eventoSlugService)
        {
            _context = context;
            _userManager = userManager;
            _auditoriaService = auditoriaService;
            _eventoSlugService = eventoSlugService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventListDto>>> GetAll()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.TipoUsuario != "Organizador")
            {
                return Forbid();
            }

            var eventos = await _context.Eventos
                .AsNoTracking()
                .Include(e => e.Local)
                .Where(e => e.OrganizadorId == user.Id)
                .OrderByDescending(e => e.DataEvento)
                .ToListAsync();

            var response = eventos.Select(MapToListDto).ToList();
            await _auditoriaService.RegistrarVisualizacaoAsync("Evento", 0, $"API - listagem de eventos do usuário {user.UserName}");

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventDetailsDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID do evento inválido." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!await User.IsOwnerOfEventoAsync(_userManager, id, _context))
            {
                return Forbid();
            }

            var evento = await _context.Eventos
                .AsNoTracking()
                .Include(e => e.Local)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
            {
                return NotFound(new { message = "Evento não encontrado." });
            }

            await _auditoriaService.RegistrarVisualizacaoAsync("Evento", id, $"API - detalhes do evento {evento.NomeEvento}");
            return Ok(MapToDetailsDto(evento));
        }

        [HttpPost]
        public async Task<ActionResult<EventDetailsDto>> Create([FromBody] CreateEventDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.TipoUsuario != "Organizador")
            {
                return Forbid();
            }

            if (!ValidateEventInput(request.NomeEvento, request.DescricaoEvento, request.TipoEvento, out var validationMessage))
            {
                return BadRequest(new { message = validationMessage });
            }

            if (request.DataEvento < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "A data do evento não pode ser anterior à data atual." });
            }

            var organizador = await _context.Organizadores
                .FirstOrDefaultAsync(o => o.Id == user.Id || o.Email == user.Email);

            if (organizador == null)
            {
                return BadRequest(new { message = "Organizador não encontrado." });
            }

            var evento = new Evento
            {
                NomeEvento = SecurityValidator.SanitizeInput(request.NomeEvento),
                DataEvento = request.DataEvento,
                DescricaoEvento = SecurityValidator.SanitizeHtml(request.DescricaoEvento),
                TipoEvento = SecurityValidator.SanitizeInput(request.TipoEvento),
                CustoEstimado = request.CustoEstimado,
                StatusEvento = string.IsNullOrWhiteSpace(request.StatusEvento)
                    ? "Planejado"
                    : SecurityValidator.SanitizeInput(request.StatusEvento),
                HoraInicio = SecurityValidator.SanitizeInput(request.HoraInicio),
                HoraFim = SecurityValidator.SanitizeInput(request.HoraFim),
                PublicoEstimado = request.PublicoEstimado,
                OrganizadorId = organizador.Id,
                LocalId = request.LocalId,
                ImagemCapa = string.IsNullOrWhiteSpace(request.ImagemCapa) ? null : SecurityValidator.SanitizeInput(request.ImagemCapa),
                PermitirComentarios = request.PermitirComentarios,
                PermitirEnvioFotos = request.PermitirEnvioFotos,
                PermitirCurtidas = request.PermitirCurtidas,
                AprovarFotosAntesPublicar = request.AprovarFotosAntesPublicar,
                PermitirVisualizacaoMural = request.PermitirVisualizacaoMural,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            evento.Slug = await _eventoSlugService.GerarSlugUnicoAsync(evento.NomeEvento);

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            await _auditoriaService.RegistrarAcaoAsync(
                "Evento",
                evento.Id,
                "CREATE",
                $"API - evento criado: {evento.NomeEvento}",
                null,
                new { evento.Id, evento.NomeEvento, evento.DataEvento });

            var eventoCriado = await _context.Eventos
                .AsNoTracking()
                .Include(e => e.Local)
                .FirstAsync(e => e.Id == evento.Id);

            var response = MapToDetailsDto(eventoCriado);
            return CreatedAtAction(nameof(GetById), new { id = evento.Id }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EventDetailsDto>> Update(int id, [FromBody] UpdateEventDto request)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID do evento inválido." });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!await User.IsOwnerOfEventoAsync(_userManager, id, _context))
            {
                return Forbid();
            }

            if (!ValidateEventInput(request.NomeEvento, request.DescricaoEvento, request.TipoEvento, out var validationMessage))
            {
                return BadRequest(new { message = validationMessage });
            }

            if (request.DataEvento < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "A data do evento não pode ser anterior à data atual." });
            }

            var evento = await _context.Eventos
                .Include(e => e.Local)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
            {
                return NotFound(new { message = "Evento não encontrado." });
            }

            evento.NomeEvento = SecurityValidator.SanitizeInput(request.NomeEvento);
            evento.DescricaoEvento = SecurityValidator.SanitizeHtml(request.DescricaoEvento);
            evento.TipoEvento = SecurityValidator.SanitizeInput(request.TipoEvento);
            evento.DataEvento = request.DataEvento;
            evento.StatusEvento = string.IsNullOrWhiteSpace(request.StatusEvento)
                ? evento.StatusEvento
                : SecurityValidator.SanitizeInput(request.StatusEvento);
            evento.HoraInicio = SecurityValidator.SanitizeInput(request.HoraInicio);
            evento.HoraFim = SecurityValidator.SanitizeInput(request.HoraFim);
            evento.PublicoEstimado = request.PublicoEstimado;
            evento.CustoEstimado = request.CustoEstimado;
            evento.LocalId = request.LocalId;
            evento.ImagemCapa = string.IsNullOrWhiteSpace(request.ImagemCapa) ? null : SecurityValidator.SanitizeInput(request.ImagemCapa);
            evento.PermitirComentarios = request.PermitirComentarios;
            evento.PermitirEnvioFotos = request.PermitirEnvioFotos;
            evento.PermitirCurtidas = request.PermitirCurtidas;
            evento.AprovarFotosAntesPublicar = request.AprovarFotosAntesPublicar;
            evento.PermitirVisualizacaoMural = request.PermitirVisualizacaoMural;
            evento.UpdatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(evento.Slug))
            {
                evento.Slug = await _eventoSlugService.GerarSlugUnicoAsync(evento.NomeEvento, evento.Id);
            }

            await _context.SaveChangesAsync();

            await _auditoriaService.RegistrarAcaoAsync(
                "Evento",
                evento.Id,
                "UPDATE",
                $"API - evento atualizado: {evento.NomeEvento}");

            return Ok(MapToDetailsDto(evento));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID do evento inválido." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!await User.IsOwnerOfEventoAsync(_userManager, id, _context))
            {
                return Forbid();
            }

            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null)
            {
                return NotFound(new { message = "Evento não encontrado." });
            }

            await _auditoriaService.RegistrarAcaoAsync(
                "Evento",
                evento.Id,
                "DELETE",
                $"API - evento excluído: {evento.NomeEvento}",
                evento,
                null);

            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static bool ValidateEventInput(string nomeEvento, string descricaoEvento, string tipoEvento, out string mensagem)
        {
            if (!SecurityValidator.IsValidInput(nomeEvento))
            {
                mensagem = "Nome do evento contém caracteres inválidos.";
                return false;
            }

            if (!SecurityValidator.IsValidInput(descricaoEvento, allowHtml: true))
            {
                mensagem = "Descrição do evento contém conteúdo suspeito.";
                return false;
            }

            if (!SecurityValidator.IsValidInput(tipoEvento))
            {
                mensagem = "Tipo do evento contém caracteres inválidos.";
                return false;
            }

            mensagem = string.Empty;
            return true;
        }

        private static EventListDto MapToListDto(Evento evento)
        {
            return new EventListDto
            {
                Id = evento.Id,
                NomeEvento = evento.NomeEvento,
                DataEvento = evento.DataEvento,
                TipoEvento = evento.TipoEvento,
                StatusEvento = evento.StatusEvento,
                PublicoEstimado = evento.PublicoEstimado,
                CustoEstimado = evento.CustoEstimado,
                LocalId = evento.LocalId,
                LocalNome = evento.Local?.NomeLocal,
                Slug = evento.Slug,
                ImagemCapa = evento.ImagemCapa
            };
        }

        private static EventDetailsDto MapToDetailsDto(Evento evento)
        {
            return new EventDetailsDto
            {
                Id = evento.Id,
                NomeEvento = evento.NomeEvento,
                DataEvento = evento.DataEvento,
                DescricaoEvento = evento.DescricaoEvento,
                TipoEvento = evento.TipoEvento,
                CustoEstimado = evento.CustoEstimado,
                StatusEvento = evento.StatusEvento,
                HoraInicio = evento.HoraInicio,
                HoraFim = evento.HoraFim,
                PublicoEstimado = evento.PublicoEstimado,
                OrganizadorId = evento.OrganizadorId,
                LocalId = evento.LocalId,
                LocalNome = evento.Local?.NomeLocal,
                LocalEndereco = evento.Local?.EnderecoLocal,
                Slug = evento.Slug,
                ImagemCapa = evento.ImagemCapa,
                PermitirComentarios = evento.PermitirComentarios,
                PermitirEnvioFotos = evento.PermitirEnvioFotos,
                PermitirCurtidas = evento.PermitirCurtidas,
                AprovarFotosAntesPublicar = evento.AprovarFotosAntesPublicar,
                PermitirVisualizacaoMural = evento.PermitirVisualizacaoMural,
                CreatedAt = evento.CreatedAt,
                UpdatedAt = evento.UpdatedAt
            };
        }
    }
}
