using Nclex.Domain.Entities;
using Nclex.Domain.Grading;

namespace Nclex.Domain.Tests.Grading;

public sealed class WeakestAreasTests
{
    private static readonly Guid AttemptId = Guid.NewGuid();

    [Fact]
    public void The_three_lowest_client_needs_categories_come_worst_first()
    {
        var scores = new[]
        {
            Score(ScoreDimension.ClientNeeds, "SECE.MOC", items: 4, earned: 3, max: 4),
            Score(ScoreDimension.ClientNeeds, "SECE.SIC", items: 4, earned: 1, max: 4),
            Score(ScoreDimension.ClientNeeds, "PI", items: 2, earned: 0, max: 2),
            Score(ScoreDimension.ClientNeeds, "HPM", items: 2, earned: 2, max: 2),
            // Other dimensions never appear, however low they are.
            Score(ScoreDimension.BodySystem, "cardiovascular", items: 5, earned: 0, max: 5),
        };

        WeakestAreas.From(scores).Select(s => s.DimensionKey).ShouldBe(["PI", "SECE.SIC", "SECE.MOC"]);
    }

    [Fact]
    public void On_a_tie_the_category_with_more_items_comes_first()
    {
        var scores = new[]
        {
            Score(ScoreDimension.ClientNeeds, "SECE.MOC", items: 2, earned: 1, max: 2),
            Score(ScoreDimension.ClientNeeds, "SECE.SIC", items: 6, earned: 3, max: 6),
        };

        WeakestAreas.From(scores).Select(s => s.DimensionKey).ShouldBe(["SECE.SIC", "SECE.MOC"]);
    }

    private static AttemptCategoryScore Score(ScoreDimension dimension, string key, int items, decimal earned, decimal max) =>
        AttemptCategoryScore.From(AttemptId, new CategoryTally(dimension, key, items, 0, earned, max, 0));
}
