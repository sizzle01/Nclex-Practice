using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts;

/// <summary>Builds the <see cref="AttemptState"/> response from the database.</summary>
public sealed class AttemptStateReader(IAppDbContext db, TimeProvider clock)
{
    /// <summary>
    /// Reads the items afresh, so the state includes changes saved earlier in the same transaction.
    /// </summary>
    public async Task<AttemptState> ReadAsync(Attempt attempt, CancellationToken cancellationToken)
    {
        var title = await db.TestSets.AsNoTracking()
            .Where(s => s.Id == attempt.TestSetId)
            .Select(s => s.Title)
            .SingleAsync(cancellationToken);
        var items = await db.AttemptItems.AsNoTracking()
            .Where(i => i.AttemptId == attempt.Id)
            .OrderBy(i => i.Position)
            .Select(i => new { i.Position, i.State, i.Flagged, HasResponse = i.Response != null })
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow();
        var settings = attempt.Settings;
        return new AttemptState(
            attempt.Id,
            new TestSetRef(attempt.TestSetId, title),
            attempt.Status,
            new AttemptStateSettings(
                settings.Mode,
                settings.RevealMode,
                settings.AllowBackNav,
                settings.PerQuestionSec,
                settings.TimeBudgetSec,
                settings.PassThreshold),
            attempt.TotalItems,
            attempt.CurrentPosition,
            new AttemptProgress(
                Answered: items.Count(i => i.HasResponse && i.State is ItemState.Answered or ItemState.Locked),
                Locked: items.Count(i => i.State == ItemState.Locked),
                Flagged: items.Count(i => i.Flagged)),
            [.. items.Select(i => new NavigatorEntry(i.Position, i.State, i.Flagged))],
            TimerState.For(attempt, now),
            attempt.StartedAt,
            attempt.LastActivityAt,
            attempt.IsStaleDraftAt(now),
            ResultAvailable: attempt.Status == AttemptStatus.Submitted);
    }
}
