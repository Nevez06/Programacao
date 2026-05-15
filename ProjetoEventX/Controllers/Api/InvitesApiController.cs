using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoEventX.DTOs.Invites;
using ProjetoEventX.Services;

namespace ProjetoEventX.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/invites")]
    public class InvitesApiController : ControllerBase
    {
        private readonly InviteService _inviteService;

        public InvitesApiController(InviteService inviteService)
        {
            _inviteService = inviteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InviteListDto>>> GetInvites([FromQuery] int take = 200)
        {
            var user = await _inviteService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var invites = await _inviteService.GetInvitesAsync(user, take);
            return Ok(invites);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InviteDetailsDto>> GetById(int id)
        {
            var user = await _inviteService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var invite = await _inviteService.GetInviteDetailsAsync(user, id);
                if (invite == null)
                {
                    return NotFound(new { message = "Convite nao encontrado." });
                }

                return Ok(invite);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<ActionResult<InviteDetailsDto>> CreateInvite([FromBody] CreateInviteDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _inviteService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var created = await _inviteService.CreateInviteAsync(user, request);
                bool? emailSent = null;
                if (request.EnviarAgora)
                {
                    var link = BuildConfirmationLink(created.EventoId, created.ConvidadoId);
                    emailSent = await _inviteService.SendInviteAsync(user, created.Id, link, request.MensagemOpcional);
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.Id },
                    new
                    {
                        invite = created,
                        emailSent
                    });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<InviteDetailsDto>> UpdateInvite(int id, [FromBody] UpdateInviteDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _inviteService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var updated = await _inviteService.UpdateInviteAsync(user, id, request);
                if (updated == null)
                {
                    return NotFound(new { message = "Convite nao encontrado." });
                }

                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}/rsvp")]
        public async Task<ActionResult<InviteDetailsDto>> RespondRsvp(int id, [FromBody] RsvpResponseDto request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _inviteService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var updated = await _inviteService.RespondRsvpAsync(user, id, request);
                if (updated == null)
                {
                    return NotFound(new { message = "Convite nao encontrado." });
                }

                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:int}/send")]
        public async Task<IActionResult> SendInvite(int id, [FromBody] SendInviteRequest? request)
        {
            var user = await _inviteService.GetCurrentUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            try
            {
                var details = await _inviteService.GetInviteDetailsAsync(user, id);
                if (details == null)
                {
                    return NotFound(new { message = "Convite nao encontrado." });
                }

                var confirmationLink = BuildConfirmationLink(details.EventoId, details.ConvidadoId);
                var sent = await _inviteService.SendInviteAsync(user, id, confirmationLink, request?.MensagemOpcional);
                if (!sent.HasValue)
                {
                    return NotFound(new { message = "Convite nao encontrado." });
                }

                return Ok(new
                {
                    sent = sent.Value,
                    message = sent.Value
                        ? "Convite enviado com sucesso."
                        : "Convite processado, mas o envio de email nao foi confirmado."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string? BuildConfirmationLink(int eventoId, int convidadoId)
        {
            return Url.Action(
                "ConfirmarPresenca",
                "Convite",
                new { eventoId, convidadoId },
                protocol: Request.Scheme);
        }

        public class SendInviteRequest
        {
            public string? MensagemOpcional { get; set; }
        }
    }
}
