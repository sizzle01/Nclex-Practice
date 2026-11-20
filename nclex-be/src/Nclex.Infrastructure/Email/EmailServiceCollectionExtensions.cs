using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Abstractions;
using Nclex.Infrastructure.Email.Templates;

namespace Nclex.Infrastructure.Email;

public static class EmailServiceCollectionExtensions
{
    /// <summary>
    /// Registers rendering and sending of outbox emails. Only the worker host sends email; the API
    /// only queues it (IEmailOutbox, registered by AddInfrastructure).
    /// </summary>
    public static IServiceCollection AddEmailDelivery(this IServiceCollection services)
    {
        services.AddOptions<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName)
            .Validate(o => o.From.Length > 0, "Email:From is required.")
            .Validate(o => Uri.IsWellFormedUriString(o.FrontendBaseUrl, UriKind.Absolute), "Email:FrontendBaseUrl must be an absolute URL.")
            .Validate(o => o.Smtp.Host.Length > 0, "Email:Smtp:Host is required.")
            // A login without its key would fail on every send; refuse to start instead.
            .Validate(o => o.Smtp.Username.Length == 0 || o.Smtp.Password.Length > 0, "Email:Smtp:Password is required when Email:Smtp:Username is set.")
            .ValidateOnStart();

        services.AddSingleton<EmailLinks>();
        services.AddSingleton<IEmailTemplate, VerifyEmailTemplate>();
        services.AddSingleton<IEmailTemplate, ResetPasswordTemplate>();
        services.AddSingleton<IEmailTemplate, ConfirmEmailChangeTemplate>();
        services.AddSingleton<IEmailTemplate, EmailChangeRequestedTemplate>();
        services.AddSingleton<IEmailTemplate, EmailChangedTemplate>();
        services.AddSingleton<IEmailTemplate, PasswordChangedTemplate>();
        services.AddSingleton<IEmailTemplate, AccountLockedTemplate>();
        services.AddSingleton<IEmailTemplate, NewDeviceSignInTemplate>();
        services.AddSingleton<IEmailTemplate, ResultPassTemplate>();
        services.AddSingleton<IEmailTemplate, ResultFailTemplate>();
        services.AddSingleton<IEmailTemplate, ResultCloseTemplate>();
        services.AddSingleton<IEmailTemplate, ResultReadyTemplate>();
        services.AddSingleton<IEmailTemplate, DraftReminderTemplate>();
        services.AddSingleton<IEmailTemplate, FlagResolvedTemplate>();
        services.AddSingleton<EmailTemplates>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddScoped<OutboxDispatcher>();
        return services;
    }
}
