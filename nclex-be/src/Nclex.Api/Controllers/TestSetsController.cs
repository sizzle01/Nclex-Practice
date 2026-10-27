using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Application.Catalog.GetTestSet;
using Nclex.Application.Catalog.ListTestSets;

namespace Nclex.Api.Controllers;

/// <summary>
/// Test sets (api.md section 3, Catalog). Premium sets are listed with <c>locked: true</c> rather
/// than hidden, so free users can see what an upgrade unlocks. No endpoint here returns items.
/// The custom builder (architecture.md 5.7) lives under <c>/custom</c>.
/// </summary>
[ApiController]
[Route("api/v1/test-sets")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class TestSetsController : ControllerBase
{
    /// <summary>Curated sets of a track (mine when left out): sample first, then practice, then challenge.</summary>
    [HttpGet]
    [ProducesResponseType<TestSetPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<TestSetPage> List(
        [FromQuery] ListTestSetsRequest request,
        [FromServices] ListTestSetsHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>One set's settings, for the detail panel.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TestSetDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<TestSetDetail> Get(
        Guid id,
        [FromServices] GetTestSetHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, cancellationToken);

    /// <summary>How many questions match the builder's filters right now.</summary>
    [HttpGet("custom/available-count")]
    [ProducesResponseType<AvailableCountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<AvailableCountResponse> AvailableCount(
        [FromQuery] AvailableCountRequest request,
        [FromServices] GetAvailableCountHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>Build a custom set from filters. 422 INSUFFICIENT_QUESTIONS (with <c>available</c>) when too few match.</summary>
    [HttpPost("custom")]
    [ProducesResponseType<CreateCustomSetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<IActionResult> CreateCustom(
        CreateCustomSetRequest request,
        [FromServices] CreateCustomSetHandler handler,
        CancellationToken cancellationToken)
    {
        var created = await handler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.TestSet.Id }, created);
    }
}
