using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Security;

public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    /// Registers password hashing and token services. Only the API host needs these, so they are
    /// separate from AddInfrastructure (the worker host would otherwise need a JWT key).
    /// </summary>
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .Validate(o => o.Issuer.Length > 0 && o.Audience.Length > 0, "Jwt:Issuer and Jwt:Audience are required.")
            .Validate(
                o => Encoding.UTF8.GetByteCount(o.SigningKey) >= JwtOptions.MinimumSigningKeyBytes,
                $"Jwt:SigningKey must be at least {JwtOptions.MinimumSigningKeyBytes} bytes.")
            .ValidateOnStart();

        services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        return services;
    }
}
