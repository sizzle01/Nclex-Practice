using System.Text.Json.Serialization;

namespace Nclex.Domain.Cat;

/// <summary>
/// The stopping rules of an adaptive exam (architecture.md 5.8), stored in <c>test_sets.cat_config</c>
/// and copied into the attempt's CAT state when it starts, so a later edit never changes a running exam.
/// </summary>
/// <param name="MinItems">Questions to answer before the exam may stop on a clear result.</param>
/// <param name="MaxItems">The exam always stops here.</param>
/// <param name="PassingTheta">The passing standard on the ability scale (logits).</param>
public sealed record CatConfig(int MinItems, int MaxItems, double PassingTheta)
{
    /// <summary>
    /// The NCLEX-RN rules since April 2023: 85 to 150 questions. Questions are seeded at difficulty
    /// −1, 0 and +1, so a passing standard of 0 means "answers moderate questions reliably".
    /// </summary>
    public static CatConfig NclexRn { get; } = new(85, 150, 0.0);

    /// <summary>True when the minimum is at least 1 and no more than the maximum.</summary>
    // A rule, not data: kept out of the stored JSON.
    [JsonIgnore]
    public bool IsValid => MinItems >= 1 && MaxItems >= MinItems;
}
