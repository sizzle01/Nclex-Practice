using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Common;

namespace Nclex.Api.Errors;

/// <summary>
/// Turns an <see cref="AppException"/> (an expected failure such as EMAIL_TAKEN) into a
/// ProblemDetails body with its code. Expected failures are not server errors, so they are not
/// logged as errors. Runs before <see cref="ProblemDetailsExceptionHandler"/>.
/// </summary>
public sealed class AppExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not AppException { Error: var error })
        {
            return false;
        }

        var status = StatusFor(error.Kind);
        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Type = ErrorCodes.TypeUri(error.Code),
                Title = error.Title,
                Extensions = { ["code"] = error.Code },
            },
        });
    }

    private static int StatusFor(AppErrorKind kind) => kind switch
    {
        AppErrorKind.Validation => StatusCodes.Status400BadRequest,
        AppErrorKind.Unauthenticated => StatusCodes.Status401Unauthorized,
        AppErrorKind.Forbidden => StatusCodes.Status403Forbidden,
        AppErrorKind.NotFound => StatusCodes.Status404NotFound,
        AppErrorKind.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };
}
