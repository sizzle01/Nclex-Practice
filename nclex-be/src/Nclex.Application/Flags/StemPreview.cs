namespace Nclex.Application.Flags;

/// <summary>The start of a question's stem, so a list of reports shows which question each is about.</summary>
public static class StemPreview
{
    public const int MaxLength = 160;

    public static string From(string stem)
    {
        var text = stem.Trim();
        return text.Length <= MaxLength ? text : string.Concat(text.AsSpan(0, MaxLength - 1).TrimEnd(), "…");
    }
}
