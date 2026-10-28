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
        if (exception is not AppException { Error: var error } appException)
        {
            return false;
        }

        var status = StatusFor(error.Kind);
        httpContext.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Type = ErrorCodes.TypeUri(error.Code),
            Title = error.Title,
            Extensions = { ["code"] = error.Code },
        };

        // api.md section 1: 429 responses include Retry-After; lockouts also say retryAfterSec.
        if (appException.RetryAfter is { } retryAfter)
        {
            RetryAfterHeader.Apply(httpContext, problem, retryAfter);
        }

        // Field problems found by a use case (e.g. a wrong current password) use the same
        // errors[] shape as the validators' VALIDATION_FAILED responses.
        if (appException.FieldErrors.Count > 0)
        {
            problem.Extensions["errors"] = appException.FieldErrors.Select(e => new FieldError(e.Field, e.Code, e.Message)).ToList();
        }

        foreach (var (name, value) in appException.Details)
        {
            problem.Extensions[name] = value;
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
        });
    }

    private static int StatusFor(AppErrorKind kind) => kind switch
    {
        AppErrorKind.Validation => StatusCodes.Status400BadRequest,
        AppErrorKind.Unauthenticated => StatusCodes.Status401Unauthorized,
        AppErrorKind.Forbidden => StatusCodes.Status403Forbidden,
        AppErrorKind.NotFound => StatusCodes.Status404NotFound,
        AppErrorKind.Conflict => StatusCodes.Status409Conflict,
        AppErrorKind.Unprocessable => StatusCodes.Status422UnprocessableEntity,
        AppErrorKind.Locked => StatusCodes.Status423Locked,
        AppErrorKind.TooManyRequests => StatusCodes.Status429TooManyRequests,
        _ => StatusCodes.Status500InternalServerError,
    };
}
