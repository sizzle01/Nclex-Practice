using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nclex.Application.Common.Validation;

namespace Nclex.Api.Errors;

/// <summary>One entry of <c>errors[]</c> in a VALIDATION_FAILED body (api.md section 1).</summary>
/// <param name="Field">camelCase name of the request field, e.g. "password".</param>
/// <param name="Code">What is wrong, e.g. "TOO_SHORT" (see ValidationErrorCodes).</param>
public sealed record FieldError(string Field, string Code);

/// <summary>Builds 400 VALIDATION_FAILED responses in the api.md section 1 shape.</summary>
public static class ValidationProblems
{
    public static ObjectResult Create(HttpContext httpContext, IReadOnlyList<FieldError> errors)
    {
        var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        // The factory applies ProblemDetailsSetup's customisation: code VALIDATION_FAILED, type and requestId.
        var problem = factory.CreateProblemDetails(
            httpContext,
            StatusCodes.Status400BadRequest,
            title: "Some fields are missing or invalid.");
        problem.Extensions["errors"] = errors;
        return new ObjectResult(problem) { StatusCode = StatusCodes.Status400BadRequest };
    }

    /// <summary>
    /// Replaces the framework's response when a body cannot be read at all (malformed JSON, or an
    /// unknown enum value such as track "XN").
    /// </summary>
    public static IActionResult FromModelState(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .Select(entry => new FieldError(ToFieldName(entry.Key), ValidationErrorCodes.InvalidValue))
            .ToList();
        return Create(context.HttpContext, errors);
    }

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
}
