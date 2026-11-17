using Nclex.Domain.Entities;

namespace Nclex.Domain.Cat;

/// <summary>
/// The Rasch (one-parameter logistic) model the adaptive exam uses (architecture.md 5.8): the chance
/// of answering a question correctly depends only on the gap between the candidate's ability (θ)
/// and the question's difficulty (b), both in logits.
/// </summary>
public static class RaschModel
{
    /// <summary>Chance that a candidate of ability <paramref name="theta"/> answers a question of difficulty <paramref name="difficulty"/> correctly.</summary>
    public static double Probability(double theta, double difficulty) => 1.0 / (1.0 + Math.Exp(difficulty - theta));

    /// <summary>
    /// How much a question tells us about a candidate at <paramref name="theta"/>: p(1 − p), largest
    /// when the question's difficulty matches the ability, which is why CAT picks questions near θ.
    /// </summary>
    public static double Information(double theta, double difficulty)
    {
        var p = Probability(theta, difficulty);
        return p * (1 - p);
    }

    /// <summary>
    /// A question's difficulty: its calibrated <c>irt_b</c> when there is one, otherwise the seed
    /// for its author-judged tier (easy −1, moderate 0, difficult +1; architecture.md 5.8).
    /// </summary>
    public static double DifficultyOf(decimal? irtB, DifficultyTier tier) => irtB is { } b
        ? (double)b
        : tier switch
        {
            DifficultyTier.Easy => -1.0,
            DifficultyTier.Difficult => 1.0,
            _ => 0.0,
        };
}
