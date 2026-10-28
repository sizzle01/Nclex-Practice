using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.MyFlags;

/// <summary>
/// One of the signed-in user's reports, as POST /question-versions/{id}/flags and GET /me/flags
/// return it. It shows the stem but never the answer key or rationale.
/// </summary>
/// <param name="ResolutionNote">
/// The moderator's note, only once the report is resolved. A rejected report's note is for
/// moderators, so the reporter sees only the status.
/// </param>
public sealed record MyFlagView(
    Guid Id,
    Guid QuestionVersionId,
    string StemPreview,
    FlagReason Reason,
    string? Comment,
    FlagStatus Status,
    string? ResolutionNote,
    Guid? AttemptId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt)
{
    public static MyFlagView From(QuestionFlag flag, string stem) => new(
        flag.Id,
        flag.QuestionVersionId,
        Flags.StemPreview.From(stem),
        flag.Reason,
        flag.Comment,
        flag.Status,
        flag.Status == FlagStatus.Resolved ? flag.ResolutionNote : null,
        flag.AttemptId,
        flag.CreatedAt,
        flag.ResolvedAt);
}

/// <summary>One page of the user's reports, newest first.</summary>
public sealed record MyFlagPage(IReadOnlyList<MyFlagView> Items, string? NextCursor);
