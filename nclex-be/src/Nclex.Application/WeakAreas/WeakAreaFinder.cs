using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;
using Nclex.Domain.WeakAreas;

namespace Nclex.Application.WeakAreas;

/// <summary>A weak Client Needs leaf, named, as the API returns it.</summary>
/// <param name="ClientNeedsId">The leaf's id, as the custom builder takes it.</param>
/// <param name="Code">The leaf's code, e.g. "PHI.PPT".</param>
/// <param name="Name">The leaf's name, e.g. "Pharmacological and Parenteral Therapies".</param>
/// <param name="Pct">Points earned as a share of the maximum across the recent attempts, 0 to 1.</param>
/// <param name="Items">Items served in the leaf across the recent attempts.</param>
public sealed record WeakAreaView(short ClientNeedsId, string Code, string Name, decimal Pct, int Items);

/// <summary>The weak areas and how many attempts they were worked out from.</summary>
public sealed record FoundWeakAreas(IReadOnlyList<WeakAreaView> Areas, int AttemptsConsidered);

/// <summary>
/// Works out a user's weak areas from their latest submitted attempts (architecture.md 5.7.1). It
/// reads the per-attempt breakdown stored at submit, not the all-time snapshots, so it sees exactly
/// the window it needs and never lags behind the snapshot refresh job.
/// </summary>
public sealed class WeakAreaFinder(IAppDbContext db)
{
    public async Task<FoundWeakAreas> FindAsync(Guid userId, ExamTrack track, CancellationToken cancellationToken)
    {
        // Served by attempts_history_idx (user_id, submitted_at DESC) WHERE status = 'submitted'.
        var recentAttemptIds = await db.Attempts.AsNoTracking()
            .Where(a => a.UserId == userId && a.Track == track && a.Status == AttemptStatus.Submitted)
            .OrderByDescending(a => a.SubmittedAt)
            .Take(WeakAreaRules.RecentAttempts)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);
        if (recentAttemptIds.Count == 0)
        {
            return new FoundWeakAreas([], 0);
        }

        var scores = await db.AttemptCategoryScores.AsNoTracking()
            .Where(s => recentAttemptIds.Contains(s.AttemptId) && s.Dimension == ScoreDimension.ClientNeeds)
            .Select(s => new LeafScore(s.DimensionKey, s.Items, s.PointsEarned, s.MaxPoints))
            .ToListAsync(cancellationToken);

        var leaves = await db.ClientNeedsCategories.AsNoTracking()
            .Where(c => c.Track == track)
            .ToDictionaryAsync(c => c.Code, StringComparer.Ordinal, cancellationToken);

        // A code no longer in the taxonomy can't be practised, so it is left out.
        IReadOnlyList<WeakAreaView> areas = [.. WeakAreaRules.From(scores)
            .Where(a => leaves.ContainsKey(a.Code))
            .Select(a => new WeakAreaView(leaves[a.Code].Id, a.Code, leaves[a.Code].Name, a.Pct, a.Items))];
        return new FoundWeakAreas(areas, recentAttemptIds.Count);
    }
}
