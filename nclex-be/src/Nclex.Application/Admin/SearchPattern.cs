using System.Text;

namespace Nclex.Application.Admin;

/// <summary>
/// LIKE patterns for the admin search boxes. Used with lower-cased text and columns, so matches
/// ignore case without tying the application layer to Postgres's ILIKE.
/// </summary>
public static class SearchPattern
{
    /// <summary>The escape character passed to EF.Functions.Like with these patterns.</summary>
    public const string Escape = "\\";

    /// <summary>"%text%" with LIKE's own wildcards in <paramref name="text"/> escaped, lower-cased.</summary>
    public static string Containing(string text)
    {
        var pattern = new StringBuilder("%");
        foreach (var c in text.Trim().ToLowerInvariant())
        {
            if (c is '%' or '_' or '\\')
            {
                pattern.Append('\\');
            }

            pattern.Append(c);
        }

        return pattern.Append('%').ToString();
    }
}
