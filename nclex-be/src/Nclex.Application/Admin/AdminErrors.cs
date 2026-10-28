using Nclex.Application.Common;

namespace Nclex.Application.Admin;

/// <summary>Errors returned by the admin console endpoints (api.md sections 1 and 8).</summary>
public static class AdminErrors
{
    /// <summary>The code for an action the current status doesn't allow (api.md section 1, added in F14).</summary>
    public const string InvalidStatusTransitionCode = "INVALID_STATUS_TRANSITION";

    public static readonly AppError QuestionNotFound = new("NOT_FOUND", AppErrorKind.NotFound, "That question was not found.");

    public static readonly AppError VersionNotFound = new("NOT_FOUND", AppErrorKind.NotFound, "That question version was not found.");

    public static readonly AppError MediaNotFound = new("NOT_FOUND", AppErrorKind.NotFound, "That media file was not found.");

    public static readonly AppError TestSetNotFound = new("NOT_FOUND", AppErrorKind.NotFound, "That test set was not found.");

    public static readonly AppError BankNotFound = new("NOT_FOUND", AppErrorKind.NotFound, "That question bank was not found.");

    public static readonly AppError UserNotFound = new("NOT_FOUND", AppErrorKind.NotFound, "That user was not found.");

    /// <summary>For example approving a draft, or editing a version that is already in review.</summary>
    public static AppError WrongStatus(string title) => new(InvalidStatusTransitionCode, AppErrorKind.Conflict, title);

    /// <summary>A reviewer tried to approve or reject a version they wrote (database.md 2.4).</summary>
    public static readonly AppError OwnVersion = new(
        "FORBIDDEN",
        AppErrorKind.Forbidden,
        "You can't review a version you wrote. Ask another reviewer.");

    /// <summary>An admin tried to change their own role, which could lock everyone out of the console.</summary>
    public static readonly AppError OwnRole = new(
        "FORBIDDEN",
        AppErrorKind.Forbidden,
        "You can't change your own role. Ask another admin.");

    /// <summary>The upload link was changed or has expired; ask for a new one.</summary>
    public static readonly AppError UploadLinkInvalid = new(
        "FORBIDDEN",
        AppErrorKind.Forbidden,
        "This upload link is invalid or has expired.");
}
