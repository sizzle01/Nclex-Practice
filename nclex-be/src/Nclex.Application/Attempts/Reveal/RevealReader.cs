using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Attempts.Reveal;

/// <summary>
/// Reads answer keys and rationales for items the candidate may now see. The only class that sends
/// those columns towards a candidate, and it checks <see cref="RevealRules"/> itself, so a caller's
/// mistake cannot leak an answer early.
/// </summary>
public sealed class RevealReader(IAppDbContext db, ItemTypeRegistry itemTypes)
{
    /// <summary>The reveal of each item in <paramref name="items"/> that may be revealed, by position.</summary>
    public async Task<IReadOnlyDictionary<int, ItemReveal>> ReadAsync(
        Attempt attempt,
        IReadOnlyList<AttemptItem> items,
        CancellationToken cancellationToken)
    {
        var revealable = items.Where(i => RevealRules.CanReveal(attempt, i)).ToList();
        if (revealable.Count == 0)
        {
            return new Dictionary<int, ItemReveal>();
        }

        var versionIds = revealable.Select(i => i.QuestionVersionId).Distinct().ToList();
        var versions = await (
                from version in db.QuestionVersions
                join category in db.ClientNeedsCategories on version.ClientNeedsId equals category.Id
                join bodySystem in db.BodySystems on version.BodySystemId equals bodySystem.Id
                where versionIds.Contains(version.Id)
                select new
                {
                    version.Id,
                    version.ItemType,
                    version.Content,
                    version.AnswerKey,
                    version.Rationale,
                    version.EducationalObjective,
                    version.CjmmSkill,
                    version.Difficulty,
                    category.Code,
                    bodySystem.Slug,
                })
            .AsNoTracking()
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        return revealable.ToDictionary(
            item => item.Position,
            item =>
            {
                var version = versions[item.QuestionVersionId];
                return new ItemReveal(
                    version.AnswerKey.RootElement.Clone(),
                    GradeOf(item, itemTypes.Get(version.ItemType), version.Content.RootElement, version.AnswerKey.RootElement),
                    version.Rationale.Deserialize<Rationale>(ItemJson.Options)!,
                    version.EducationalObjective,
                    new RevealTags(version.Code, version.Slug, version.CjmmSkill, version.Difficulty));
            });
    }

    /// <summary>
    /// The stored grade. An attempt finalized before grading existed has none, so it is worked out
    /// here instead; graders are pure, so the answer is the same.
    /// </summary>
    private static ItemGrade GradeOf(AttemptItem item, IItemTypeModule module, JsonElement content, JsonElement key)
    {
        if (item is { IsGraded: true, GradeDetail: { } detail })
        {
            return new ItemGrade(item.PointsEarned!.Value, item.MaxPoints!.Value, item.IsCorrect!.Value, GradeDetailJson.Read(detail));
        }

        var grade = module.Grade(content, key, item.Response?.RootElement);
        return new ItemGrade(grade.Points, grade.MaxPoints, grade.IsCorrect, grade.Detail);
    }
}
