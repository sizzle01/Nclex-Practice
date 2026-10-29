using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance;

/// <summary>Who is asking for their performance, with the track and time zone their charts default to.</summary>
public sealed record PerformanceViewer(Guid UserId, ExamTrack Track, TimeZoneInfo TimeZone);

/// <summary>Loads the signed-in user as a <see cref="PerformanceViewer"/>.</summary>
public sealed class PerformanceViewers(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<PerformanceViewer> CurrentAsync(CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking()
            .Where(u => u.Id == currentUser.UserId && u.DeletedAt == null)
            .Select(u => new { u.Id, u.Track, u.Timezone })
            .SingleOrDefaultAsync(cancellationToken)
            // A valid token for a deleted account: treat the caller as signed out, as GET /me does.
            ?? throw new AppException(CommonErrors.Unauthenticated);
        return new PerformanceViewer(user.Id, user.Track, TrendBucketing.ZoneOrUtc(user.Timezone));
    }
}
