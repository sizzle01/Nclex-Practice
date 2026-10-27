namespace Nclex.Domain.ItemTypes.SelectN;

/// <summary>What candidates see: the options and how many to pick, e.g. "Select 3" (interfaces.md 2.3).</summary>
public sealed record SelectNContent(IReadOnlyList<Option> Options, int SelectCount);

/// <summary>The correct options; exactly <see cref="SelectNContent.SelectCount"/> of them.</summary>
/// <param name="Correct">The options to pick.</param>
/// <param name="ScoringOverride">
/// Leave out for the default, one point per correct selection. <c>all_or_nothing</c> scores the item
/// as a single point instead (interfaces.md 2.3 note).
/// </param>
public sealed record SelectNKey(IReadOnlyList<string> Correct, SelectNScoringOverride? ScoringOverride = null);

/// <summary>The options the candidate selected; never more than the select count.</summary>
public sealed record SelectNResponse(IReadOnlyList<string> Selected);

/// <summary>Alternative scoring for a Select N item.</summary>
public enum SelectNScoringOverride
{
    /// <summary>One point only when every correct option is selected.</summary>
    AllOrNothing,
}
