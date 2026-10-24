using Nclex.Application.Common;

namespace Nclex.Application.Auth;

/// <summary>Errors raised by sign-up and login (api.md section 1).</summary>
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
}
