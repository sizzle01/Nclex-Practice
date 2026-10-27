using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Email;
using Nclex.Infrastructure.Email.Templates;

namespace Nclex.Api.IntegrationTests.Email;

/// <summary>Renders the F03 templates through the worker's registrations.</summary>
public sealed class SecurityEmailTemplateTests(ApiFactory factory)
{
    private static readonly DateTimeOffset At = new(2026, 10, 27, 14, 5, 0, TimeSpan.Zero);

    [Fact]
    public void The_confirm_link_points_at_the_confirm_page_and_counts_as_a_secret()
    {
        var template = Template(EmailTemplateNames.ConfirmEmailChange);

        var email = template.Render(Json(new ConfirmEmailChangePayload("Ada", "new@example.com", "tok/en+1")));

        template.PayloadHoldsSecret.ShouldBeTrue();
        email.Subject.ShouldBe("Confirm your new email address");
        email.TextBody.ShouldContain($"{ApiFactory.FrontendBaseUrl}/confirm-email-change?token=tok%2Fen%2B1");
        email.TextBody.ShouldContain("new@example.com");
    }

    public static TheoryData<string, object, string> Notices => new()
    {
        { EmailTemplateNames.EmailChangeRequested, new EmailChangeRequestedPayload("Ada", "new@example.com"), "A change of email address was requested" },
        { EmailTemplateNames.EmailChanged, new EmailChangedPayload("Ada", "new@example.com", At), "Your email address was changed" },
        { EmailTemplateNames.PasswordChanged, new PasswordChangedPayload("Ada", At), "Your password was changed" },
        { EmailTemplateNames.AccountLocked, new AccountLockedPayload("Ada", At), "Your account was temporarily locked" },
        { EmailTemplateNames.NewDeviceSignIn, new NewDeviceSignInPayload("Ada", At, "Firefox/140", "10.1.2.3"), "New sign-in to your account" },
    };

    [Theory]
    [MemberData(nameof(Notices))]
    public void Each_notice_renders_with_its_subject_and_keeps_its_payload(string name, object payload, string subject)
    {
        var template = Template(name);

        var email = template.Render(Json(payload));

        email.Subject.ShouldBe(subject);
        email.HtmlBody.ShouldContain("Hello Ada,");
        email.TextBody.ShouldContain("Hello Ada,");
        // Notices carry no link token, so the worker keeps their payload after sending.
        template.PayloadHoldsSecret.ShouldBeFalse();
    }

    [Fact]
    public void Times_are_shown_in_utc()
    {
        var email = Template(EmailTemplateNames.AccountLocked).Render(Json(new AccountLockedPayload(null, At)));

        email.TextBody.ShouldContain("27 Oct 2026 at 14:05 UTC");
        email.TextBody.ShouldContain("Hello,");
    }

    [Fact]
    public void User_controlled_values_are_html_encoded()
    {
        var payload = new NewDeviceSignInPayload("<b>Ada</b>", At, "<script>alert(1)</script>", null);

        var email = Template(EmailTemplateNames.NewDeviceSignIn).Render(Json(payload));

        email.HtmlBody.ShouldNotContain("<script>");
        email.HtmlBody.ShouldNotContain("<b>Ada</b>");
        email.HtmlBody.ShouldContain("&lt;script&gt;");
        email.TextBody.ShouldContain("IP address: unknown");
    }

    [Fact]
    public void Notices_offer_the_forgot_password_page()
    {
        var email = Template(EmailTemplateNames.PasswordChanged).Render(Json(new PasswordChangedPayload(null, At)));

        email.TextBody.ShouldContain($"Reset my password: {ApiFactory.FrontendBaseUrl}/forgot-password");
    }

    // The same options the API uses to queue payloads (web defaults: camelCase).
    private static readonly JsonSerializerOptions PayloadJson = new(JsonSerializerDefaults.Web);

    private static string Json(object payload) => JsonSerializer.Serialize(payload, payload.GetType(), PayloadJson);

    private IEmailTemplate Template(string name) =>
        factory.Services.GetRequiredService<EmailTemplates>().Get(name);
}
