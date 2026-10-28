using Nclex.Application.Common;

namespace Nclex.Application.Flags;

/// <summary>Errors returned by the content-feedback endpoints (api.md sections 1, 7 and 8).</summary>
public static class FlagErrors
{
    /// <summary>The question version does not exist, or candidates never see it (a draft, for example).</summary>
    public static readonly AppError VersionNotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That question was not found.");

    /// <summary>The attempt does not exist or is someone else's; both look the same, as in the attempt engine.</summary>
    public static readonly AppError AttemptNotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That attempt was not found.");

    public static readonly AppError FlagNotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That report was not found.");

    /// <summary>The user already reported this version for this reason (api.md section 7).</summary>
    public static readonly AppError Duplicate = new(
        "FLAG_DUPLICATE",
        AppErrorKind.Conflict,
        "You already reported this question for that reason.");

    /// <summary>The report is resolved or rejected, so it can no longer change.</summary>
    public static readonly AppError Closed = new(
        "FLAG_CLOSED",
        AppErrorKind.Conflict,
        "This report is already closed.");
}
