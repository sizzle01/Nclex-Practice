using Nclex.Application.Attempts.Results;
using Nclex.Domain.Entities;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance;

/// <summary>One category of the performance page's breakdown, across every attempt.</summary>
/// <param name="Key">A Client Needs code, body-system slug, item type...</param>
/// <param name="Name">The name to show, e.g. "Management of Care".</param>
/// <param name="ItemsAnswered">Items served in this category, answered or not.</param>
/// <param name="Accuracy">Points earned as a share of the maximum, 0 to 1.</param>
/// <param name="AttemptsCount">How many attempts had items in this category.</param>
public sealed record BreakdownRow(
    string Key,
    string Name,
    int ItemsAnswered,
    int ItemsCorrect,
    decimal Accuracy,
    int AvgTimeMs,
    int AttemptsCount)
{
    public static BreakdownRow From(PerformanceSnapshot snapshot, DimensionNameLookup names) => new(
        snapshot.DimensionKey,
        PerformanceDimensions.ToScoreDimension(snapshot.Dimension) is { } dimension
            ? names.Name(dimension, snapshot.DimensionKey)
            : snapshot.DimensionKey,
        snapshot.ItemsAnswered,
        snapshot.ItemsCorrect,
        snapshot.Accuracy,
        snapshot.AvgTimeMs,
        snapshot.AttemptsCount);
}
