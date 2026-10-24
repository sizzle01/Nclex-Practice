namespace Nclex.Api.Errors;

public static class ProblemDetailsSetup
{
    /// <summary>
    /// Makes every ProblemDetails body the API writes, whether from our exception handler or from
    /// the framework (unknown route, auth challenge), follow the api.md section 1 shape:
    /// type, status, code, title, detail, requestId.
    /// </summary>
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;
            var status = problem.Status ?? context.HttpContext.Response.StatusCode;

            if (!problem.Extensions.ContainsKey("code") && ErrorCodes.ForStatus(status) is { } code)
            {
                problem.Extensions["code"] = code;
                problem.Type = ErrorCodes.TypeUri(code);
            }

            problem.Extensions["requestId"] = context.HttpContext.TraceIdentifier;
            // The framework adds a W3C traceId; api.md uses requestId only, so avoid two ids.
            problem.Extensions.Remove("traceId");
        });
        // Handlers are tried in registration order: expected failures first, then the 500 fallback.
        services.AddExceptionHandler<AppExceptionHandler>();
        services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        return services;
    }

    /// <summary>Turns bare error status codes (e.g. 404 for an unknown route) into ProblemDetails bodies.</summary>
    public static IApplicationBuilder UseApiStatusCodeProblemDetails(this IApplicationBuilder app) =>
        app.UseStatusCodePages(async context =>
        {
            var httpContext = context.HttpContext;
            var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext });
        });
}
