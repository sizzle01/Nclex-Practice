namespace Nclex.Domain.Performance;

/// <summary>
/// A way of slicing a user's performance across attempts (the <c>dimension</c> column of
/// <c>performance_snapshots</c>, database.md section 2.7). The same slices as a result's breakdown,
/// plus <see cref="Overall"/> for the totals on the dashboard.
/// </summary>
public enum PerformanceDimension
{
    /// <summary>Every item of every attempt; its one key is <see cref="PerformanceDimensions.OverallKey"/>.</summary>
    Overall,
    ClientNeeds,
    ClientNeedsParent,
    BodySystem,
    ItemType,
    CjmmSkill,
    Difficulty,
}
