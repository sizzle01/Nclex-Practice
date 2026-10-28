using Nclex.Application.Abstractions;
using Nclex.Domain.Audit;
using Nclex.Domain.Content;

namespace Nclex.Application.Admin.Questions.Review;

/// <summary>
/// POST /admin/question-versions/{id}/approve: in_review → approved, by a reviewer or admin who is
/// not the author (database.md 2.4). Approved versions wait for someone to publish them.
/// </summary>
public sealed class ApproveVersionHandler(
    IAppDbContext db,
    VersionLoader loader,
    AdminQuestionReader reader,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<AdminQuestionView> HandleAsync(Guid versionId, ReviewRequest request, CancellationToken cancellationToken)
    {
        var (question, version) = await loader.LoadAsync(versionId, cancellationToken);
        VersionLoader.EnsureActive(question);
        VersionLoader.EnsureAllowed(VersionAction.Approve, version, currentUser.UserId);

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        version.Approve(currentUser.UserId, notes, clock.GetUtcNow());
        audit.RecordAdminEvent(AuditActions.VersionApproved, currentUser.UserId, AuditActions.QuestionVersionEntity, version.Id, new { questionId = question.Id, version = version.Version });
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(question.Id, cancellationToken);
    }
}
