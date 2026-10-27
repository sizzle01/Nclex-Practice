using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Taxonomy.GetTaxonomy;

namespace Nclex.Api.Controllers;

/// <summary>The tags questions are organised by (api.md section 3, Catalog).</summary>
[ApiController]
[Route("api/v1/taxonomy")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class TaxonomyController : ControllerBase
{
    /// <summary>Client Needs tree, body systems, item types (with availability) and CJMM skills for a track.</summary>
    [HttpGet]
    [ProducesResponseType<TaxonomyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<TaxonomyResponse> Get(
        [FromQuery] GetTaxonomyRequest request,
        [FromServices] GetTaxonomyHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);
}
