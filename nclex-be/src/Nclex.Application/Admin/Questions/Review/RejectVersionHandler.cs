using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Audit;
using Nclex.Domain.Content;

namespace Nclex.Application.Admin.Questions.Review;

/// <summary>
/// POST /admin/question-versions/{id}/reject: in_review → rejected, with notes (api.md section 8).
/// A rejected version stays as a record; the author fixes the problems in a new version.
/// </summary>
public sealed class RejectVersionHandler(
    IAppDbContext db,
    VersionLoader loader,
    AdminQuestionReader reader,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<AdminQuestionView> HandleAsync(Guid versionId, ReviewRequest request, CancellationToken cancellationToken)
    {
        // Required here rather than in the validator, which approve shares: a rejection must say why.
        if (string.IsNullOrWhiteSpace(request.Notes))
        {
            throw AppException.ForField("notes", ValidationErrorCodes.Required);
        }

        var (question, version) = await loader.LoadAsync(versionId, cancellationToken);
        VersionLoader.EnsureAllowed(VersionAction.Reject, version, currentUser.UserId);

        version.Reject(currentUser.UserId, request.Notes.Trim(), clock.GetUtcNow());
        audit.RecordAdminEvent(AuditActions.VersionRejected, currentUser.UserId, AuditActions.QuestionVersionEntity, version.Id, new { questionId = question.Id, version = version.Version });
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(question.Id, cancellationToken);
    }
}
