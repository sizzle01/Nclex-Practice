using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nclex.Api.Errors;
using Nclex.Application.Abstractions;

namespace Nclex.Api.RateLimiting;

public static class RateLimitingSetup
{
    /// <summary>Policy name for POST /auth/login: 50 attempts per 15 minutes per client IP (architecture.md section 6).</summary>
    public const string LoginPerIp = "login-per-ip";

    public const int LoginPerIpLimit = 50;

    public static readonly TimeSpan LoginPerIpWindow = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Registers the rate-limit policies. The per-(email, IP) login limit is not here: the email is
    /// in the request body, which the middleware cannot read, so LoginHandler applies it.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(LoginPerIp, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.Get(ip, partitionIp => new RedisSlidingWindowLimiter(
                    key: $"login-ip:{partitionIp}",
                    LoginPerIpLimit,
                    LoginPerIpWindow,
                    context.RequestServices.GetRequiredService<IRateLimitStore>()));
            });

            options.OnRejected = WriteRateLimitedProblemAsync;
        });
        return services;
    }

    // api.md section 1: 429 RATE_LIMITED with Retry-After, in the usual ProblemDetails shape.
    private static async ValueTask WriteRateLimitedProblemAsync(OnRejectedContext rejected, CancellationToken cancellationToken)
    {
        var httpContext = rejected.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests. Please try again later.",
        };

        if (rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            RetryAfterHeader.Apply(httpContext, problem, retryAfter);
        }

        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>()
            .WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problem });
    }
}
