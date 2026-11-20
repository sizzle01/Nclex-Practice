using Nclex.Domain.Entities;
using Nclex.Domain.Grading;

namespace Nclex.Domain.Tests.Grading;

public sealed class ClinicalJudgmentTests
{
    private static ServedItem Item(int position, decimal earned, CjmmSkill? skill, long timeMs = 60_000, decimal max = 1) =>
        new(position, earned, max, timeMs, skill);

    [Fact]
    public void Steps_come_in_model_order_whatever_order_the_items_were_served()
    {
        var items = new[]
        {
            Item(1, 1, CjmmSkill.EvaluateOutcomes),
            Item(2, 0, CjmmSkill.RecognizeCues),
            Item(3, 1, CjmmSkill.TakeAction),
            Item(4, 1, CjmmSkill.RecognizeCues),
        };

        var result = ClinicalJudgment.For(items)!;

        result.Steps.Select(s => s.Skill).ShouldBe([CjmmSkill.RecognizeCues, CjmmSkill.TakeAction, CjmmSkill.EvaluateOutcomes]);
        result.Steps[0].ShouldBe(new StepScore(CjmmSkill.RecognizeCues, 2, 1, 2, 0.5m, 60_000));
    }

    [Fact]
    public void Partial_credit_counts_in_a_step_score()
    {
        var items = new[] { Item(1, 2, CjmmSkill.GenerateSolutions, max: 3), Item(2, 1, CjmmSkill.GenerateSolutions, max: 1) };

        var step = ClinicalJudgment.For(items)!.Steps.ShouldHaveSingleItem();

        step.ScorePct.ShouldBe(0.75m);
    }

    [Fact]
    public void The_first_half_takes_the_odd_item_and_halves_follow_the_served_order()
    {
        var items = new[]
        {
            Item(3, 0, CjmmSkill.TakeAction, timeMs: 90_000),
            Item(1, 1, CjmmSkill.RecognizeCues, timeMs: 30_000),
            Item(2, 1, CjmmSkill.AnalyzeCues, timeMs: 30_000),
        };

        var result = ClinicalJudgment.For(items)!;

        (result.FirstHalf.Items, result.FirstHalf.ScorePct, result.FirstHalf.AvgTimeMs).ShouldBe((2, 1m, 30_000));
        (result.SecondHalf.Items, result.SecondHalf.ScorePct, result.SecondHalf.AvgTimeMs).ShouldBe((1, 0m, 90_000));
        result.SecondHalf.Steps.ShouldHaveSingleItem().Skill.ShouldBe(CjmmSkill.TakeAction);
    }

    [Fact]
    public void Items_without_a_step_count_in_the_halves_but_not_in_the_steps()
    {
        var items = new[] { Item(1, 1, null), Item(2, 0, CjmmSkill.TakeAction) };

        var result = ClinicalJudgment.For(items)!;

        result.FirstHalf.Items.ShouldBe(1);
        result.FirstHalf.Steps.ShouldBeEmpty();
        result.Steps.ShouldHaveSingleItem().Skill.ShouldBe(CjmmSkill.TakeAction);
    }

    [Fact]
    public void An_attempt_of_one_item_cannot_be_split()
    {
        ClinicalJudgment.For([Item(1, 1, CjmmSkill.TakeAction)]).ShouldBeNull();
    }

    public static TheoryData<int, int, int, bool> Drops => new()
    {
        // items per half, correct in the first half, correct in the second half, clear drop?
        { 10, 9, 8, true },     // 90% to 80%: exactly 10 points is a clear drop
        { 10, 9, 7, true },     // 90% to 70%
        { 20, 18, 16, true },   // 90% to 80% on a longer attempt
        { 20, 18, 17, false },  // 90% to 85%
        { 9, 9, 0, false },     // too few items in each half to judge
        { 20, 12, 18, false },  // the second half did better
    };

    [Theory]
    [MemberData(nameof(Drops))]
    public void A_clear_drop_needs_ten_items_a_half_and_ten_points_lower(int perHalf, int firstCorrect, int secondCorrect, bool drop)
    {
        var items = Enumerable.Range(1, perHalf)
            .Select(p => Item(p, p <= firstCorrect ? 1 : 0, CjmmSkill.TakeAction))
            .Concat(Enumerable.Range(1, perHalf).Select(p => Item(perHalf + p, p <= secondCorrect ? 1 : 0, CjmmSkill.TakeAction)))
            .ToList();

        ClinicalJudgment.For(items)!.SecondHalfDrop.ShouldBe(drop);
    }
}
