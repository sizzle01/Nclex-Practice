using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Admin.Subtopics;

namespace Nclex.Api.Controllers;

/// <summary>
/// The subtopic list (api.md section 8). All content staff can read it, because authors tag
/// questions; reviewers and admins keep it, since it is what candidates are told to study.
/// </summary>
[ApiController]
[Route("api/v1/admin/subtopics")]
[Authorize(Policy = Policies.ContentStaff)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class AdminSubtopicsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<SubtopicList>(StatusCodes.Status200OK)]
    public Task<SubtopicList> List(
        [FromQuery] ListSubtopicsRequest request,
        [FromServices] AdminSubtopicsHandler handler,
        CancellationToken cancellationToken) =>
        handler.ListAsync(request, cancellationToken);

    [HttpPost]
    [Authorize(Policy = Policies.ContentReviewer)]
    [ProducesResponseType<SubtopicView>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Create(
        CreateSubtopicRequest request,
        [FromServices] AdminSubtopicsHandler handler,
        CancellationToken cancellationToken)
    {
        var created = await handler.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Policies.ContentReviewer)]
    [ProducesResponseType<SubtopicView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<SubtopicView> Update(
        Guid id,
        UpdateSubtopicRequest request,
        [FromServices] AdminSubtopicsHandler handler,
        CancellationToken cancellationToken) =>
        handler.UpdateAsync(id, request, cancellationToken);
}
