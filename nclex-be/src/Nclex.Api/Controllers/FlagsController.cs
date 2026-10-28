using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Flags.MyFlags;
using Nclex.Application.Flags.ReportIssue;

namespace Nclex.Api.Controllers;

/// <summary>Content feedback: a signed-in user reports a problem with a question (api.md section 7).</summary>
[ApiController]
[Route("api/v1")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class FlagsController : ControllerBase
{
    /// <summary>
    /// Report a problem with a question version. 409 FLAG_DUPLICATE if I already reported it for
    /// the same reason; 404 if the version, or the attempt sent with it, is not one I can see.
    /// </summary>
    [HttpPost("question-versions/{id:guid}/flags")]
    [ProducesResponseType<MyFlagView>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Report(
        Guid id,
        ReportIssueRequest request,
        [FromServices] ReportIssueHandler handler,
        CancellationToken cancellationToken)
    {
        var flag = await handler.HandleAsync(id, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, flag);
    }

    /// <summary>My reports and where each one stands, newest first.</summary>
    [HttpGet("me/flags")]
    [ProducesResponseType<MyFlagPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<MyFlagPage> Mine(
        [FromQuery] ListMyFlagsRequest request,
        [FromServices] ListMyFlagsHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);
}
