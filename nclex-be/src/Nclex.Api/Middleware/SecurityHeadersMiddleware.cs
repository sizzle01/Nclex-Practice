namespace Nclex.Api.Middleware;

/// <summary>
/// Security headers on every API response (architecture.md section 6). The API only returns JSON,
/// media and event streams, never pages, so its CSP allows nothing to run and nothing to frame it.
/// The frontend's own headers and CSP are set in nclex-fe/src/proxy.ts.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        // OnStarting, so the headers also land on error bodies the exception handler writes.
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers["Referrer-Policy"] = "no-referrer";
            headers.XFrameOptions = "DENY";
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            // Media and JSON are for this site only; the CDN for premium media comes later (section 6).
            headers["Cross-Origin-Resource-Policy"] = "same-site";
            return Task.CompletedTask;
        });

        return next(context);
    }
}
