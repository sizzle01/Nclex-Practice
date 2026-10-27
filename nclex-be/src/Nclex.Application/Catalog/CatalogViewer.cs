using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Access;
using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog;

/// <summary>The signed-in user as the catalog sees them: which track they study and which plan they are on.</summary>
/// <param name="UserId">The user's id.</param>
/// <param name="Track">Used when a request does not name a track.</param>
/// <param name="Plan">Decides which sets and banks are locked.</param>
public sealed record CatalogViewer(Guid UserId, ExamTrack Track, PlanTier Plan)
{
    /// <summary>True when this user's plan does not cover content of <paramref name="contentTier"/>.</summary>
    public bool IsLocked(PlanTier contentTier) => !PlanAccess.Allows(Plan, contentTier);
}

/// <summary>
/// Loads the <see cref="CatalogViewer"/> and guards premium content (architecture.md section 6,
/// "Tier guard for premium sets and banks"). The catalog only shows a set as locked; starting an
/// attempt (F08) calls <see cref="EnsureAllowed"/> so a locked set cannot be used.
/// </summary>
public sealed class PlanTierGuard(IAppDbContext db, ICurrentUser currentUser)
{
    /// <summary>
    /// Reads the plan from the database rather than the access token, so an upgrade takes effect at
    /// once instead of when the token is next refreshed (the same reasoning as the verified-email check).
    /// </summary>
    public async Task<CatalogViewer> LoadViewerAsync(CancellationToken cancellationToken) =>
        await db.Users.AsNoTracking()
            .Where(u => u.Id == currentUser.UserId && u.DeletedAt == null)
            .Select(u => new CatalogViewer(u.Id, u.Track, u.PlanTier))
            .SingleOrDefaultAsync(cancellationToken)
        // A valid token for a deleted account: treat the caller as signed out, as GET /me does.
        ?? throw new AppException(CommonErrors.Unauthenticated);

    /// <summary>Stops with 403 PLAN_UPGRADE_REQUIRED when the viewer's plan does not cover the content.</summary>
    public static void EnsureAllowed(CatalogViewer viewer, PlanTier contentTier)
    {
        if (viewer.IsLocked(contentTier))
        {
            throw new AppException(CatalogErrors.PlanUpgradeRequired);
        }
    }
}
