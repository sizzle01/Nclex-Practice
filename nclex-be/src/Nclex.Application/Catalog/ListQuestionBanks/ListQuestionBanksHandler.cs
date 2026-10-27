using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog.ListQuestionBanks;

/// <summary>GET /question-banks: the published banks of a track, with item counts and lock state.</summary>
public sealed class ListQuestionBanksHandler(IAppDbContext db, PlanTierGuard planTierGuard)
{
    public async Task<QuestionBankList> HandleAsync(ListQuestionBanksRequest request, CancellationToken cancellationToken)
    {
        var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
        var track = request.Track ?? viewer.Track;

        var banks = await db.QuestionBanks.AsNoTracking()
            .Where(b => b.IsPublished && b.Track == track)
            .OrderBy(b => b.Name).ThenBy(b => b.Id)
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Description,
                b.AccessTier,
                // Retired or never-published questions can't be served, so they don't count.
                QuestionCount = db.QuestionBankItems
                    .Where(i => i.BankId == b.Id)
                    .Join(db.Questions, i => i.QuestionId, q => q.Id, (_, q) => q)
                    .Count(q => q.Status == QuestionStatus.Active && q.CurrentVersionId != null),
            })
            .ToListAsync(cancellationToken);

        return new QuestionBankList(
            [.. banks.Select(b => new QuestionBankSummary(b.Id, b.Name, b.Description, b.AccessTier, b.QuestionCount, viewer.IsLocked(b.AccessTier)))]);
    }
}
