using Nclex.Domain.Entities;
using Nclex.Domain.Grading;

namespace Nclex.Domain.Tests.Grading;

public sealed class ReadinessPolicyTests
{
    private readonly ReadinessPolicy _policy = new();

    [Theory]
    [InlineData("0.6500", true, ReadinessLabel.LikelyReady)]
    [InlineData("0.9000", true, ReadinessLabel.LikelyReady)]
    [InlineData("0.6499", false, ReadinessLabel.NeedsMorePractice)]
    [InlineData("0", false, ReadinessLabel.NeedsMorePractice)]
    public void Reaching_the_threshold_is_likely_ready(string scorePct, bool passed, ReadinessLabel label)
    {
        _policy.Evaluate(decimal.Parse(scorePct, System.Globalization.CultureInfo.InvariantCulture), 0.65m)
            .ShouldBe(new Readiness(passed, label));
    }

    [Fact]
    public void A_set_without_a_threshold_has_no_estimate()
    {
        _policy.Evaluate(0.9m, null).ShouldBe(Readiness.None);
    }
}
