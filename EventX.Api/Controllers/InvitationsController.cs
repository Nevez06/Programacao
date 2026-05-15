using EventX.Api.Auth;
using EventX.Api.DTOs.Invitations;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/invitations")]
public sealed class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;

    public InvitationsController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpGet("templates")]
    [ProducesResponseType(typeof(IReadOnlyList<InvitationTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<InvitationTemplateDto>>> GetTemplates(
        [FromQuery] int? eventId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var templates = await _invitationService.GetTemplatesAsync(userId, eventId, cancellationToken);
        return Ok(templates);
    }

    [HttpGet("drafts")]
    [ProducesResponseType(typeof(IReadOnlyList<InvitationDraftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<InvitationDraftDto>>> GetDrafts(
        [FromQuery] int? eventId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var drafts = await _invitationService.GetDraftsAsync(userId, eventId, cancellationToken);
        return Ok(drafts);
    }

    [HttpPost("drafts")]
    [ProducesResponseType(typeof(InvitationDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InvitationDraftDto>> SaveDraft(
        [FromBody] SaveInvitationDraftRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var draft = await _invitationService.SaveDraftAsync(userId, request, cancellationToken);
        if (draft is null)
        {
            return BadRequest(new { message = "Invalid invitation draft payload." });
        }

        return Ok(draft);
    }

    [HttpPut("drafts/{id:int}")]
    [ProducesResponseType(typeof(InvitationDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvitationDraftDto>> UpdateDraft(
        int id,
        [FromBody] SaveInvitationDraftRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var draft = await _invitationService.UpdateDraftAsync(userId, id, request, cancellationToken);
        if (draft is null)
        {
            return NotFound(new { message = "Invitation draft not found or invalid payload." });
        }

        return Ok(draft);
    }

    [HttpDelete("drafts/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDraft(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var deleted = await _invitationService.DeleteDraftAsync(userId, id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("create-from-template")]
    [ProducesResponseType(typeof(InvitationDraftDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InvitationDraftDto>> CreateFromTemplate(
        [FromBody] CreateInvitationFromTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var draft = await _invitationService.CreateFromTemplateAsync(userId, request, cancellationToken);
        if (draft is null)
        {
            return BadRequest(new { message = "Invalid invitation template payload." });
        }

        return Created($"api/invitations/drafts/{draft.Id}", draft);
    }
}
