namespace Nclex.Domain.Grading;

/// <summary>Rounding rules shared by every score on the results page.</summary>
public static class ScoreMaths
{
    /// <summary>
    /// <paramref name="earned"/> as a share of <paramref name="max"/>, 0 to 1, rounded to the 4 places
    /// that <c>score_pct numeric(5,4)</c> holds. Partial credit counts (PRD 5.6), so this is by points,
    /// not by items correct.
    /// </summary>
    public static decimal Share(decimal earned, decimal max) =>
        max <= 0 ? 0 : Math.Round(earned / max, 4, MidpointRounding.AwayFromZero);
}
