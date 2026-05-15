using EventX.Api.Auth;
using EventX.Api.DTOs.Events;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> Get(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var events = await _eventService.GetMyEventsAsync(userId, cancellationToken);
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EventDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var eventDto = await _eventService.GetMyEventByIdAsync(userId, id, cancellationToken);
        return eventDto is null ? NotFound() : Ok(eventDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EventDto>> Create(
        [FromBody] CreateEventRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _eventService.CreateEventAsync(userId, request, cancellationToken);
        if (created is null)
        {
            return BadRequest(new { message = "Invalid event payload." });
        }

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EventDto>> Update(
        int id,
        [FromBody] UpdateEventRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var existing = await _eventService.GetMyEventByIdAsync(userId, id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        var updated = await _eventService.UpdateEventAsync(userId, id, request, cancellationToken);
        if (updated is null)
        {
            return BadRequest(new { message = "Invalid event payload." });
        }

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var removed = await _eventService.DeleteEventAsync(userId, id, cancellationToken);
        return removed ? NoContent() : NotFound();
    }
}
