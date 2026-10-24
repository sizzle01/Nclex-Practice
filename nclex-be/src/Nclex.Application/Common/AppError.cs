namespace Nclex.Application.Common;

/// <summary>
/// An expected failure that a use case reports to the caller, such as "email already taken".
/// The API turns it into a ProblemDetails body with <see cref="Code"/> (api.md section 1).
/// </summary>
/// <param name="Code">Machine-readable code from api.md section 1, e.g. "EMAIL_TAKEN".</param>
/// <param name="Kind">What sort of failure it is; the API maps this to an HTTP status.</param>
/// <param name="Title">Short message that is safe to show to the user.</param>
public sealed record AppError(string Code, AppErrorKind Kind, string Title);

/// <summary>Categories of expected failure, each matching one HTTP status.</summary>
public enum AppErrorKind
{
    /// <summary>400: the request is malformed or breaks a rule.</summary>
    Validation,

    /// <summary>401: the caller is not signed in, or their credentials are wrong.</summary>
    Unauthenticated,

    /// <summary>403: signed in, but not allowed.</summary>
    Forbidden,

    /// <summary>404: the resource does not exist (or is not yours).</summary>
    NotFound,

    /// <summary>409: the request clashes with the current state.</summary>
    Conflict,
}

/// <summary>Errors shared by several features.</summary>
public static class CommonErrors
{
    public static readonly AppError Unauthenticated = new(
        "UNAUTHENTICATED",
        AppErrorKind.Unauthenticated,
        "You need to log in again.");
}
