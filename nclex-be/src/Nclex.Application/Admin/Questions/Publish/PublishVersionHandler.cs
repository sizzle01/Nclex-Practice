using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Audit;
using Nclex.Domain.Content;

namespace Nclex.Application.Admin.Questions.Publish;

/// <summary>
/// POST /admin/question-versions/{id}/publish: approved → published. In one transaction the
/// previous published version becomes superseded and <c>questions.current_version_id</c> moves to
/// this one (database.md 2.4). Attempts already started keep the version they resolved, because
/// attempt items store <c>question_version_id</c>; new attempts and sets get this version.
/// </summary>
public sealed class PublishVersionHandler(
    IAppDbContext db,
    VersionLoader loader,
    MediaReadiness mediaReadiness,
    AdminQuestionReader reader,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<AdminQuestionView> HandleAsync(Guid versionId, CancellationToken cancellationToken)
    {
        var (question, version) = await loader.LoadAsync(versionId, cancellationToken);
        VersionLoader.EnsureActive(question);
        VersionLoader.EnsureAllowed(VersionAction.Publish, version, currentUser.UserId);
        // Checked again: an asset's alt text or captions may have changed since the review.
        await mediaReadiness.EnsureReadyAsync(version, cancellationToken);

        var previous = question.CurrentVersionId is { } currentId
            ? await db.QuestionVersions.SingleAsync(v => v.Id == currentId, cancellationToken)
            : null;
        question.Publish(version, previous, clock.GetUtcNow());
        audit.RecordAdminEvent(AuditActions.VersionPublished, currentUser.UserId, AuditActions.QuestionEntity, question.Id, new { versionId = version.Id, version = version.Version, supersededVersionId = previous?.Id });

        // One SaveChanges is one transaction: the superseded version, the published one and the
        // question's pointer change together or not at all.
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(question.Id, cancellationToken);
    }
}
