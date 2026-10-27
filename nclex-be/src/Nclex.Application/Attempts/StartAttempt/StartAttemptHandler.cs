using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth;
using Nclex.Application.Catalog;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.StartAttempt;

/// <summary>
/// POST /attempts: starts an attempt of a test set (architecture.md 5.3). The exam timer does not
/// run yet; the player opens the attempt when it loads. Also the entry point for the custom
/// builder's <c>startNow</c> (F07), which passes the id of the set it just created.
/// </summary>
public sealed class StartAttemptHandler(
    IAppDbContext db,
    PlanTierGuard planTierGuard,
    AttemptStateReader stateReader,
    TimeProvider clock)
{
    public async Task<StartAttemptResult> HandleAsync(StartAttemptRequest request, CancellationToken cancellationToken)
    {
        var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
        var set = await db.TestSets.AsNoTracking()
            .Where(TestSetVisibility.VisibleTo(viewer.UserId))
            .SingleOrDefaultAsync(s => s.Id == request.TestSetId, cancellationToken)
            ?? throw new AppException(CatalogErrors.TestSetNotFound);

        // Sample sets are the taster an unverified user may try (docs/README.md assumption 5).
        if (!set.IsSample && !await IsEmailVerifiedAsync(viewer.UserId, cancellationToken))
        {
            throw new AppException(AuthErrors.EmailNotVerified);
        }

        PlanTierGuard.EnsureAllowed(viewer, set.AccessTier);

        // api.md: starting a prebuilt set again resumes its draft instead of making a second one.
        if (set.Kind == TestSetKind.Prebuilt && await FindDraftAsync(viewer.UserId, set.Id, cancellationToken) is { } draft)
        {
            return new StartAttemptResult(await stateReader.ReadAsync(draft, cancellationToken), Created: false);
        }

        var versionIds = await CurrentVersionsAsync(set.Id, cancellationToken);
        if (versionIds.Count == 0)
        {
            throw new AppException(AttemptErrors.NoQuestionsAvailable);
        }

        var (attempt, items) = Attempt.Start(viewer.UserId, set, versionIds, clock.GetUtcNow());
        db.Attempts.Add(attempt);
        db.AttemptItems.AddRange(items);
        await db.SaveChangesAsync(cancellationToken);

        return new StartAttemptResult(await stateReader.ReadAsync(attempt, cancellationToken), Created: true);
    }

    private Task<bool> IsEmailVerifiedAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Id == userId && u.EmailVerifiedAt != null, cancellationToken);

    private Task<Attempt?> FindDraftAsync(Guid userId, Guid setId, CancellationToken cancellationToken) =>
        db.Attempts.AsNoTracking()
            .Where(a => a.UserId == userId && a.TestSetId == setId)
            .Where(a => a.Status == AttemptStatus.InProgress || a.Status == AttemptStatus.Paused)
            .OrderByDescending(a => a.LastActivityAt)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// The published version of each question in the set, in set order (database.md: test_set_items
    /// are "resolved to current version at attempt start"). Retired questions are left out.
    /// </summary>
    private Task<List<Guid>> CurrentVersionsAsync(Guid setId, CancellationToken cancellationToken) =>
        (from item in db.TestSetItems
         join question in db.Questions on item.QuestionId equals question.Id
         where item.TestSetId == setId
               && question.Status == QuestionStatus.Active
               && question.CurrentVersionId != null
         orderby item.Position
         select question.CurrentVersionId!.Value)
        .ToListAsync(cancellationToken);
}
