using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Grading;

/// <summary>
/// The text stored for dimensions and enum-valued keys: snake_case, the same names the API and the
/// database enums use (e.g. "client_needs", "multiple_choice", "take_action").
/// </summary>
public static class DimensionKeys
{
    public static string For(ScoreDimension dimension) => Snake(dimension.ToString());

    public static string For(ItemType itemType) => Snake(itemType.ToString());

    public static string For(CjmmSkill skill) => Snake(skill.ToString());

    public static string For(DifficultyTier difficulty) => Snake(difficulty.ToString());

    /// <summary>The dimension stored as <paramref name="text"/>.</summary>
    public static ScoreDimension ParseDimension(string text) =>
        Enum.GetValues<ScoreDimension>().Single(d => For(d) == text);

    private static string Snake(string name) => JsonNamingPolicy.SnakeCaseLower.ConvertName(name);
}
