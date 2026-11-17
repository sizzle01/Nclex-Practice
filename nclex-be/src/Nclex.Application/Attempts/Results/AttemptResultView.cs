using System.Text.Json.Serialization;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Results;

/// <summary>Body of GET /attempts/{id}/result (api.md section 4).</summary>
/// <param name="Weakest">The weakest Client Needs categories, worst first.</param>
/// <param name="Adaptive">How an adaptive exam ended (F19); null for every other attempt.</param>
public sealed record AttemptResultView(
    Guid AttemptId,
    string Title,
    DateTimeOffset SubmittedAt,
    SubmitReason SubmitReason,
    ResultScore Score,
    ResultTime Time,
    ResultReadiness? Readiness,
    ResultBreakdown Breakdown,
    IReadOnlyList<WeakArea> Weakest,
    AdaptiveResult? Adaptive);

/// <summary>
/// The outcome of an adaptive exam (architecture.md 5.8). As on the NCLEX it is pass or fail from
/// the ability estimate: the share of points means little when every question is pitched at the
/// candidate's level, so the results page leads with this instead.
/// </summary>
/// <param name="Passed">Pass or fail.</param>
/// <param name="ItemsAnswered">Confirmed answers, the ones the decision was made on.</param>
/// <param name="StopReason">Why the exam ended.</param>
/// <param name="Timed">Whether the candidate chose the timed exam.</param>
/// <param name="Disclaimer">Shown beside the result: this is practice, not the NCLEX.</param>
public sealed record AdaptiveResult(
    bool Passed,
    int ItemsAnswered,
    int MinItems,
    int MaxItems,
    Nclex.Domain.Cat.CatStopReason StopReason,
    bool Timed,
    string Disclaimer)
{
    public const string PracticeDisclaimer =
        "This adaptive practice exam follows the NCLEX's rules, but its questions are not calibrated on real candidates, so treat the result as an estimate.";

    public static AdaptiveResult? For(Attempt attempt)
    {
        if (!attempt.IsAdaptive)
        {
            return null;
        }

        var state = attempt.ReadCatState();
        // Time running out is the one ending the stop rule never records (the timer finalizes).
        var reason = attempt.SubmitReason == SubmitReason.TimeExpired
            ? Nclex.Domain.Cat.CatStopReason.TimeExpired
            : state.StopReason ?? Nclex.Domain.Cat.CatStopReason.MaxItems;
        return new AdaptiveResult(
            state.Passed,
            state.AnsweredCount,
            state.Config.MinItems,
            state.Config.MaxItems,
            reason,
            attempt.Settings.Timed,
            PracticeDisclaimer);
    }
}

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
