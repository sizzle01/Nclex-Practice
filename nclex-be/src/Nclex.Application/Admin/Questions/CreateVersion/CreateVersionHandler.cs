using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Audit;
using Nclex.Domain.Content;

namespace Nclex.Application.Admin.Questions.CreateVersion;

/// <summary>
/// POST /admin/questions/{id}/versions: a new draft cloned from the current version (api.md
/// section 8), so an author can change a published question without touching what candidates see.
/// A question that was never published (for example its only version was rejected) is cloned from
/// its latest version instead.
/// </summary>
public sealed class CreateVersionHandler(
    IAppDbContext db,
    AdminQuestionReader reader,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<AdminQuestionView> HandleAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var question = await db.Questions.AsNoTracking().SingleOrDefaultAsync(q => q.Id == questionId, cancellationToken)
            ?? throw new AppException(AdminErrors.QuestionNotFound);
        VersionLoader.EnsureActive(question);

        var versions = await db.QuestionVersions.AsNoTracking()
            .Where(v => v.QuestionId == questionId)
            .OrderByDescending(v => v.Version)
            .ToListAsync(cancellationToken);
        if (versions.FirstOrDefault(v => VersionWorkflow.IsOpen(v.Status)) is { } open)
        {
            throw new AppException(AdminErrors.WrongStatus(
                $"Version {open.Version} is still being worked on. Finish it (publish or reject) before starting another."));
        }

        var source = versions.FirstOrDefault(v => v.Id == question.CurrentVersionId) ?? versions[0];
        var now = clock.GetUtcNow();
        var draft = source.CloneAsDraft(versions[0].Version + 1, currentUser.UserId, now);
        db.QuestionVersions.Add(draft);
        audit.RecordAdminEvent(AuditActions.VersionCreated, currentUser.UserId, AuditActions.QuestionVersionEntity, draft.Id, new { questionId, version = draft.Version, clonedFrom = source.Id });
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(questionId, cancellationToken);
    }
}
