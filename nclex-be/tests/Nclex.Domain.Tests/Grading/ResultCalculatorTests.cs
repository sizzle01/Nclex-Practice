using Nclex.Domain.Entities;
using Nclex.Domain.Grading;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Tests.Grading;

public sealed class ResultCalculatorTests
{
    private static readonly ItemTags Moc = new("SECE.MOC", "SECE", "fundamentals", ItemType.MultipleChoice, CjmmSkill.TakeAction, DifficultyTier.Easy);
    private static readonly ItemTags Sic = new("SECE.SIC", "SECE", "respiratory", ItemType.Sata, null, DifficultyTier.Moderate);
    private static readonly ItemTags Psych = new("PI", "PI", "mental-health", ItemType.MultipleChoice, CjmmSkill.AnalyzeCues, DifficultyTier.Easy);

    [Fact]
    public void Totals_score_by_points_so_partial_credit_counts()
    {
        var items = new[]
        {
            new GradedItem(1, 1, IsCorrect: true, IsAnswered: true, 60_000, Moc),
            new GradedItem(2, 3, IsCorrect: false, IsAnswered: true, 90_000, Sic),
            new GradedItem(0, 1, IsCorrect: false, IsAnswered: false, 30_000, Psych),
        };

        var totals = ResultCalculator.Totals(items);

        totals.ShouldBe(new ResultTotals(
            PointsEarned: 3,
            MaxPoints: 5,
            ScorePct: 0.6m,
            ItemsCorrect: 1,
            ItemsAnswered: 2,
            ItemsTotal: 3,
            TotalTimeMs: 180_000,
            AvgTimeMs: 60_000));
    }

    [Fact]
    public void The_score_is_rounded_to_four_places()
    {
        var items = new[]
        {
            new GradedItem(1, 1, true, true, 0, Moc),
            new GradedItem(1, 1, true, true, 0, Moc),
            new GradedItem(0, 1, false, true, 0, Moc),
        };

        ResultCalculator.Totals(items).ScorePct.ShouldBe(0.6667m);
    }

    [Fact]
    public void A_set_of_blanks_scores_zero_out_of_its_maximum()
    {
        var totals = ResultCalculator.Totals([new GradedItem(0, 3, false, false, 0, Sic)]);

        (totals.PointsEarned, totals.MaxPoints, totals.ScorePct, totals.ItemsAnswered).ShouldBe((0m, 3m, 0m, 0));
    }

    [Fact]
    public void The_breakdown_has_one_row_per_key_of_every_dimension()
    {
        var items = new[]
        {
            new GradedItem(1, 1, true, true, 60_000, Moc),
            new GradedItem(2, 3, false, true, 90_000, Sic),
            new GradedItem(0, 1, false, false, 30_000, Psych),
        };

        var rows = ResultCalculator.Breakdown(items)
            .Select(t => $"{DimensionKeys.For(t.Dimension)}:{t.Key} {t.Items}/{t.ItemsCorrect} {t.PointsEarned}/{t.MaxPoints} {t.TimeMs}")
            .ToList();

        rows.ShouldBe(
        [
            "client_needs:PI 1/0 0/1 30000",
            "client_needs:SECE.MOC 1/1 1/1 60000",
            "client_needs:SECE.SIC 1/0 2/3 90000",
            // A top-level leaf is its own parent; the two SECE subcategories add up.
            "client_needs_parent:PI 1/0 0/1 30000",
            "client_needs_parent:SECE 2/1 3/4 150000",
            "body_system:fundamentals 1/1 1/1 60000",
            "body_system:mental-health 1/0 0/1 30000",
            "body_system:respiratory 1/0 2/3 90000",
            "item_type:multiple_choice 2/1 1/2 90000",
            "item_type:sata 1/0 2/3 90000",
            // The SATA item targets no CJMM step, so it is left out of that dimension.
            "cjmm_skill:analyze_cues 1/0 0/1 30000",
            "cjmm_skill:take_action 1/1 1/1 60000",
            "difficulty:easy 2/1 1/2 90000",
            "difficulty:moderate 1/0 2/3 90000",
        ]);
    }

    [Fact]
    public void Each_dimension_adds_up_to_the_totals()
    {
        var items = new[]
        {
            new GradedItem(1, 1, true, true, 60_000, Moc),
            new GradedItem(2, 3, false, true, 90_000, Sic),
            new GradedItem(0, 1, false, false, 30_000, Psych),
        };
        var totals = ResultCalculator.Totals(items);

        var byDimension = ResultCalculator.Breakdown(items)
            .Where(t => t.Dimension != ScoreDimension.CjmmSkill)
            .GroupBy(t => t.Dimension);

        byDimension.ShouldAllBe(d =>
            d.Sum(t => t.PointsEarned) == totals.PointsEarned
            && d.Sum(t => t.MaxPoints) == totals.MaxPoints
            && d.Sum(t => t.Items) == totals.ItemsTotal
            && d.Sum(t => t.ItemsCorrect) == totals.ItemsCorrect
            && d.Sum(t => t.TimeMs) == totals.TotalTimeMs);
    }

    [Fact]
    public void An_attempt_without_items_is_refused()
    {
        Should.Throw<ArgumentException>(() => ResultCalculator.Totals([]));
    }
}
