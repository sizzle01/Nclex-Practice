using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Attempts.GetItem;

/// <summary>
/// GET /attempts/{id}/items/{position}: the item to display. It reads only the columns a candidate
/// may see, so the answer key and rationale never leave the database here.
/// </summary>
public sealed class GetItemHandler(IAppDbContext db, AttemptStore store, ItemTypeRegistry itemTypes)
{
    public async Task<CandidateItemView> HandleAsync(Guid attemptId, int position, CancellationToken cancellationToken)
    {
        var attempt = await store.FindOwnedAsync(attemptId, cancellationToken);
        switch (attempt.Status)
        {
            case AttemptStatus.Abandoned:
                throw new AppException(AttemptErrors.NotActive);
            case AttemptStatus.Paused:
                // Content is hidden during a pause (PRD 5.4).
                throw new AppException(AttemptErrors.Paused);
        }

        var row = await (
                from attemptItem in db.AttemptItems
                join version in db.QuestionVersions on attemptItem.QuestionVersionId equals version.Id
                where attemptItem.AttemptId == attempt.Id && attemptItem.Position == position
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
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new AppException(AttemptErrors.ItemNotFound);

        // The seed keeps the shuffled order the same each time this item is shown in this attempt.
        var content = itemTypes.Get(row.ItemType).Present(row.Content.RootElement, $"{attempt.Id}:{position}");
        var item = row.Item;
        return new CandidateItemView(
            item.Position,
            item.QuestionVersionId,
            row.ItemType,
            row.Stem,
            content,
            row.Exhibits.RootElement.Clone(),
            await CaseStudyAsync(row.CaseStudyId, row.CasePosition, cancellationToken),
            item.State,
            item.Response?.RootElement.Clone(),
            item.ClientSeq,
            item.Flagged,
            item.StruckOptions,
            row.TimeAllotmentSec);
    }

    private async Task<CaseStudyView?> CaseStudyAsync(Guid? caseStudyId, short? casePosition, CancellationToken cancellationToken)
    {
        if (caseStudyId is null || casePosition is null)
        {
            return null;
        }

        var caseStudy = await db.CaseStudies.AsNoTracking().SingleAsync(c => c.Id == caseStudyId, cancellationToken);
        return new CaseStudyView(caseStudy.Id, caseStudy.Title, caseStudy.Scenario.RootElement.Clone(), casePosition.Value);
    }
}
