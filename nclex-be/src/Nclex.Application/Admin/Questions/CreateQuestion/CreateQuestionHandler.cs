using Nclex.Application.Abstractions;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Questions.CreateQuestion;

/// <summary>
/// POST /admin/questions: a new question whose version 1 is a draft written by the caller
/// (api.md section 8). It reaches candidates only after review and publishing.
/// </summary>
public sealed class CreateQuestionHandler(
    IAppDbContext db,
    AuthoringValidator validator,
    AdminQuestionReader reader,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<AdminQuestionView> HandleAsync(CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var item = await validator.ValidateAsync(request.Draft, request.Track, placement: null, cancellationToken);
        var now = clock.GetUtcNow();
        var question = Question.Create(Guid.CreateVersion7(now), request.Track, request.AccessTier ?? PlanTier.Free, currentUser.UserId, now);
        var draft = QuestionVersion.CreateDraft(question.Id, 1, item, currentUser.UserId, now);
        db.Questions.Add(question);
        db.QuestionVersions.Add(draft);
        audit.RecordAdminEvent(AuditActions.QuestionCreated, currentUser.UserId, AuditActions.QuestionEntity, question.Id, new { versionId = draft.Id, itemType = item.ItemType });
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(question.Id, cancellationToken);
    }
}
