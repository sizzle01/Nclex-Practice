using Nclex.Application.Abstractions;
using Nclex.Domain.Audit;
using Nclex.Domain.Content;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Questions.UpdateDraft;

/// <summary>
/// PUT /admin/question-versions/{id}: replaces a draft's content and tags, checked against the item
/// type's schema (api.md section 8). Any author may edit any draft; the version keeps its author,
/// who is the one barred from reviewing it. A case-study item keeps its place in the case.
/// </summary>
public sealed class UpdateDraftHandler(
    IAppDbContext db,
    VersionLoader loader,
    AuthoringValidator validator,
    AdminQuestionReader reader,
    ICurrentUser currentUser,
    IAuditLog audit)
{
    public async Task<AdminQuestionView> HandleAsync(Guid versionId, DraftInput draft, CancellationToken cancellationToken)
    {
        var (question, version) = await loader.LoadAsync(versionId, cancellationToken);
        VersionLoader.EnsureActive(question);
        VersionLoader.EnsureAllowed(VersionAction.Edit, version, currentUser.UserId);

        var placement = version.CaseStudyId is { } caseId && version.CasePosition is { } position ? new CasePlacement(caseId, position) : null;
        var item = await validator.ValidateAsync(draft, question.Track, placement, cancellationToken);
        version.EditDraft(item);
        audit.RecordEntityEvent(AuditActions.VersionEdited, currentUser.UserId, AuditActions.QuestionVersionEntity, version.Id.ToString(), new { questionId = question.Id, version = version.Version });
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(question.Id, cancellationToken);
    }
}
