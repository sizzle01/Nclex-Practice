using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Nclex.Application.Auth;
using Nclex.Application.Common;

namespace Nclex.Api.Auth;

/// <summary>
/// Gives a failed verified-email check its own error code. A plain 403 would say FORBIDDEN, but
/// api.md section 1 has EMAIL_NOT_VERIFIED, so the frontend can offer to resend the link.
/// Every other result is handled by ASP.NET Core's default handler.
/// </summary>
public sealed class ApiAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        var failedVerifiedEmail = authorizeResult.Forbidden
            && authorizeResult.AuthorizationFailure?.FailedRequirements.OfType<VerifiedEmailRequirement>().Any() == true;
        if (failedVerifiedEmail)
        {
            // The exception handler turns this into the ProblemDetails body.
            throw new AppException(AuthErrors.EmailNotVerified);
        }

        return _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
