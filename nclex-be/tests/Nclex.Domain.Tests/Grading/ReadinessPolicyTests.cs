using System.Globalization;
using Nclex.Domain.Entities;
using Nclex.Domain.Grading;

namespace Nclex.Domain.Tests.Grading;

public sealed class ReadinessPolicyTests
{
    private readonly ReadinessPolicy _policy = new();

    // The edges are judged on the whole percentage shown: 85.5% shows as "86%" and passes,
    // 69.5% shows as "70%" and is close to pass.
    [Theory]
    [InlineData("1", true, ReadinessLabel.LikelyReady)]
    [InlineData("0.8600", true, ReadinessLabel.LikelyReady)]
    [InlineData("0.8550", true, ReadinessLabel.LikelyReady)]
    [InlineData("0.8549", false, ReadinessLabel.CloseToPass)]
    [InlineData("0.8500", false, ReadinessLabel.CloseToPass)]
    [InlineData("0.7000", false, ReadinessLabel.CloseToPass)]
    [InlineData("0.6950", false, ReadinessLabel.CloseToPass)]
    [InlineData("0.6949", false, ReadinessLabel.NeedsMorePractice)]
    [InlineData("0", false, ReadinessLabel.NeedsMorePractice)]
    public void Pass_is_86_and_close_to_pass_is_70_to_85(string scorePct, bool passed, ReadinessLabel label)
    {
        _policy.Evaluate(decimal.Parse(scorePct, CultureInfo.InvariantCulture))
            .ShouldBe(new Readiness(passed, label));
    }

    [Fact]
    public void The_pass_mark_stored_on_results_is_86_percent()
    {
        ReadinessPolicy.PassMark.ShouldBe(0.86m);
    }
}
