namespace Nclex.Domain.Entities;

/// <summary>
/// The test set's settings copied onto an attempt when it starts (the <c>attempts.settings</c> JSON
/// column). A later edit to the set never changes an attempt that is already running.
/// </summary>
/// <param name="Mode">Practice or challenge.</param>
/// <param name="RevealMode">Tutor (answers after each item) or exam (answers at the end).</param>
/// <param name="Timed">Whether any timer runs (F09).</param>
/// <param name="PerQuestionSec">Seconds per item, or null for no item timer.</param>
/// <param name="TimeBudgetSec">Seconds for the whole attempt, or null for no exam timer.</param>
/// <param name="PassThreshold">Share of points for a "likely pass" estimate; null for none.</param>
/// <param name="PausePolicy">Pause limits (F09).</param>
/// <param name="AllowBackNav">Whether the candidate may go back to earlier items.</param>
public sealed record AttemptSettings(
    TestMode Mode,
    RevealMode RevealMode,
    bool Timed,
    int? PerQuestionSec,
    int? TimeBudgetSec,
    decimal? PassThreshold,
    PausePolicy PausePolicy,
    bool AllowBackNav)
{
    // For EF Core: the stored JSON holds PausePolicy as a nested object, which EF cannot pass to
    // the positional constructor, so it builds the record this way and sets the properties.
    private AttemptSettings()
        : this(TestMode.Practice, RevealMode.Tutor, false, null, null, null, PausePolicy.Unlimited, true)
    {
    }

    /// <summary>Copies the settings of <paramref name="set"/>.</summary>
    public static AttemptSettings From(TestSet set) => new(
        set.Mode,
        set.RevealMode,
        set.Timed,
        set.PerQuestionSec,
        set.TimeBudgetSec,
        set.PassThreshold,
        set.PausePolicy,
        set.AllowBackNav);
}
