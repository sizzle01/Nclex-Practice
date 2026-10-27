using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Catalog.ListQuestionBanks;

namespace Nclex.Api.Controllers;

/// <summary>Question banks (api.md section 3, Catalog).</summary>
[ApiController]
[Route("api/v1/question-banks")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class QuestionBanksController : ControllerBase
{
    /// <summary>Published banks of a track (mine when left out), with item counts and lock state.</summary>
    [HttpGet]
    [ProducesResponseType<QuestionBankList>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<QuestionBankList> List(
        [FromQuery] ListQuestionBanksRequest request,
        [FromServices] ListQuestionBanksHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);
}
