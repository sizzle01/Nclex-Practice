using Nclex.Domain.Cat;

namespace Nclex.Domain.Tests.Cat;

public sealed class AbilityEstimatorTests
{
    [Fact]
    public void With_no_answers_the_estimate_is_the_prior()
    {
        AbilityEstimator.Estimate([]).ShouldBe(AbilityEstimate.Prior);
    }

    [Fact]
    public void Right_answers_raise_the_estimate_and_wrong_ones_lower_it()
    {
        var right = AbilityEstimator.Estimate(Answers(10, difficulty: 0, score: 1));
        var wrong = AbilityEstimator.Estimate(Answers(10, difficulty: 0, score: 0));

        right.Theta.ShouldBeGreaterThan(0.5);
        wrong.Theta.ShouldBeLessThan(-0.5);
        right.Theta.ShouldBe(-wrong.Theta, tolerance: 1e-9); // the model and prior are symmetric
    }

    [Fact]
    public void Every_answer_right_still_gives_a_finite_estimate()
    {
        // Maximum likelihood has no answer here; the prior keeps EAP inside the grid.
        var estimate = AbilityEstimator.Estimate(Answers(150, difficulty: 1, score: 1));

        double.IsFinite(estimate.Theta).ShouldBeTrue();
        estimate.Theta.ShouldBeInRange(2.0, 4.0);
    }

    [Fact]
    public void More_answers_shrink_the_standard_error()
    {
        var after10 = AbilityEstimator.Estimate(Alternating(10));
        var after100 = AbilityEstimator.Estimate(Alternating(100));

        after100.StandardError.ShouldBeLessThan(after10.StandardError);
        after100.Theta.ShouldBe(0, tolerance: 0.05);
    }

    [Fact]
    public void Partial_credit_counts_between_wrong_and_right()
    {
        var partial = AbilityEstimator.Estimate(Answers(10, difficulty: 0, score: 2.0 / 3));
        var right = AbilityEstimator.Estimate(Answers(10, difficulty: 0, score: 1));

        partial.Theta.ShouldBeInRange(0.0, right.Theta);
    }

    [Fact]
    public void The_95_percent_interval_is_1_96_standard_errors_each_side()
    {
        var estimate = new AbilityEstimate(0.5, 0.2);

        estimate.Lower95.ShouldBe(0.108, tolerance: 1e-9);
        estimate.Upper95.ShouldBe(0.892, tolerance: 1e-9);
    }

    private static List<ScoredResponse> Answers(int count, double difficulty, double score) =>
        [.. Enumerable.Repeat(new ScoredResponse(difficulty, score), count)];

    private static List<ScoredResponse> Alternating(int count) =>
        [.. Enumerable.Range(0, count).Select(i => new ScoredResponse(0, i % 2))];
}
