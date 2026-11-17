using Nclex.Domain.WeakAreas;

namespace Nclex.Domain.Tests.WeakAreas;

public sealed class WeakAreaRulesTests
{
    // One item worth one point each, so points earned = items correct.
    private static LeafScore Score(string code, int items, int correct) => new(code, items, correct, items);

    [Fact]
    public void A_leaf_under_the_threshold_with_enough_items_is_weak()
    {
        var areas = WeakAreaRules.From([Score("PHI.PPT", 10, 6)]);

        areas.ShouldHaveSingleItem().ShouldBe(new WeakArea("PHI.PPT", 10, 0.6000m));
    }

    [Fact]
    public void Exactly_the_threshold_is_not_weak()
    {
        WeakAreaRules.From([Score("PHI.PPT", 20, 13)]).ShouldBeEmpty(); // 65%
    }

    [Fact]
    public void Just_under_the_threshold_is_weak()
    {
        // 649 of 1000 points: 64.9%.
        WeakAreaRules.From([new LeafScore("PHI.PPT", 10, 6.49m, 10m)]).ShouldHaveSingleItem().Pct.ShouldBe(0.6490m);
    }

    [Fact]
    public void Four_items_are_too_few_but_five_are_enough()
    {
        WeakAreaRules.From([Score("PHI.PPT", 4, 0)]).ShouldBeEmpty();
        WeakAreaRules.From([Score("PHI.PPT", 5, 0)]).ShouldHaveSingleItem();
    }

    [Fact]
    public void Attempts_are_added_up_per_leaf()
    {
        // 3 items in each of two attempts: neither alone is enough, together they are.
        var areas = WeakAreaRules.From([Score("SECE.MOC", 3, 1), Score("SECE.MOC", 3, 2)]);

        areas.ShouldHaveSingleItem().ShouldBe(new WeakArea("SECE.MOC", 6, 0.5000m));
    }

    [Fact]
    public void Partial_credit_counts_by_points()
    {
        // 6.25 of 10 points = 62.5%: points from partly right answers count.
        var areas = WeakAreaRules.From([new LeafScore("PHI.RRP", 5, 6.25m, 10m)]);

        areas.ShouldHaveSingleItem().Pct.ShouldBe(0.6250m);
    }

    [Fact]
    public void Worst_first_then_more_items_then_code()
    {
        var areas = WeakAreaRules.From(
        [
            Score("PHI.RRP", 10, 6),  // 60%
            Score("HPM", 10, 3),      // 30%
            Score("PSI", 20, 6),      // 30%, more items
            Score("PHI.BCC", 10, 6),  // 60%, same as RRP: code decides
            Score("SECE.SIC", 10, 9), // strong
        ]);

        areas.Select(a => a.Code).ShouldBe(["PSI", "HPM", "PHI.BCC", "PHI.RRP"]);
    }

    [Fact]
    public void No_attempts_means_no_weak_areas()
    {
        WeakAreaRules.From([]).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 5)]
    [InlineData(18, 18)]
    [InlineData(25, 25)]
    [InlineData(300, 25)]
    public void Test_size_is_the_target_or_what_is_available(int available, int expected)
    {
        WeakAreaPractice.SizeFor(available).ShouldBe(expected);
    }
}
