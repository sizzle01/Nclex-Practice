using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Reveal;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.GetItem;

/// <summary>
/// Builds <see cref="CandidateItemView"/>s for the item endpoint and the review page. It reads only
/// the columns a candidate may see; anything from the answer key comes through <see cref="RevealReader"/>.
/// </summary>
public sealed class ItemViewReader(IAppDbContext db, ItemTypeRegistry itemTypes, RevealReader reveals)
{
    /// <summary>The items at <paramref name="positions"/>, in position order; positions with no item are left out.</summary>
    public async Task<IReadOnlyList<CandidateItemView>> ReadAsync(
        Attempt attempt,
        IReadOnlyList<int> positions,
        CancellationToken cancellationToken)
    {
        var rows = await (
                from attemptItem in db.AttemptItems
                join version in db.QuestionVersions on attemptItem.QuestionVersionId equals version.Id
                where attemptItem.AttemptId == attempt.Id && positions.Contains(attemptItem.Position)
                orderby attemptItem.Position
                select new
                {
                    Item = attemptItem,
                    version.ItemType,
                    version.Stem,
                    version.Content,
                    version.Exhibits,
                    version.TimeAllotmentSec,
                    version.CaseStudyId,
                    version.CasePosition,
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var revealed = await reveals.ReadAsync(attempt, [.. rows.Select(r => r.Item)], cancellationToken);
        var caseStudies = await CaseStudiesAsync([.. rows.Select(r => r.CaseStudyId)], cancellationToken);
        return [.. rows.Select(row =>
        {
            var item = row.Item;
            // The seed keeps the shuffled order the same each time this item is shown in this attempt.
            var content = itemTypes.Get(row.ItemType).Present(row.Content.RootElement, $"{attempt.Id}:{item.Position}");
            var caseStudy = row.CaseStudyId is { } caseId && row.CasePosition is { } casePosition
                ? caseStudies[caseId] with { Position = casePosition }
                : null;
            return new CandidateItemView(
                item.Position,
                item.QuestionVersionId,
                row.ItemType,
                row.Stem,
                content,
                row.Exhibits.RootElement.Clone(),
                caseStudy,
                item.State,
                item.Response?.RootElement.Clone(),
                item.ClientSeq,
                item.Flagged,
                item.StruckOptions,
                row.TimeAllotmentSec,
                revealed.GetValueOrDefault(item.Position));
        })];
    }

    private async Task<Dictionary<Guid, CaseStudyView>> CaseStudiesAsync(IReadOnlyList<Guid?> caseStudyIds, CancellationToken cancellationToken)
    {
        var ids = caseStudyIds.OfType<Guid>().Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var caseStudies = await db.CaseStudies.AsNoTracking().Where(c => ids.Contains(c.Id)).ToListAsync(cancellationToken);
        // Position is filled in per item by the caller.
        return caseStudies.ToDictionary(c => c.Id, c => new CaseStudyView(c.Id, c.Title, c.Scenario.RootElement.Clone(), 0));
    }
}
