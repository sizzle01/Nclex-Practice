using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Admin.TestSets;

namespace Nclex.Api.Controllers;

/// <summary>Curated (prebuilt) test sets, published or not (api.md section 8, admin only).</summary>
[ApiController]
[Route("api/v1/admin/test-sets")]
[Authorize(Policy = Policies.Admin)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class AdminTestSetsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AdminTestSetPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<AdminTestSetPage> List(
        [FromQuery] ListAdminTestSetsRequest request,
        [FromServices] AdminTestSetsHandler handler,
        CancellationToken cancellationToken) =>
        handler.ListAsync(request, cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AdminTestSetView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AdminTestSetView> Get(
        Guid id,
        [FromServices] AdminTestSetsHandler handler,
        CancellationToken cancellationToken) =>
        handler.GetAsync(id, cancellationToken);

    /// <summary>Create a set; its questions must be published, active and on its track.</summary>
    [HttpPost]
    [ProducesResponseType<AdminTestSetView>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> Create(
        CreateTestSetRequest request,
        [FromServices] AdminTestSetsHandler handler,
        CancellationToken cancellationToken)
    {
        var created = await handler.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Change settings, the ordered question list or whether it is published.</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType<AdminTestSetView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AdminTestSetView> Update(
        Guid id,
        UpdateTestSetRequest request,
        [FromServices] AdminTestSetsHandler handler,
        CancellationToken cancellationToken) =>
        handler.UpdateAsync(id, request, cancellationToken);
}
