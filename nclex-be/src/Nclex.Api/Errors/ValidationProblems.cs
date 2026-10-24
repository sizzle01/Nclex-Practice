using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Common.Validation;

namespace Nclex.Api.Errors;

/// <summary>One entry of <c>errors[]</c> in a VALIDATION_FAILED body (api.md section 1).</summary>
/// <param name="Field">camelCase name of the request field, e.g. "password".</param>
/// <param name="Code">What is wrong, e.g. "TOO_SHORT" (see ValidationErrorCodes).</param>
public sealed record FieldError(string Field, string Code);

/// <summary>Builds 400 VALIDATION_FAILED responses in the api.md section 1 shape.</summary>
public static class ValidationProblems
{
    public static IActionResult Create(IReadOnlyList<FieldError> errors) =>
        new ValidationProblemResult(errors);

    /// <summary>
    /// Replaces the framework's response when a body cannot be read at all (malformed JSON, or an
    /// unknown enum value such as track "XN").
    /// </summary>
    public static IActionResult FromModelState(ActionContext context) =>
        Create([.. context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .Select(entry => new FieldError(ToFieldName(entry.Key), ValidationErrorCodes.InvalidValue))]);

    /// <summary>"Email" becomes "email"; a JSON path such as "$.track" becomes "track"; an unreadable body becomes "body".</summary>
    public static string ToFieldName(string propertyPath)
    {
        var path = propertyPath.StartsWith("$.", StringComparison.Ordinal) ? propertyPath[2..] : propertyPath;
        if (path.Length == 0 || path == "$")
        {
            return "body";
        }

        return string.Join('.', path.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
    }

    /// <summary>
    /// Writes the body through IProblemDetailsService, like every other error, so it gets the same
    /// code, type, requestId and application/problem+json content type. (An ObjectResult would have
    /// its content type replaced by the controllers' [Produces("application/json")].)
    /// </summary>
    private sealed class ValidationProblemResult(IReadOnlyList<FieldError> errors) : IActionResult
    {
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var httpContext = context.HttpContext;
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Some fields are missing or invalid.",
                    Extensions = { ["errors"] = errors },
                },
            });
        }
    }
}
