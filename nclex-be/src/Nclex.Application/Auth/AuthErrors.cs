using Nclex.Application.Common;

namespace Nclex.Application.Auth;

/// <summary>Errors raised by sign-up, login and the emailed-link flows (api.md section 1).</summary>
public static class AuthErrors
{
    public static readonly AppError EmailTaken = new(
        "EMAIL_TAKEN",
        AppErrorKind.Conflict,
        "An account with this email already exists.");

    // Deliberately vague: saying which part was wrong would reveal which emails have accounts.
    public static readonly AppError InvalidCredentials = new(
        "INVALID_CREDENTIALS",
        AppErrorKind.Unauthenticated,
        "The email or password is incorrect.");

    // Not in api.md, which has no code for a bad emailed link. One code covers unknown, expired,
    // used and superseded tokens, so a response never reveals which of these a token is.
    public static readonly AppError InvalidToken = new(
        "INVALID_TOKEN",
        AppErrorKind.Validation,
        "This link is invalid or has expired.");

    public static readonly AppError EmailNotVerified = new(
        "EMAIL_NOT_VERIFIED",
        AppErrorKind.Forbidden,
        "Verify your email address to continue.");
}
