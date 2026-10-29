using System.Text.Json;
using Nclex.Domain.Entities;

namespace Nclex.Domain.Performance;

/// <summary>
/// The text stored and sent for a <see cref="PerformanceDimension"/>: snake_case, the same names
/// <c>attempt_category_scores</c> uses (e.g. "client_needs"), plus "overall".
/// </summary>
public static class PerformanceDimensions
{
    /// <summary>The only key of the <see cref="PerformanceDimension.Overall"/> dimension.</summary>
    public const string OverallKey = "all";

    public static string Name(PerformanceDimension dimension) =>
        JsonNamingPolicy.SnakeCaseLower.ConvertName(dimension.ToString());

    /// <summary>False when <paramref name="text"/> names no dimension.</summary>
    public static bool TryParse(string? text, out PerformanceDimension dimension)
    {
        foreach (var candidate in Enum.GetValues<PerformanceDimension>())
        {
            if (Name(candidate) == text)
            {
                dimension = candidate;
                return true;
            }
        }

        dimension = default;
        return false;
    }

    /// <summary>The dimension of the same name as a result's breakdown <paramref name="dimension"/>.</summary>
    public static PerformanceDimension From(ScoreDimension dimension) => dimension switch
    {
        ScoreDimension.ClientNeeds => PerformanceDimension.ClientNeeds,
        ScoreDimension.ClientNeedsParent => PerformanceDimension.ClientNeedsParent,
        ScoreDimension.BodySystem => PerformanceDimension.BodySystem,
        ScoreDimension.ItemType => PerformanceDimension.ItemType,
        ScoreDimension.CjmmSkill => PerformanceDimension.CjmmSkill,
        ScoreDimension.Difficulty => PerformanceDimension.Difficulty,
        _ => throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Unknown score dimension."),
    };

    /// <summary>The breakdown dimension of the same name; null for <see cref="PerformanceDimension.Overall"/>.</summary>
    public static ScoreDimension? ToScoreDimension(PerformanceDimension dimension) => dimension switch
    {
        PerformanceDimension.Overall => null,
        PerformanceDimension.ClientNeeds => ScoreDimension.ClientNeeds,
        PerformanceDimension.ClientNeedsParent => ScoreDimension.ClientNeedsParent,
        PerformanceDimension.BodySystem => ScoreDimension.BodySystem,
        PerformanceDimension.ItemType => ScoreDimension.ItemType,
        PerformanceDimension.CjmmSkill => ScoreDimension.CjmmSkill,
        PerformanceDimension.Difficulty => ScoreDimension.Difficulty,
        _ => throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Unknown performance dimension."),
    };
}
