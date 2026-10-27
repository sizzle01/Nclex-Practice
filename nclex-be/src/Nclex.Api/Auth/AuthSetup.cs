using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Security;

namespace Nclex.Api.Auth;

public static class AuthSetup
{
    /// <summary>
    /// JWT bearer authentication and the authorization policies (architecture.md section 6).
    /// Program.cs makes every controller action require a signed-in user unless it is marked
    /// [AllowAnonymous].
    /// </summary>
    public static IServiceCollection AddApiAuth(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Configured from JwtOptions when first needed, so tests can supply settings after Program.cs runs.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
            {
                // Keep claim names as written in the token ("sub", "role") instead of mapping them
                // to long XML-namespace names.
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new()
                {
                    ValidIssuer = jwt.Value.Issuer,
                    ValidAudience = jwt.Value.Audience,
                    IssuerSigningKey = JwtAccessTokenIssuer.CreateSigningKey(jwt.Value),
                    ValidAlgorithms = ["HS256"],
                    NameClaimType = AccessTokenClaims.Subject,
                    RoleClaimType = AccessTokenClaims.Role,
                    // Small allowance for clock drift between servers; the default of 5 minutes
                    // would stretch a 15-minute token by a third.
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorizationBuilder()
            .AddRolePolicies()
            .AddPolicy(Policies.VerifiedEmail, policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new VerifiedEmailRequirement()));
        // Scoped: the handler reads the database through the request's DbContext.
        services.AddScoped<IAuthorizationHandler, VerifiedEmailHandler>();
        services.Replace(ServiceDescriptor.Singleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationResultHandler>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        return services;
    }

    /// <summary>One policy per role, named by <see cref="Policies.Role"/>, e.g. [Authorize(Policy = "role:admin")].</summary>
    private static AuthorizationBuilder AddRolePolicies(this AuthorizationBuilder builder)
    {
        foreach (var role in Enum.GetValues<UserRole>())
        {
            builder.AddPolicy(Policies.Role(role), policy => policy.RequireRole(role.ToClaimValue()));
        }

        return builder;
    }
}

/// <summary>Names of authorization policies.</summary>
public static class Policies
{
    /// <summary>
    /// Policy that requires the given role. Use the constants below in attributes, e.g.
    /// <c>[Authorize(Policy = Policies.Candidate)]</c>.
    /// </summary>
    public static string Role(UserRole role) => $"role:{role.ToClaimValue()}";

    public const string Candidate = "role:candidate";
    public const string Educator = "role:educator";
    public const string Author = "role:author";
    public const string Reviewer = "role:reviewer";
    public const string Admin = "role:admin";

    /// <summary>Signed in with a verified email; returns 403 EMAIL_NOT_VERIFIED otherwise (protects non-sample attempts from F08).</summary>
    public const string VerifiedEmail = "verified-email";
}
