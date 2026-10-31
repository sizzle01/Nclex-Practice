using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Admin.Questions;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.Recommendations;

namespace Nclex.Application.Admin.Subtopics;

/// <summary>
/// The tags on a question. PUT /admin/questions/{id}/subtopics replaces them, including on a published
/// question: tags are study metadata and sit outside the version workflow (see <see cref="QuestionSubtopic"/>).
/// </summary>
public sealed class QuestionSubtopicTags(IAppDbContext db, AdminQuestionReader reader, ICurrentUser currentUser, IAuditLog audit)
{
    private const string Path = "subtopics";

    public async Task<AdminQuestionView> SetAsync(Guid questionId, SetQuestionSubtopicsRequest request, CancellationToken cancellationToken)
    {
        var question = await db.Questions.SingleOrDefaultAsync(q => q.Id == questionId, cancellationToken)
            ?? throw new Common.AppException(AdminErrors.QuestionNotFound);
        VersionLoader.EnsureActive(question);

        // The newest version is the one an author is working on, so its category decides.
        var clientNeedsId = await db.QuestionVersions.AsNoTracking()
            .Where(v => v.QuestionId == questionId)
            .OrderByDescending(v => v.Version)
            .Select(v => v.ClientNeedsId)
            .FirstAsync(cancellationToken);
        var chosen = await db.Subtopics.AsNoTracking()
            .Where(s => request.SubtopicIds.Contains(s.Id))
            .ToListAsync(cancellationToken);
        if (chosen.Count != request.SubtopicIds.Count || chosen.Any(s => !s.IsActive || s.ClientNeedsId != clientNeedsId))
        {
            throw AuthoringValidator.Invalid([new ValidationIssue("subtopicIds", ValidationIssueCodes.InvalidValue, "Choose active subtopics from this question's Client Needs subcategory.")]);
        }

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        await db.QuestionSubtopics.Where(t => t.QuestionId == questionId).ExecuteDeleteAsync(cancellationToken);
        db.QuestionSubtopics.AddRange(request.SubtopicIds.Select(id => QuestionSubtopic.Create(questionId, id)));
        audit.RecordEntityEvent(AuditActions.QuestionSubtopicsSet, currentUser.UserId, AuditActions.QuestionEntity, questionId.ToString(), new { request.SubtopicIds });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadAsync(questionId, cancellationToken);
    }

    /// <summary>
    /// Stops a version going to review without usable tags, because a question without them can never
    /// appear in a candidate's suggestions. The author may have moved the draft to another category.
    /// </summary>
    public async Task EnsureReadyForReviewAsync(QuestionVersion version, CancellationToken cancellationToken)
    {
        var tags = await (
            from tag in db.QuestionSubtopics.AsNoTracking()
            where tag.QuestionId == version.QuestionId
            join subtopic in db.Subtopics.AsNoTracking() on tag.SubtopicId equals subtopic.Id
            select subtopic)
            .ToListAsync(cancellationToken);
        if (tags.Count < SubtopicNames.MinPerQuestion)
        {
            throw AuthoringValidator.Invalid([new ValidationIssue(Path, ValidationIssueCodes.Required, "Choose at least one subtopic before sending this question for review.")]);
        }

        var unusable = tags.Where(s => !s.IsActive || s.ClientNeedsId != version.ClientNeedsId).ToList();
        if (unusable.Count > 0)
        {
            throw AuthoringValidator.Invalid(unusable.Select(s => new ValidationIssue(Path, ValidationIssueCodes.InvalidValue, $"\"{s.Name}\" is inactive or belongs to another Client Needs subcategory. Update this question's subtopics.")));
        }
    }
}
