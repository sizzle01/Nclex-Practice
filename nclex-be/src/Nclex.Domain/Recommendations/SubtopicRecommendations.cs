namespace Nclex.Domain.Recommendations;

/// <summary>One answered question's outcome for one of the subtopics it is tagged with.</summary>
/// <param name="IsCorrect">True only for full marks, so partial credit counts as a miss.</param>
public sealed record TaggedAnswer(Guid SubtopicId, string Name, bool IsCorrect);

/// <summary>A subtopic the candidate struggled with in one test.</summary>
public sealed record SubtopicGap(Guid SubtopicId, string Name, int Missed, int Answered);

/// <summary>Works out which subtopics to suggest after a test (docs/features/F17).</summary>
public static class SubtopicRecommendations
{
    public const int MaxCount = 5;

    /// <summary>
    /// The subtopics with at least one miss: most missed first, then the lowest share correct, then
    /// by name so the order never changes between page loads. Callers pass answered questions only,
    /// because running out of time says nothing about what a candidate knows.
    /// </summary>
    public static IReadOnlyList<SubtopicGap> From(IEnumerable<TaggedAnswer> answers) =>
        [.. answers
            .GroupBy(a => a.SubtopicId)
            .Select(group => new SubtopicGap(
                group.Key,
                group.First().Name,
                group.Count(a => !a.IsCorrect),
                group.Count()))
            .Where(gap => gap.Missed > 0)
            .OrderByDescending(gap => gap.Missed)
            .ThenByDescending(gap => (decimal)gap.Missed / gap.Answered)
            .ThenBy(gap => gap.Name, StringComparer.Ordinal)
            .Take(MaxCount)];
}
