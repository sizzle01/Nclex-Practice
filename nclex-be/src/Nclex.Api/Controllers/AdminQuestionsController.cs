using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Admin.Questions;
using Nclex.Application.Admin.Questions.CreateQuestion;
using Nclex.Application.Admin.Questions.CreateVersion;
using Nclex.Application.Admin.Questions.Retire;
using Nclex.Application.Admin.Questions.Search;

namespace Nclex.Api.Controllers;

/// <summary>
/// Questions in the admin console (api.md section 8). Responses include answer keys and
/// rationales, so every action is limited to content staff.
/// </summary>
[ApiController]
[Route("api/v1/admin/questions")]
[Authorize(Policy = Policies.ContentStaff)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class AdminQuestionsController : ControllerBase
{
    /// <summary>Search questions; status=in_review is the reviewer queue, oldest first.</summary>
    [HttpGet]
    [ProducesResponseType<QuestionSearchPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<QuestionSearchPage> Search(
        [FromQuery] SearchQuestionsRequest request,
        [FromServices] SearchQuestionsHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>Create a question with version 1 as a draft.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.ContentAuthor)]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> Create(
        CreateQuestionRequest request,
        [FromServices] CreateQuestionHandler handler,
        CancellationToken cancellationToken)
    {
        var created = await handler.HandleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>The question with all versions, full payloads included.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AdminQuestionView> Get(
        Guid id,
        [FromServices] AdminQuestionReader reader,
        CancellationToken cancellationToken) =>
        reader.ReadAsync(id, cancellationToken);

    /// <summary>Start a new draft version, cloned from the current one. 409 while another version is open.</summary>
    [HttpPost("{id:guid}/versions")]
    [Authorize(Policy = Policies.ContentAuthor)]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> CreateVersion(
        Guid id,
        [FromServices] CreateVersionHandler handler,
        CancellationToken cancellationToken)
    {
        var question = await handler.HandleAsync(id, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, question);
    }

    /// <summary>Remove the question from future sets.</summary>
    [HttpPost("{id:guid}/retire")]
    [Authorize(Policy = Policies.ContentReviewer)]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<AdminQuestionView> Retire(
        Guid id,
        [FromServices] RetireQuestionHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, cancellationToken);
}
