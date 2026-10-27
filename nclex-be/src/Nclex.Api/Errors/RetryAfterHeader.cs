using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Nclex.Api.Errors;

/// <summary>
/// api.md section 1: 429 and 423 responses say when to try again, both as the Retry-After header
/// and as <c>retryAfterSec</c> in the body. Whole seconds, rounded up, and at least 1.
/// </summary>
public static class RetryAfterHeader
{
    public static void Apply(HttpContext httpContext, ProblemDetails problem, TimeSpan retryAfter)
    {
        var seconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
        httpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
        problem.Extensions["retryAfterSec"] = seconds;
    }
}
