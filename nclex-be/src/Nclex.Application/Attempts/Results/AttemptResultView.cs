using System.Text.Json.Serialization;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Results;

/// <summary>Body of GET /attempts/{id}/result (api.md section 4).</summary>
/// <param name="Weakest">The weakest Client Needs categories, worst first.</param>
public sealed record AttemptResultView(
    Guid AttemptId,
    string Title,
    DateTimeOffset SubmittedAt,
    SubmitReason SubmitReason,
    ResultScore Score,
    ResultTime Time,
    ResultReadiness? Readiness,
    ResultBreakdown Breakdown,
    IReadOnlyList<WeakArea> Weakest);

/// <summary>
/// The score by each dimension, each list in key order. The JSON names are the dimension names
/// of api.md, which are snake_case rather than the usual camelCase.
/// </summary>
public sealed record ResultBreakdown(
    [property: JsonPropertyName("client_needs")] IReadOnlyList<CategoryResult> ClientNeeds,
    [property: JsonPropertyName("client_needs_parent")] IReadOnlyList<CategoryResult> ClientNeedsParent,
    [property: JsonPropertyName("body_system")] IReadOnlyList<CategoryResult> BodySystem,
    [property: JsonPropertyName("item_type")] IReadOnlyList<CategoryResult> ItemType,
    [property: JsonPropertyName("cjmm_skill")] IReadOnlyList<CategoryResult> CjmmSkill,
    [property: JsonPropertyName("difficulty")] IReadOnlyList<CategoryResult> Difficulty);

/// <summary>One row of a breakdown table.</summary>
/// <param name="Key">The code, slug or snake_case value stored for the dimension.</param>
/// <param name="Correct">Items that earned full marks.</param>
/// <param name="Pct">Points earned as a share of the maximum, 0 to 1.</param>
public sealed record CategoryResult(
    string Key,
    string Name,
    int Items,
    int Correct,
    decimal PointsEarned,
    decimal MaxPoints,
    decimal Pct,
    int AvgTimeMs);

/// <summary>A category to focus on next.</summary>
public sealed record WeakArea(ScoreDimension Dimension, string Key, string Name, decimal Pct);
