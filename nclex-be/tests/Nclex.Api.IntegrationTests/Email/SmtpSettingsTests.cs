using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nclex.Infrastructure.Email;

namespace Nclex.Api.IntegrationTests.Email;

/// <summary>
/// SMTP login: Mailpit takes mail anonymously, while providers such as Brevo need a username and
/// key (operations.md section 2, Worker settings).
/// </summary>
public sealed class SmtpSettingsTests
{
    [Fact]
    public void Without_a_username_the_client_connects_anonymously()
    {
        var settings = new SmtpSettings { Host = "mailpit", Port = 1025 };

        settings.CreateCredentials().ShouldBeNull();
    }

    [Fact]
    public void With_a_username_the_client_logs_in_with_the_username_and_key()
    {
        var settings = new SmtpSettings
        {
            Host = "smtp-relay.example.com",
            Port = 587,
            EnableSsl = true,
            Username = "sender@example.com",
            Password = "smtp-key",
        };

        var credentials = settings.CreateCredentials().ShouldNotBeNull();

        credentials.UserName.ShouldBe("sender@example.com");
        credentials.Password.ShouldBe("smtp-key");
    }

    [Fact]
    public void A_username_without_a_key_is_rejected_at_start_up()
    {
        var options = EmailOptionsFrom(new Dictionary<string, string?>
        {
            ["Email:From"] = "NCLEX Practice <no-reply@example.com>",
            ["Email:FrontendBaseUrl"] = "https://app.example.com",
            ["Email:Smtp:Host"] = "smtp-relay.example.com",
            ["Email:Smtp:Username"] = "sender@example.com",
        });

        var error = Should.Throw<OptionsValidationException>(() => options.Value);

        error.Message.ShouldContain("Email:Smtp:Password is required");
    }

    private static IOptions<EmailOptions> EmailOptionsFrom(Dictionary<string, string?> settings)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        services.AddEmailDelivery();
        return services.BuildServiceProvider().GetRequiredService<IOptions<EmailOptions>>();
    }
}
