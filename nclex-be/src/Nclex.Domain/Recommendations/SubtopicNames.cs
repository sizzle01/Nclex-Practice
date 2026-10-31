using System.Text;

namespace Nclex.Domain.Recommendations;

/// <summary>The shape rules for subtopic names and how many a question may carry (docs/features/F17).</summary>
public static class SubtopicNames
{
    public const int MaxNameLength = 120;

    /// <summary>A question needs at least one tag before review, or it can't appear in recommendations.</summary>
    public const int MinPerQuestion = 1;

    /// <summary>More than three tags means the question is not really testing a specific topic.</summary>
    public const int MaxPerQuestion = 3;

    /// <summary>The name as a URL-safe slug: "Pre-op checks" becomes "pre-op-checks".</summary>
    public static string Slug(string name) => string.Join('-', Words(name));

    /// <summary>True for lower-case letters and digits separated by single hyphens.</summary>
    public static bool IsSlug(string value) => value.Length > 0 && Slug(value) == value;

    /// <summary>
    /// What two names have in common when they mean the same thing: case, punctuation and plural
    /// endings removed. A candidate should never see "Heart failure" and "Heart Failures" as two topics.
    /// </summary>
    public static string ComparisonKey(string name) =>
        string.Join(' ', Words(name).Select(word => word.Length > 3 && word.EndsWith('s') ? word[..^1] : word));

    private static IEnumerable<string> Words(string text)
    {
        var word = new StringBuilder();
        foreach (var character in text.ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                word.Append(character);
            }
            else if (word.Length > 0)
            {
                yield return word.ToString();
                word.Clear();
            }
        }

        if (word.Length > 0)
        {
            yield return word.ToString();
        }
    }
}
