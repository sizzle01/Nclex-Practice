using System.Diagnostics.CodeAnalysis;

namespace Nclex.Domain.ItemTypes;

/// <summary>A selectable choice in an item (interfaces.md section 1).</summary>
/// <param name="Id">Short and stable within a version, e.g. "a". Responses, keys and rationales refer to it.</param>
/// <param name="Text">Sanitised Markdown.</param>
/// <param name="MediaId">An image shown as or beside the option (graphic-option items).</param>
[SuppressMessage("Naming", "CA1716", Justification = "The name comes from interfaces.md section 1; the domain has no Visual Basic consumers.")]
public sealed record Option(string Id, string Text, string? MediaId = null);

/// <summary>
/// The explanation shown after the reveal (interfaces.md section 1). Hidden from candidates until then.
/// </summary>
/// <param name="Correct">Why the correct answer(s) are correct.</param>
/// <param name="PerOption">A "why" for every selectable id, keyed by id (PRD 5.1). Item types check it covers them all.</param>
/// <param name="References">Sources, e.g. a guideline or textbook.</param>
/// <param name="MediaIds">Diagrams shown with the explanation.</param>
public sealed record Rationale(
    string Correct,
    IReadOnlyDictionary<string, string> PerOption,
    IReadOnlyList<string>? References = null,
    IReadOnlyList<string>? MediaIds = null);

/// <summary>
/// How one part of a response contributed to the score: an option, row, blank... (interfaces.md section 1).
/// The results page uses these to show partial credit.
/// </summary>
/// <param name="Target">The option, row, blank or group id.</param>
/// <param name="Delta">+1, 0 or -1.</param>
/// <param name="Selected">What the candidate chose for this target, if anything.</param>
/// <param name="Expected">The correct choice for this target, where that is not implied by the target itself.</param>
/// <param name="Missed">A correct choice the candidate did not select (delta 0).</param>
public sealed record GradeDetail(string Target, int Delta, object? Selected = null, object? Expected = null, bool Missed = false);

/// <summary>The score of one response (interfaces.md section 1).</summary>
public sealed record GradeResult(decimal Points, decimal MaxPoints, bool IsCorrect, IReadOnlyList<GradeDetail> Detail)
{
    /// <summary>An item is correct only when it earns full marks.</summary>
    public static GradeResult From(decimal points, decimal maxPoints, IReadOnlyList<GradeDetail> detail) =>
        new(points, maxPoints, points == maxPoints, detail);
}

/// <summary>A problem found in an authored item or a response.</summary>
/// <param name="Path">Where the problem is, e.g. "content.options[2].id" or "rationale.perOption.c".</param>
/// <param name="Code">One of <see cref="ValidationIssueCodes"/>.</param>
/// <param name="Message">A sentence for the author.</param>
public sealed record ValidationIssue(string Path, string Code, string Message);

/// <summary>Machine-readable codes for <see cref="ValidationIssue"/>.</summary>
public static class ValidationIssueCodes
{
    /// <summary>The JSON does not have the shape the item type expects.</summary>
    public const string InvalidShape = "INVALID_SHAPE";
    public const string Required = "REQUIRED";
    public const string DuplicateId = "DUPLICATE_ID";

    /// <summary>A key, response or rationale refers to an id that is not in the content.</summary>
    public const string UnknownId = "UNKNOWN_ID";

    /// <summary>A selectable id has no entry in <c>rationale.perOption</c>.</summary>
    public const string MissingRationale = "MISSING_RATIONALE";
    public const string TooFew = "TOO_FEW";
    public const string TooMany = "TOO_MANY";
}
