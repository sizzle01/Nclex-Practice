namespace Nclex.Application.Content.ImportQuestions;

/// <summary>What an import did. When <see cref="Issues"/> is not empty, nothing was written.</summary>
/// <param name="Created">New questions, published as version 1.</param>
/// <param name="Updated">Questions that changed and got a new published version (or only a new access tier).</param>
/// <param name="Unchanged">Questions whose published version already matched the file.</param>
/// <param name="Issues">Everything wrong with the file.</param>
public sealed record ImportResult(int Created, int Updated, int Unchanged, IReadOnlyList<ImportIssue> Issues)
{
    /// <summary>What happened to the file's question banks.</summary>
    public ImportCounts Banks { get; init; } = ImportCounts.None;

    /// <summary>What happened to the file's test sets.</summary>
    public ImportCounts TestSets { get; init; } = ImportCounts.None;

    /// <summary>What happened to the file's media assets.</summary>
    public ImportCounts Media { get; init; } = ImportCounts.None;

    /// <summary>What happened to the file's case studies.</summary>
    public ImportCounts CaseStudies { get; init; } = ImportCounts.None;

    public bool Succeeded => Issues.Count == 0;

    public static ImportResult Failed(IReadOnlyList<ImportIssue> issues) => new(0, 0, 0, issues);
}

/// <summary>How many banks or sets an import created, changed, or found already up to date.</summary>
public sealed record ImportCounts(int Created, int Updated, int Unchanged)
{
    public static ImportCounts None { get; } = new(0, 0, 0);
}

/// <summary>A problem in an import file.</summary>
/// <param name="Path">Where it is, e.g. "questions[3].rationale.perOption.c".</param>
/// <param name="Code">An item-type <c>ValidationIssueCodes</c> value or one of <see cref="ImportIssueCodes"/>.</param>
/// <param name="Message">A sentence for the content author.</param>
public sealed record ImportIssue(string Path, string Code, string Message);

/// <summary>Issue codes for problems the importer finds itself, as opposed to the item-type modules.</summary>
public static class ImportIssueCodes
{
    public const string InvalidFile = "INVALID_FILE";
    public const string Required = "REQUIRED";
    public const string InvalidValue = "INVALID_VALUE";
    public const string DuplicateId = "DUPLICATE_ID";

    /// <summary>The Client Needs code or body-system slug does not exist (or is not a leaf for the track).</summary>
    public const string UnknownTag = "UNKNOWN_TAG";

    /// <summary>The item type has no module yet, so its items can't be validated or graded.</summary>
    public const string UnsupportedItemType = "UNSUPPORTED_ITEM_TYPE";

    /// <summary>The id belongs to an existing question, bank or set on the other track.</summary>
    public const string TrackMismatch = "TRACK_MISMATCH";

    /// <summary>A bank or set lists a question that is neither in the file nor published on the track.</summary>
    public const string UnknownQuestion = "UNKNOWN_QUESTION";

    /// <summary>A file named in the import file is missing, or its path leaves the import file's folder.</summary>
    public const string FileNotFound = "FILE_NOT_FOUND";

    /// <summary>An exhibit or option uses a mediaId that is neither in the file nor already imported.</summary>
    public const string UnknownMedia = "UNKNOWN_MEDIA";

    /// <summary>A question names a case study that is neither in the file nor imported on the track.</summary>
    public const string UnknownCaseStudy = "UNKNOWN_CASE_STUDY";

    /// <summary>A curated set has only some of a case's questions, or not back to back in case order.</summary>
    public const string CaseStudySplit = "CASE_STUDY_SPLIT";
}
