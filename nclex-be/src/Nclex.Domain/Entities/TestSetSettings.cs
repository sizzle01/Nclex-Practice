namespace Nclex.Domain.Entities;

/// <summary>
/// What a curated set looks like to a candidate: its name, how it is taken and who may take it.
/// Grouped so the importer can compare a file's settings with the stored ones in one step.
/// </summary>
/// <param name="Title">Shown on the set card.</param>
/// <param name="Description">Optional longer text for the detail panel.</param>
/// <param name="Mode">Practice or challenge.</param>
/// <param name="RevealMode">Tutor (answers after each item) or exam (answers at the end).</param>
/// <param name="Timed">Whether any timer runs.</param>
/// <param name="PerQuestionSec">Time per item, or null for no item timer.</param>
/// <param name="TimeBudgetSec">Time for the whole attempt, or null for no exam timer.</param>
/// <param name="PassThreshold">Share of points needed for a "likely pass" estimate, e.g. 0.65; null for none.</param>
/// <param name="PausePolicy">Pause limits.</param>
/// <param name="AllowBackNav">Whether candidates may go back to earlier items.</param>
/// <param name="IsSample">Sample sets can be started before verifying an email (docs/README.md assumption 5).</param>
/// <param name="AccessTier">Premium sets are locked for free accounts.</param>
public sealed record TestSetSettings(
    string Title,
    string? Description,
    TestMode Mode,
    RevealMode RevealMode,
    bool Timed,
    int? PerQuestionSec,
    int? TimeBudgetSec,
    decimal? PassThreshold,
    PausePolicy PausePolicy,
    bool AllowBackNav,
    bool IsSample,
    PlanTier AccessTier)
{
    /// <summary>
    /// The database CHECK "NOT timed OR per_question_sec IS NOT NULL OR time_budget_sec IS NOT NULL":
    /// a timed set needs at least one timer. Untimed sets must not carry timers either, so the
    /// card never shows a time for a set that has none.
    /// </summary>
    public bool HasConsistentTiming =>
        Timed
            ? PerQuestionSec is not null || TimeBudgetSec is not null
            : PerQuestionSec is null && TimeBudgetSec is null;
}
