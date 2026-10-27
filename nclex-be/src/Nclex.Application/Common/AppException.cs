namespace Nclex.Application.Common;

/// <summary>
/// Thrown by a use case to stop with an expected <see cref="AppError"/>. The API's exception
/// handler turns it into a ProblemDetails response; it is not logged as a server error.
/// </summary>
/// <param name="error">The failure to report.</param>
/// <param name="retryAfter">For rate limits and lockouts: how long until the caller may try again.</param>
/// <param name="fieldErrors">For VALIDATION_FAILED: which fields are wrong, sent as <c>errors[]</c>.</param>
public sealed class AppException(AppError error, TimeSpan? retryAfter = null, IReadOnlyList<AppFieldError>? fieldErrors = null)
    : Exception(error.Title)
{
    public AppError Error { get; } = error;

    /// <summary>Sent as the Retry-After header and <c>retryAfterSec</c> (api.md section 1).</summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;

    /// <summary>Sent as <c>errors: [{ field, code }]</c>; empty when the error is not about fields.</summary>
    public IReadOnlyList<AppFieldError> FieldErrors { get; } = fieldErrors ?? [];

    /// <summary>A 400 VALIDATION_FAILED about one field, e.g. ("currentPassword", "INCORRECT").</summary>
    public static AppException ForField(string field, string code) =>
        new(CommonErrors.ValidationFailed, fieldErrors: [new AppFieldError(field, code)]);
}

/// <summary>One entry of <c>errors[]</c> raised by a use case.</summary>
/// <param name="Field">camelCase request field name, e.g. "currentPassword".</param>
/// <param name="Code">A code from <see cref="Validation.ValidationErrorCodes"/>.</param>
public sealed record AppFieldError(string Field, string Code);
