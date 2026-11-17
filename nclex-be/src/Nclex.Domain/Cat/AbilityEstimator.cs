namespace Nclex.Domain.Cat;

/// <summary>An ability estimate and its standard error, both in logits.</summary>
public sealed record AbilityEstimate(double Theta, double StandardError)
{
    /// <summary>z for a two-sided 95% confidence interval.</summary>
    public const double Z95 = 1.96;

    /// <summary>Before any answer: the prior, a standard normal.</summary>
    public static AbilityEstimate Prior { get; } = new(0.0, 1.0);

    /// <summary>Lower end of the 95% confidence interval.</summary>
    public double Lower95 => Theta - (Z95 * StandardError);

    /// <summary>Upper end of the 95% confidence interval.</summary>
    public double Upper95 => Theta + (Z95 * StandardError);
}

/// <summary>One answered question, as the estimator sees it.</summary>
/// <param name="Difficulty">The question's difficulty b, in logits.</param>
/// <param name="Score">Share of the question's points earned, 0 to 1.</param>
public sealed record ScoredResponse(double Difficulty, double Score);

/// <summary>
/// Estimates ability from the answers so far with the expected a posteriori (EAP) method: the mean
/// of the posterior over a grid of θ values, with a standard normal prior. Unlike maximum
/// likelihood it gives a finite answer even when every answer is right (or wrong), which happens
/// early in every exam.
/// </summary>
public static class AbilityEstimator
{
    private const double GridMin = -4.0;
    private const double GridMax = 4.0;
    private const double GridStep = 0.05;

    private static readonly double[] Grid = BuildGrid();

    /// <summary>
    /// The estimate after <paramref name="responses"/>. A partly correct NGN answer counts as a
    /// fractional score, so 2 of 3 points pulls θ up less than a full mark does.
    /// </summary>
    public static AbilityEstimate Estimate(IReadOnlyList<ScoredResponse> responses)
    {
        if (responses.Count == 0)
        {
            return AbilityEstimate.Prior;
        }

        // Work in logs and subtract the largest before exponentiating, so 150 answers never underflow.
        var logPosterior = Grid.Select(theta => LogPrior(theta) + LogLikelihood(theta, responses)).ToArray();
        var peak = logPosterior.Max();
        var weights = logPosterior.Select(l => Math.Exp(l - peak)).ToArray();
        var total = weights.Sum();

        var mean = Grid.Select((theta, i) => theta * weights[i]).Sum() / total;
        var variance = Grid.Select((theta, i) => (theta - mean) * (theta - mean) * weights[i]).Sum() / total;
        return new AbilityEstimate(mean, Math.Sqrt(variance));
    }

    private static double LogPrior(double theta) => -0.5 * theta * theta;

    private static double LogLikelihood(double theta, IReadOnlyList<ScoredResponse> responses)
    {
        var sum = 0.0;
        foreach (var response in responses)
        {
            var p = RaschModel.Probability(theta, response.Difficulty);
            var score = Math.Clamp(response.Score, 0.0, 1.0);
            sum += (score * Math.Log(p)) + ((1 - score) * Math.Log(1 - p));
        }

        return sum;
    }

    private static double[] BuildGrid()
    {
        var count = (int)Math.Round((GridMax - GridMin) / GridStep) + 1;
        return [.. Enumerable.Range(0, count).Select(i => GridMin + (i * GridStep))];
    }
}
