using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Audit;

namespace Nclex.Application.Admin.Questions.Retire;

/// <summary>
/// POST /admin/questions/{id}/retire: removes the question from future sets (api.md section 8).
/// Attempt start and the custom builder already skip retired questions; attempts in progress keep it.
/// </summary>
public sealed class RetireQuestionHandler(
    IAppDbContext db,
    AdminQuestionReader reader,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    public async Task<AdminQuestionView> HandleAsync(Guid questionId, CancellationToken cancellationToken)
    {
        var question = await db.Questions.SingleOrDefaultAsync(q => q.Id == questionId, cancellationToken)
            ?? throw new AppException(AdminErrors.QuestionNotFound);
        VersionLoader.EnsureActive(question);

        question.Retire(clock.GetUtcNow());
        audit.RecordEntityEvent(AuditActions.QuestionRetired, currentUser.UserId, AuditActions.QuestionEntity, question.Id.ToString());
        await db.SaveChangesAsync(cancellationToken);
        return await reader.ReadAsync(question.Id, cancellationToken);
    }
}
