namespace Nclex.Application.Common;

/// <summary>
/// Thrown by a use case to stop with an expected <see cref="AppError"/>. The API's exception
/// handler turns it into a ProblemDetails response; it is not logged as a server error.
/// </summary>
/// <param name="error">The failure to report.</param>
/// <param name="retryAfter">For rate limits and lockouts: how long until the caller may try again.</param>
public sealed class AppException(AppError error, TimeSpan? retryAfter = null) : Exception(error.Title)
{
    public AppError Error { get; } = error;

    /// <summary>Sent as the Retry-After header and <c>retryAfterSec</c> (api.md section 1).</summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
