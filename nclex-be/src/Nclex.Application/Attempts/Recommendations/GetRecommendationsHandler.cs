using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;
using Nclex.Domain.Recommendations;

namespace Nclex.Application.Attempts.Recommendations;

/// <summary>
/// GET /attempts/{id}/recommendations: the subtopics to study after a test. Worked out on each read
/// from today's tags, so correcting a tag also improves past results. It has its own endpoint so a
/// failure here can't take the results page down.
/// </summary>
public sealed class GetRecommendationsHandler(IAppDbContext db, AttemptStore store)
{
    public async Task<AttemptRecommendationsView> HandleAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await store.FindOwnedAsync(attemptId, cancellationToken);
        if (attempt.Status != AttemptStatus.Submitted)
        {
            throw new AppException(attempt.Status == AttemptStatus.Abandoned ? AttemptErrors.NotActive : AttemptErrors.NotSubmitted);
        }

        var answers = await (
            from item in db.AttemptItems.AsNoTracking()
            where item.AttemptId == attempt.Id && item.Response != null
            join version in db.QuestionVersions.AsNoTracking() on item.QuestionVersionId equals version.Id
            join tag in db.QuestionSubtopics.AsNoTracking() on version.QuestionId equals tag.QuestionId
            join subtopic in db.Subtopics.AsNoTracking() on tag.SubtopicId equals subtopic.Id
            where subtopic.IsActive
            select new TaggedAnswer(subtopic.Id, subtopic.Name, item.IsCorrect == true))
            .ToListAsync(cancellationToken);

        return new AttemptRecommendationsView(
            attempt.Id,
            [.. SubtopicRecommendations.From(answers).Select(gap => new RecommendedSubtopic(gap.SubtopicId, gap.Name, gap.Missed, gap.Answered))]);
    }
}
