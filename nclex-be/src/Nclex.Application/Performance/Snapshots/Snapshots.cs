using System.Linq.Expressions;
using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.Snapshots;

/// <summary>Filters for <c>performance_snapshots</c> shared by the job and the read endpoints.</summary>
internal static class Snapshots
{
    /// <summary>The user's overall row in <paramref name="track"/>: their totals across every attempt.</summary>
    public static Expression<Func<PerformanceSnapshot, bool>> Overall(Guid userId, ExamTrack track) =>
        s => s.UserId == userId
            && s.Track == track
            && s.Dimension == PerformanceDimension.Overall
            && s.DimensionKey == PerformanceDimensions.OverallKey;
}
