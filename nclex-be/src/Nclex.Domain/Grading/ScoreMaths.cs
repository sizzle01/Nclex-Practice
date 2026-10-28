namespace Nclex.Domain.Grading;

/// <summary>
/// Rounding rules shared by every score on the results page. Values come out with the same number
/// of decimal places as their database columns, so a score just computed and the same score read
/// back later serialise identically (a retried Submit returns exactly the same body).
/// </summary>
public static class ScoreMaths
{
    /// <summary>
    /// <paramref name="earned"/> as a share of <paramref name="max"/>, 0 to 1, to the 4 places that
    /// <c>numeric(5,4)</c> holds. Partial credit counts (PRD 5.6), so this is by points, not by items
    /// correct.
    /// </summary>
    public static decimal Share(decimal earned, decimal max) =>
        Share(max <= 0 ? 0 : earned / max);

    /// <summary>A share or threshold, 0 to 1, to the 4 places that <c>numeric(5,4)</c> holds.</summary>
    public static decimal Share(decimal value) =>
        // Adding 0.0000m sets the scale to 4 places: 0.4 becomes 0.4000, as Postgres returns it.
        Math.Round(value, 4, MidpointRounding.AwayFromZero) + 0.0000m;

    /// <summary>Points, to the 2 places that the points columns (<c>numeric(_,2)</c>) hold.</summary>
    public static decimal Points(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero) + 0.00m;
}
