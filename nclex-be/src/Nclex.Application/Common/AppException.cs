namespace Nclex.Application.Common;

/// <summary>
/// Thrown by a use case to stop with an expected <see cref="AppError"/>. The API's exception
/// handler turns it into a ProblemDetails response; it is not logged as a server error.
/// </summary>
public sealed class AppException(AppError error) : Exception(error.Title)
{
    public AppError Error { get; } = error;
}
