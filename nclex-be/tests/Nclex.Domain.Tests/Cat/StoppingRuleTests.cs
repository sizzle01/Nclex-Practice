using Nclex.Domain.Cat;

namespace Nclex.Domain.Tests.Cat;

public sealed class StoppingRuleTests
{
    private static readonly CatConfig Config = CatConfig.NclexRn;

    // 95% interval 0.6 ± 0.39: wholly above the passing standard of 0.
    private static readonly AbilityEstimate ClearPass = new(0.6, 0.2);

    // 95% interval −0.6 ± 0.39: wholly below.
    private static readonly AbilityEstimate ClearFail = new(-0.6, 0.2);

    // 95% interval 0.1 ± 0.39: contains the passing standard.
    private static readonly AbilityEstimate Unclear = new(0.1, 0.2);

    [Fact]
    public void A_clear_result_does_not_stop_the_exam_before_the_minimum()
    {
        StoppingRule.AfterAnswer(Config, ClearPass, answered: 84).ShouldBeNull();
    }

    [Theory]
    [InlineData(0.6)]
    [InlineData(-0.6)]
    public void A_clear_result_stops_the_exam_at_the_minimum(double theta)
    {
        StoppingRule.AfterAnswer(Config, new AbilityEstimate(theta, 0.2), answered: 85).ShouldBe(CatStopReason.ClearResult);
    }

    [Fact]
    public void An_unclear_result_carries_on_past_the_minimum()
    {
        StoppingRule.AfterAnswer(Config, Unclear, answered: 120).ShouldBeNull();
    }

    [Fact]
    public void The_exam_always_stops_at_the_maximum()
    {
        StoppingRule.AfterAnswer(Config, Unclear, answered: 150).ShouldBe(CatStopReason.MaxItems);
    }

    [Fact]
    public void A_clear_pass_and_a_clear_fail_are_decided_by_the_estimate()
    {
        StoppingRule.Passed(Config, ClearPass, answered: 85).ShouldBeTrue();
        StoppingRule.Passed(Config, ClearFail, answered: 85).ShouldBeFalse();
    }

    [Fact]
    public void At_the_maximum_or_out_of_time_the_last_estimate_decides()
    {
        StoppingRule.Passed(Config, new AbilityEstimate(0.05, 0.2), answered: 150).ShouldBeTrue();
        StoppingRule.Passed(Config, new AbilityEstimate(-0.05, 0.2), answered: 110).ShouldBeFalse();
    }

    [Fact]
    public void Running_out_of_time_before_the_minimum_is_a_fail_however_well_it_went()
    {
        StoppingRule.Passed(Config, new AbilityEstimate(2.5, 0.3), answered: 60).ShouldBeFalse();
    }
}
