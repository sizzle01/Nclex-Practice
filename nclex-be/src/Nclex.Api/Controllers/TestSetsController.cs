using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.GetTestSet;
using Nclex.Application.Catalog.ListTestSets;

namespace Nclex.Api.Controllers;

/// <summary>
/// Test sets (api.md section 3, Catalog). Premium sets are listed with <c>locked: true</c> rather
/// than hidden, so free users can see what an upgrade unlocks. No endpoint here returns items.
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
}
