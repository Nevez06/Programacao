using EventX.Api.Auth;
using EventX.Api.DTOs.Quotes;
using EventX.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventX.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/quotes")]
public sealed class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<QuoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<QuoteDto>>> Get(
        [FromQuery] QuoteQueryDto query,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var quotes = await _quoteService.GetQuotesAsync(userId, query, cancellationToken);
        return Ok(quotes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<QuoteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var quote = await _quoteService.GetQuoteByIdAsync(userId, id, cancellationToken);
        return quote is null ? NotFound() : Ok(quote);
    }

    [HttpPost]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<QuoteDto>> Create(
        [FromBody] CreateQuoteRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _quoteService.CreateQuoteAsync(userId, request, cancellationToken);
        if (created is null)
        {
            return BadRequest(new { message = "Invalid quote payload." });
        }

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<QuoteDto>> UpdateStatus(
        int id,
        [FromBody] UpdateQuoteStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var current = await _quoteService.GetQuoteByIdAsync(userId, id, cancellationToken);
        if (current is null)
        {
            return NotFound();
        }

        var updated = await _quoteService.UpdateStatusAsync(userId, id, request, cancellationToken);
        if (updated is null)
        {
            return BadRequest(new { message = "Invalid status payload." });
        }

        return Ok(updated);
    }

    [HttpPut("{id:int}/negotiate")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<QuoteDto>> Negotiate(
        int id,
        [FromBody] NegotiateQuoteRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var current = await _quoteService.GetQuoteByIdAsync(userId, id, cancellationToken);
        if (current is null)
        {
            return NotFound();
        }

        var updated = await _quoteService.NegotiateAsync(userId, id, request, cancellationToken);
        if (updated is null)
        {
            return BadRequest(new { message = "Invalid negotiation payload." });
        }

        return Ok(updated);
    }
}
