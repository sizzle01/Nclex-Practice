using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Auth.ConfirmEmailChange;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Auth.ForgotPassword;
using Nclex.Application.Auth.Lockout;
using Nclex.Application.Auth.Login;
using Nclex.Application.Auth.Logout;
using Nclex.Application.Auth.LogoutAll;
using Nclex.Application.Auth.Refresh;
using Nclex.Application.Auth.ResendVerification;
using Nclex.Application.Auth.ResetPassword;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Auth.Signup;
using Nclex.Application.Auth.VerifyEmail;
using Nclex.Application.Email;
using Nclex.Application.Users;
using Nclex.Application.Users.ChangeEmail;
using Nclex.Application.Users.ChangePassword;
using Nclex.Application.Users.DeleteAccount;
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
        services.AddScoped<EmailTokenService>();
        services.AddScoped<AccountEmails>();
        services.AddScoped<SecurityEmails>();
        services.AddScoped<LoginThrottle>();
        services.AddScoped<AccountLockout>();
        services.AddScoped<Reauthentication>();
        services.AddScoped<CurrentAccount>();

        // Auth
        services.AddScoped<SignupHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<LogoutAllHandler>();
        services.AddScoped<VerifyEmailHandler>();
        services.AddScoped<ResendVerificationHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<ConfirmEmailChangeHandler>();

        // Users
        services.AddScoped<GetMeHandler>();
        services.AddScoped<UpdateMeHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<ChangeEmailHandler>();
        services.AddScoped<DeleteAccountHandler>();

        return services;
    }
}
