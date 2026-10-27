using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Attempts;
using Nclex.Application.Attempts.DiscardAttempt;
using Nclex.Application.Attempts.GetAttempt;
using Nclex.Application.Attempts.GetItem;
using Nclex.Application.Attempts.ListAttempts;
using Nclex.Application.Attempts.Navigate;
using Nclex.Application.Attempts.OpenAttempt;
using Nclex.Application.Attempts.SaveResponse;
using Nclex.Application.Attempts.StartAttempt;
using Nclex.Application.Attempts.UpdateItemMarks;

namespace Nclex.Api.Controllers;

/// <summary>
/// The attempt engine (api.md section 4). Writes carry the <c>X-Attempt-Client</c> header of the
/// device holding the lease; someone else's attempt is reported as 404.
/// </summary>
[ApiController]
[Route("api/v1/attempts")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
public sealed class AttemptsController : ControllerBase
{
    /// <summary>Starts an attempt of a set, or returns the user's existing draft of the same prebuilt set.</summary>
    [HttpPost]
    [ProducesResponseType<AttemptState>(StatusCodes.Status201Created)]
    [ProducesResponseType<AttemptState>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public async Task<ActionResult<AttemptState>> Start(
        StartAttemptRequest request,
        [FromServices] StartAttemptHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.Created
            ? CreatedAtAction(nameof(Get), new { id = result.Attempt.Id }, result.Attempt)
            : Ok(result.Attempt);
    }

    /// <summary>The user's attempts with the given statuses (drafts when left out), most recent first.</summary>
    [HttpGet]
    [ProducesResponseType<AttemptPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<AttemptPage> List(
        [FromQuery] ListAttemptsRequest request,
        [FromServices] ListAttemptsHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>The attempt's state, without item content.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AttemptState>(StatusCodes.Status200OK)]
    public Task<AttemptState> Get(
        Guid id,
        [FromServices] GetAttemptHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, cancellationToken);

    /// <summary>Claims the device lease for this player page, taking it over from any other device.</summary>
    [HttpPost("{id:guid}/open")]
    [ProducesResponseType<AttemptState>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<AttemptState> Open(
        Guid id,
        [FromHeader(Name = AttemptClient.HeaderName)] string? clientId,
        [FromServices] OpenAttemptHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, clientId, cancellationToken);

    /// <summary>One item to display, without its answer key or rationale.</summary>
    [HttpGet("{id:guid}/items/{position:int}")]
    [ProducesResponseType<CandidateItemView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked, "application/problem+json")]
    public Task<CandidateItemView> GetItem(
        Guid id,
        int position,
        [FromServices] GetItemHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, position, cancellationToken);

    /// <summary>Autosaves the answer to one item. A stale <c>clientSeq</c> changes nothing and still returns 200.</summary>
    [HttpPut("{id:guid}/items/{position:int}/response")]
    [ProducesResponseType<SaveResponseResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked, "application/problem+json")]
    public Task<SaveResponseResult> SaveResponse(
        Guid id,
        int position,
        [FromHeader(Name = AttemptClient.HeaderName)] string? clientId,
        SaveResponseRequest request,
        [FromServices] SaveResponseHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, position, clientId, request, cancellationToken);

    /// <summary>Changes mark-for-review or strike-through on one item.</summary>
    [HttpPatch("{id:guid}/items/{position:int}")]
    [ProducesResponseType<ItemMarks>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked, "application/problem+json")]
    public Task<ItemMarks> UpdateMarks(
        Guid id,
        int position,
        [FromHeader(Name = AttemptClient.HeaderName)] string? clientId,
        UpdateItemMarksRequest request,
        [FromServices] UpdateItemMarksHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, position, clientId, request, cancellationToken);

    /// <summary>Moves to another item; the server records the time spent on the one being left.</summary>
    [HttpPost("{id:guid}/navigate")]
    [ProducesResponseType<NavigateResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked, "application/problem+json")]
    public Task<NavigateResult> Navigate(
        Guid id,
        [FromHeader(Name = AttemptClient.HeaderName)] string? clientId,
        NavigateRequest request,
        [FromServices] NavigateHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, clientId, request, cancellationToken);

    /// <summary>Discards a draft. Needs no lease, so the dashboard can do it.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Discard(
        Guid id,
        [FromServices] DiscardAttemptHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(id, cancellationToken);
        return NoContent();
    }
}
