using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Application.WeakAreas.GetWeakAreas;
using Nclex.Application.WeakAreas.StartWeakAreaPractice;

namespace Nclex.Api.Controllers;

/// <summary>
/// "Practice your weak areas" (architecture.md 5.7.1): the card's preview under <c>/me</c> and the
/// one-click test under <c>/test-sets</c>, next to the custom builder it shares its sets with.
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class WeakAreasController : ControllerBase
{
    /// <summary>My weak Client Needs areas in my last 10 tests, and how many questions a practice test would have.</summary>
    [HttpGet("me/weak-areas")]
    [ProducesResponseType<WeakAreasView>(StatusCodes.Status200OK)]
    public Task<WeakAreasView> Get(
        [FromQuery] GetWeakAreasRequest request,
        [FromServices] GetWeakAreasHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>
    /// Build a tutor-mode practice test from my weak areas and start it. 422 NO_WEAK_AREAS when none
    /// qualify; 422 INSUFFICIENT_QUESTIONS (with <c>available</c>) when fewer than 5 questions do.
    /// </summary>
    [HttpPost("test-sets/weak-areas")]
    [ProducesResponseType<CreateCustomSetResponse>(StatusCodes.Status201Created)]
    // EMAIL_NOT_VERIFIED: starting an attempt needs a verified email, as for any custom set.
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<IActionResult> Start(
        StartWeakAreaPracticeRequest request,
        [FromServices] StartWeakAreaPracticeHandler handler,
        CancellationToken cancellationToken)
    {
        var created = await handler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(TestSetsController.Get), "TestSets", new { id = created.TestSet.Id }, created);
    }
}
