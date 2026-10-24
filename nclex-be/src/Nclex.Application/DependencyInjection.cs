using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Auth.Login;
using Nclex.Application.Auth.Logout;
using Nclex.Application.Auth.LogoutAll;
using Nclex.Application.Auth.Refresh;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Auth.Signup;
using Nclex.Application.Users.GetMe;
using Nclex.Application.Users.UpdateMe;

namespace Nclex.Application;

public static class DependencyInjection
{
    /// <summary>Registers the use-case handlers and request validators.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SignupRequestValidator>();

        services.AddScoped<SessionService>();

        // Auth
        services.AddScoped<SignupHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<LogoutAllHandler>();

        // Users
        services.AddScoped<GetMeHandler>();
        services.AddScoped<UpdateMeHandler>();

        return services;
    }
}
