using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Infrastructure.Security;

namespace Nclex.Api.Auth;

/// <summary>
/// The signed-in user must have verified their email (architecture.md section 6, "Verified-email
/// guard for non-sample attempts"). Used through <see cref="Policies.VerifiedEmail"/>.
/// </summary>
public sealed class VerifiedEmailRequirement : IAuthorizationRequirement;

/// <summary>
/// Checks <see cref="VerifiedEmailRequirement"/> against the database rather than a token claim, so
/// verifying on one device (say, a phone) takes effect at once on the others, without waiting up
/// to 15 minutes for their access tokens to be refreshed.
/// </summary>
public sealed class VerifiedEmailHandler(IAppDbContext db) : AuthorizationHandler<VerifiedEmailRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifiedEmailRequirement requirement)
    {
        if (!Guid.TryParse(context.User.FindFirst(AccessTokenClaims.Subject)?.Value, out var userId))
        {
            return; // not signed in: the authentication requirement already fails with 401
        }

        var verified = await db.Users.AnyAsync(u => u.Id == userId && u.EmailVerifiedAt != null);
        if (verified)
        {
            context.Succeed(requirement);
        }
    }
}
