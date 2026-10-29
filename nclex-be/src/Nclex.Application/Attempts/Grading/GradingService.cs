using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Results;
using Nclex.Application.Common;
using Nclex.Application.Notifications;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Attempts.Grading;

/// <summary>
/// Runs the pure graders of the item-type modules on locked items and turns the grades into a
/// result. The maths lives in Nclex.Domain (<see cref="ResultCalculator"/>); this class loads what
/// it needs and stores what comes out.
/// </summary>
public sealed class GradingService(
    IAppDbContext db,
    ItemTypeRegistry itemTypes,
    IReadinessPolicy readiness,
    DimensionNames dimensionNames,
    INotificationPublisher notifications,
    NclexMetrics metrics,
    TimeProvider clock) : IGradingService
{
    public async Task GradeLockedTutorItemsAsync(Attempt attempt, CancellationToken cancellationToken)
    {
        if (attempt.Settings.RevealMode != RevealMode.Tutor)
        {
            return;
        }

        // Local holds the items this request loaded, with their unsaved changes, so it sees a lock
        // made a moment ago that a database query would not.
        var newlyLocked = db.AttemptItems.Local
            .Where(i => i.AttemptId == attempt.Id && i.IsLocked && !i.IsGraded)
            .ToList();
        await GradeAsync(newlyLocked, cancellationToken);
    }

    public async Task<AttemptResult> FinalizeAttemptAsync(
        Attempt attempt,
        IReadOnlyList<AttemptItem> items,
        IReadOnlySet<int> seenPositions,
        CancellationToken cancellationToken)
    {
        // Grading duration and any grading failure are on-call signals (architecture.md section 9).
        var started = clock.GetTimestamp();
        try
        {
            var result = await GradeAndScoreAsync(attempt, items, seenPositions, cancellationToken);
            metrics.RecordGrading(clock.GetElapsedTime(started), succeeded: true);
            return result;
        }
        catch
        {
            metrics.RecordGrading(clock.GetElapsedTime(started), succeeded: false);
            throw;
        }
    }

    private async Task<AttemptResult> GradeAndScoreAsync(
        Attempt attempt,
        IReadOnlyList<AttemptItem> items,
        IReadOnlySet<int> seenPositions,
        CancellationToken cancellationToken)
    {
        await GradeAsync([.. items.Where(i => !i.IsGraded)], cancellationToken);

        var versions = await VersionTagsAsync(items, cancellationToken);
        var graded = items.Select(i => new GradedItem(
            i.PointsEarned!.Value,
            i.MaxPoints!.Value,
            i.IsCorrect!.Value,
            i.IsAnswered,
            i.TimeSpentMs,
            versions[i.QuestionVersionId].Tags)).ToList();

        var now = clock.GetUtcNow();
        var totals = ResultCalculator.Totals(graded);
        var threshold = attempt.Settings.PassThreshold;
        var result = AttemptResult.Create(attempt.Id, totals, threshold, readiness.Evaluate(totals.ScorePct, threshold), now);
        var scores = ResultCalculator.Breakdown(graded).Select(t => AttemptCategoryScore.From(attempt.Id, t)).ToList();
        db.AttemptResults.Add(result);
        db.AttemptCategoryScores.AddRange(scores);

        var seen = items.Where(i => seenPositions.Contains(i.Position)).ToList();
        await RecordHistoryAsync(attempt.UserId, seen, versions, now, cancellationToken);
        await notifications.PublishAsync(await ResultNotificationAsync(attempt, result, scores, cancellationToken), cancellationToken);
        return result;
    }

    private async Task GradeAsync(List<AttemptItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        // The answer key is read here and nowhere near a candidate response or a log line.
        var versionIds = items.Select(i => i.QuestionVersionId).Distinct().ToList();
        var versions = await db.QuestionVersions.AsNoTracking()
            .Where(v => versionIds.Contains(v.Id))
            .Select(v => new { v.Id, v.ItemType, v.Content, v.AnswerKey })
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var now = clock.GetUtcNow();
        foreach (var item in items)
        {
            var version = versions[item.QuestionVersionId];
            var grade = itemTypes.Get(version.ItemType).Grade(
                version.Content.RootElement,
                version.AnswerKey.RootElement,
                item.Response?.RootElement);
            item.RecordGrade(grade, now);
        }
    }

    private async Task<Dictionary<Guid, VersionTags>> VersionTagsAsync(
        IReadOnlyList<AttemptItem> items,
        CancellationToken cancellationToken)
    {
        var versionIds = items.Select(i => i.QuestionVersionId).Distinct().ToList();
        var rows = await (
                from version in db.QuestionVersions
                join category in db.ClientNeedsCategories on version.ClientNeedsId equals category.Id
                join bodySystem in db.BodySystems on version.BodySystemId equals bodySystem.Id
                where versionIds.Contains(version.Id)
                select new
                {
                    version.Id,
                    version.QuestionId,
                    version.ItemType,
                    version.CjmmSkill,
                    version.Difficulty,
                    category.Code,
                    ParentCode = db.ClientNeedsCategories.Where(p => p.Id == category.ParentId).Select(p => p.Code).FirstOrDefault(),
                    bodySystem.Slug,
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            r => r.Id,
            r => new VersionTags(
                r.QuestionId,
                // Questions are tagged to a leaf; a top-level leaf such as Psychosocial Integrity is its own parent.
                new ItemTags(r.Code, r.ParentCode ?? r.Code, r.Slug, r.ItemType, r.CjmmSkill, r.Difficulty)));
    }

    /// <summary>
    /// Feeds the builder's "unused" and "incorrect" pools (database.md 2.7). Only items the candidate
    /// saw count: one they never reached is still unused.
    /// </summary>
    private async Task RecordHistoryAsync(
        Guid userId,
        List<AttemptItem> seen,
        Dictionary<Guid, VersionTags> versions,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        // A set holds each question once, but the latest position wins should one ever repeat.
        var latestByQuestion = seen
            .GroupBy(i => versions[i.QuestionVersionId].QuestionId)
            .ToDictionary(g => g.Key, g => g.MaxBy(i => i.Position)!);
        var questionIds = latestByQuestion.Keys.ToList();
        var existing = await db.UserQuestionHistory
            .Where(h => h.UserId == userId && questionIds.Contains(h.QuestionId))
            .ToDictionaryAsync(h => h.QuestionId, cancellationToken);

        foreach (var (questionId, item) in latestByQuestion)
        {
            if (existing.TryGetValue(questionId, out var history))
            {
                history.RecordSeen(item.IsCorrect, now);
            }
            else
            {
                db.UserQuestionHistory.Add(UserQuestionHistory.FirstSeen(userId, questionId, item.IsCorrect, now));
            }
        }
    }

    private async Task<NewNotification> ResultNotificationAsync(
        Attempt attempt,
        AttemptResult result,
        IReadOnlyList<AttemptCategoryScore> scores,
        CancellationToken cancellationToken)
    {
        var setTitle = await db.TestSets.AsNoTracking()
            .Where(s => s.Id == attempt.TestSetId)
            .Select(s => s.Title)
            .SingleAsync(cancellationToken);
        var displayName = await db.Users.AsNoTracking()
            .Where(u => u.Id == attempt.UserId)
            .Select(u => u.DisplayName)
            .SingleAsync(cancellationToken);
        var names = await dimensionNames.ForTrackAsync(attempt.Track, cancellationToken);
        var weakest = WeakestAreas.From(scores)
            .Select(s => new WeakCategory(s.DimensionKey, names.Name(s.Dimension, s.DimensionKey)))
            .ToList();
        return ResultNotifications.For(attempt, result, setTitle, displayName, weakest);
    }

    private sealed record VersionTags(Guid QuestionId, ItemTags Tags);
}
