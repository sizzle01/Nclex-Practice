using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Flags.Moderation;

namespace Nclex.Api.Controllers;

/// <summary>The moderation queue for content reports; reviewers and admins only (api.md section 8).</summary>
[ApiController]
[Route("api/v1/admin/flags")]
[Authorize(Policy = Policies.ContentModerator)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class AdminFlagsController : ControllerBase
{
    /// <summary>Reports with one status (open when left out), oldest first.</summary>
    [HttpGet]
    [ProducesResponseType<FlagQueuePage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<FlagQueuePage> List(
        [FromQuery] ListFlagQueueRequest request,
        [FromServices] ListFlagQueueHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>
    /// Assign, triage, resolve or reject a report. Resolving notifies the reporter with the note.
    /// 409 FLAG_CLOSED once the report is resolved or rejected.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType<FlagQueueItem>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<FlagQueueItem> Update(
        Guid id,
        UpdateFlagRequest request,
        [FromServices] UpdateFlagHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, request, cancellationToken);
}
