namespace Nclex.Api.Middleware;

/// <summary>
/// Gives every request an id that appears in all its log lines, in the X-Request-Id response
/// header and in error bodies, so a user-reported error can be matched to its logs
/// (architecture.md section 9, api.md section 1 error format).
/// </summary>
public sealed class RequestIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Request-Id";

    public Task InvokeAsync(HttpContext context)
    {
        // Always generated server-side: a client-supplied id could be used to forge log correlation.
        // Version 7 GUIDs are time-ordered, so ids sort by when the request arrived.
        var requestId = $"req_{Guid.CreateVersion7():N}";
        context.TraceIdentifier = requestId;

        // Set just before the response starts: the exception handler clears headers when it
        // replaces a failed response with an error body, and the id matters most on errors.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = requestId;
            return Task.CompletedTask;
        });

        // Logging/RequestIdEnricher reads TraceIdentifier to put the id on every log line.
        return next(context);
    }
}
