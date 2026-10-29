using Nclex.Application.Abstractions;
using Nclex.Domain.Audit;
using Nclex.Domain.Content;

namespace Nclex.Application.Admin.Questions.Review;

/// <summary>
/// POST /admin/question-versions/{id}/submit-for-review: draft → in_review. Its media must already
/// be accessible, so the reviewer sees exactly what candidates would.
/// </summary>
public sealed class SubmitForReviewHandler(
    IAppDbContext db,
    VersionLoader loader,
    MediaReadiness mediaReadiness,
    AdminQuestionReader reader,
    ICurrentUser currentUser,
    IAuditLog audit)
{
    public async Task<AdminQuestionView> HandleAsync(Guid versionId, CancellationToken cancellationToken)
    {
        var (question, version) = await loader.LoadAsync(versionId, cancellationToken);
        VersionLoader.EnsureActive(question);
        VersionLoader.EnsureAllowed(VersionAction.SubmitForReview, version, currentUser.UserId);
        await mediaReadiness.EnsureReadyAsync(version, cancellationToken);

        version.SubmitForReview();
        audit.RecordEntityEvent(AuditActions.VersionSubmitted, currentUser.UserId, AuditActions.QuestionVersionEntity, version.Id.ToString(), new { questionId = question.Id, version = version.Version });
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(question.Id, cancellationToken);
    }
}
