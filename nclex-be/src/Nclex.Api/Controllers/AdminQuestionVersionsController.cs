using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Admin.Questions;
using Nclex.Application.Admin.Questions.PreviewGrade;
using Nclex.Application.Admin.Questions.Publish;
using Nclex.Application.Admin.Questions.Review;
using Nclex.Application.Admin.Questions.UpdateDraft;
using Nclex.Application.Attempts.Reveal;

namespace Nclex.Api.Controllers;

/// <summary>
/// The review workflow on one version (api.md section 8): edit the draft, then
/// draft → in_review → approved → published, or in_review → rejected. Each action returns the whole
/// question, so the console can redraw its version history from the response.
/// </summary>
[ApiController]
[Route("api/v1/admin/question-versions")]
[Authorize(Policy = Policies.ContentStaff)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
public sealed class AdminQuestionVersionsController : ControllerBase
{
    /// <summary>Edit a draft; checked against its item type's schema.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.ContentAuthor)]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<AdminQuestionView> Update(
        Guid id,
        DraftInput draft,
        [FromServices] UpdateDraftHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, draft, cancellationToken);

    /// <summary>draft → in_review.</summary>
    [HttpPost("{id:guid}/submit-for-review")]
    [Authorize(Policy = Policies.ContentAuthor)]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<AdminQuestionView> SubmitForReview(
        Guid id,
        [FromServices] SubmitForReviewHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, cancellationToken);

    /// <summary>in_review → approved. 403 FORBIDDEN for the version's own author.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = Policies.ContentReviewer)]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<AdminQuestionView> Approve(
        Guid id,
        [FromBody] ReviewRequest? request,
        [FromServices] ApproveVersionHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, request ?? new ReviewRequest(null), cancellationToken);

    /// <summary>in_review → rejected, with notes for the author.</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = Policies.ContentReviewer)]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<AdminQuestionView> Reject(
        Guid id,
        ReviewRequest request,
        [FromServices] RejectVersionHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, request, cancellationToken);

    /// <summary>approved → published; the previous published version becomes superseded.</summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = Policies.ContentReviewer)]
    [ProducesResponseType<AdminQuestionView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public Task<AdminQuestionView> Publish(
        Guid id,
        [FromServices] PublishVersionHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, cancellationToken);

    /// <summary>Test the key: grade a sample answer with the real grader. Nothing is stored.</summary>
    [HttpPost("{id:guid}/preview-grade")]
    [ProducesResponseType<ItemGrade>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<ItemGrade> PreviewGrade(
        Guid id,
        PreviewGradeRequest request,
        [FromServices] PreviewGradeHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, request, cancellationToken);
}
