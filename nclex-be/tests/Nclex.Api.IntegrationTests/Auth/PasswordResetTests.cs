using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Auth;

public sealed class PasswordResetTests(ApiFactory factory)
{
    private const string ForgotPath = "/api/v1/auth/password/forgot";
    private const string ResetPath = "/api/v1/auth/password/reset";
    private const string NewPassword = "a-brand-new-password";

    private readonly AuthApi _api = new(factory);
    private readonly OutboxProbe _outbox = new(factory);

    [Fact]
    public async Task Forgot_password_queues_a_reset_email_for_an_existing_account()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);

        var response = await _api.PostAsync(ForgotPath, new { email = email.ToUpperInvariant() });

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await _outbox.ForAsync(email)).Count(m => m.Template == "reset_password").ShouldBe(1);
    }

    [Fact]
    public async Task Forgot_password_for_an_unknown_email_looks_the_same_and_sends_nothing()
    {
        var email = AuthApi.NewEmail();

        var response = await _api.PostAsync(ForgotPath, new { email });

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
        (await _outbox.ForAsync(email)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Forgot_password_with_a_malformed_email_fails_validation()
    {
        var response = await _api.PostAsync(ForgotPath, new { email = "nope" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("email", "INVALID_FORMAT")]);
    }

    [Fact]
    public async Task Reset_sets_the_new_password()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var token = await RequestResetTokenAsync(email);

        var response = await _api.PostAsync(ResetPath, new { token, newPassword = NewPassword });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _api.LoginAsync(email, NewPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
        await (await _api.LoginAsync(email, AuthApi.Password)).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Reset_emails_a_password_changed_notice_and_is_audited()
    {
        var email = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync(email);
        var token = await RequestResetTokenAsync(email);

        await _api.PostAsync(ResetPath, new { token, newPassword = NewPassword });

        (await _outbox.ForAsync(email)).Count(m => m.Template == "password_changed").ShouldBe(1);
        (await new AuditProbe(factory).ActionsForAsync(user.Body.User.Id)).ShouldContain("auth.password_reset");
    }

    [Fact]
    public async Task Reset_signs_out_every_device_by_default()
    {
        var email = AuthApi.NewEmail();
        var laptop = await _api.SignupNewUserAsync(email);
        var phone = await _api.LoginOkAsync(email);
        var token = await RequestResetTokenAsync(email);

        await _api.PostAsync(ResetPath, new { token, newPassword = NewPassword });

        (await _api.RefreshAsync(laptop.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await _api.RefreshAsync(phone.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reset_can_keep_existing_sessions()
    {
        var email = AuthApi.NewEmail();
        var laptop = await _api.SignupNewUserAsync(email);
        var token = await RequestResetTokenAsync(email);

        await _api.PostAsync(ResetPath, new { token, newPassword = NewPassword, signOutEverywhere = false });

        (await _api.RefreshAsync(laptop.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Reset_also_verifies_the_email()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var token = await RequestResetTokenAsync(email);

        await _api.PostAsync(ResetPath, new { token, newPassword = NewPassword });

        var signedIn = await _api.LoginAsync(email, NewPassword);
        (await AuthApi.SignedIn.FromAsync(signedIn)).Body.User.EmailVerified.ShouldBeTrue();
    }

    [Fact]
    public async Task A_reset_token_works_only_once()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var token = await RequestResetTokenAsync(email);
        await _api.PostAsync(ResetPath, new { token, newPassword = NewPassword });

        var second = await _api.PostAsync(ResetPath, new { token, newPassword = "yet-another-password" });

        await second.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
        (await _api.LoginAsync(email, NewPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_reset_token_expires_after_30_minutes()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var token = await RequestResetTokenAsync(email);
        var stored = await AgeResetTokensAsync(email, TimeSpan.FromMinutes(30));

        var response = await _api.PostAsync(ResetPath, new { token, newPassword = NewPassword });

        stored.ShouldBe(1);
        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
        (await _api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Asking_again_makes_the_earlier_reset_link_stop_working()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var first = await RequestResetTokenAsync(email);
        var second = await RequestResetTokenAsync(email);

        await (await _api.PostAsync(ResetPath, new { token = first, newPassword = NewPassword }))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
        (await _api.PostAsync(ResetPath, new { token = second, newPassword = NewPassword })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task A_verification_token_cannot_reset_a_password()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var verifyToken = await _outbox.LatestTokenAsync(email, "verify_email");

        var response = await _api.PostAsync(ResetPath, new { token = verifyToken, newPassword = NewPassword });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
    }

    [Fact]
    public async Task Reset_checks_the_new_password_like_sign_up()
    {
        var response = await _api.PostAsync(ResetPath, new { token = "", newPassword = "short" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("token", "REQUIRED"), ("newPassword", "TOO_SHORT")], ignoreOrder: true);
    }

    private async Task<string> RequestResetTokenAsync(string email)
    {
        (await _api.PostAsync(ForgotPath, new { email })).StatusCode.ShouldBe(HttpStatusCode.Accepted);
        return await _outbox.LatestTokenAsync(email, "reset_password");
    }

    // Moves the reset tokens back in time, as if they were issued `age` ago.
    private async Task<int> AgeResetTokensAsync(string email, TimeSpan age)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Database.ExecuteSqlAsync(
            $"""
            UPDATE email_tokens SET created_at = created_at - {age}, expires_at = expires_at - {age}
            WHERE purpose = 'reset_password' AND consumed_at IS NULL
              AND user_id = (SELECT id FROM users WHERE email = {email})
            """,
            TestContext.Current.CancellationToken);
    }
}
