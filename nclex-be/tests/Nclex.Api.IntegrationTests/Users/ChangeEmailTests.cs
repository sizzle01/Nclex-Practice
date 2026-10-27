using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Users;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Users;

public sealed class ChangeEmailTests(ApiFactory factory)
{
    private const string RequestPath = "/api/v1/me/email";
    private const string ConfirmPath = "/api/v1/auth/email/confirm-change";

    private readonly AuthApi _api = new(factory);
    private readonly OutboxProbe _outbox = new(factory);
    private readonly AuditProbe _audit = new(factory);

    [Fact]
    public async Task A_request_sends_a_link_to_the_new_address_and_a_notice_to_the_old_one()
    {
        var (oldEmail, newEmail) = (AuthApi.NewEmail(), AuthApi.NewEmail());
        var user = await _api.SignupNewUserAsync(oldEmail);

        var response = await RequestAsync(user.AccessToken, newEmail);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        (await _outbox.ForAsync(newEmail)).ShouldHaveSingleItem().Template.ShouldBe("confirm_email_change");
        (await _outbox.ForAsync(oldEmail)).Count(m => m.Template == "email_change_requested").ShouldBe(1);
        (await _audit.ActionsForAsync(user.Body.User.Id)).ShouldContain("auth.email_change_requested");
    }

    [Fact]
    public async Task Nothing_changes_until_the_link_is_followed()
    {
        var (oldEmail, newEmail) = (AuthApi.NewEmail(), AuthApi.NewEmail());
        var user = await _api.SignupNewUserAsync(oldEmail);

        await RequestAsync(user.AccessToken, newEmail);

        (await GetProfileAsync(user.AccessToken)).Email.ShouldBe(oldEmail);
        (await _api.LoginAsync(oldEmail, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Following_the_link_switches_the_account_to_the_new_address()
    {
        var (oldEmail, newEmail) = (AuthApi.NewEmail(), AuthApi.NewEmail());
        var user = await _api.SignupNewUserAsync(oldEmail);
        await RequestAsync(user.AccessToken, newEmail);

        var response = await _api.PostAsync(ConfirmPath, new { token = await LinkTokenAsync(newEmail) });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var profile = await GetProfileAsync(user.AccessToken);
        profile.Email.ShouldBe(newEmail);
        // Following a link sent to the address proves the user owns it.
        profile.EmailVerified.ShouldBeTrue();
        (await _api.LoginAsync(newEmail, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _api.LoginAsync(oldEmail, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task The_old_address_is_told_once_the_change_has_happened()
    {
        var (oldEmail, newEmail) = (AuthApi.NewEmail(), AuthApi.NewEmail());
        var user = await _api.SignupNewUserAsync(oldEmail);
        await RequestAsync(user.AccessToken, newEmail);

        await _api.PostAsync(ConfirmPath, new { token = await LinkTokenAsync(newEmail) });

        (await _outbox.ForAsync(oldEmail)).Count(m => m.Template == "email_changed").ShouldBe(1);
        (await _audit.ActionsForAsync(user.Body.User.Id)).ShouldContain("auth.email_changed");
    }

    [Fact]
    public async Task A_link_works_only_once()
    {
        var newEmail = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync();
        await RequestAsync(user.AccessToken, newEmail);
        var token = await LinkTokenAsync(newEmail);
        await _api.PostAsync(ConfirmPath, new { token });

        var second = await _api.PostAsync(ConfirmPath, new { token });

        await second.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
    }

    [Fact]
    public async Task A_link_expires_after_24_hours()
    {
        var newEmail = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync();
        await RequestAsync(user.AccessToken, newEmail);
        var token = await LinkTokenAsync(newEmail);
        await AgeChangeEmailTokensAsync(user.Body.User.Id, TimeSpan.FromHours(24) + TimeSpan.FromSeconds(1));

        var response = await _api.PostAsync(ConfirmPath, new { token });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
    }

    [Fact]
    public async Task A_newer_request_makes_the_earlier_link_stop_working()
    {
        var (first, second) = (AuthApi.NewEmail(), AuthApi.NewEmail());
        var user = await _api.SignupNewUserAsync();
        await RequestAsync(user.AccessToken, first);
        var firstToken = await LinkTokenAsync(first);
        await RequestAsync(user.AccessToken, second);

        await (await _api.PostAsync(ConfirmPath, new { token = firstToken })).ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
        (await _api.PostAsync(ConfirmPath, new { token = await LinkTokenAsync(second) })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task An_address_that_already_has_an_account_is_refused_at_request()
    {
        var taken = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(taken);
        var user = await _api.SignupNewUserAsync();

        var response = await RequestAsync(user.AccessToken, taken.ToUpperInvariant());

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "EMAIL_TAKEN");
    }

    [Fact]
    public async Task An_address_taken_after_the_request_is_refused_at_confirmation()
    {
        var newEmail = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync();
        await RequestAsync(user.AccessToken, newEmail);
        var token = await LinkTokenAsync(newEmail);
        await _api.SignupNewUserAsync(newEmail);

        var response = await _api.PostAsync(ConfirmPath, new { token });

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "EMAIL_TAKEN");
    }

    [Fact]
    public async Task Changing_to_the_current_address_is_refused()
    {
        var email = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync(email);

        var response = await RequestAsync(user.AccessToken, email.ToUpperInvariant());

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("newEmail", "UNCHANGED")]);
    }

    [Fact]
    public async Task The_new_address_must_be_a_valid_email()
    {
        var user = await _api.SignupNewUserAsync();

        var response = await RequestAsync(user.AccessToken, "not-an-email");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("newEmail", "INVALID_FORMAT")]);
    }

    [Fact]
    public async Task Confirming_needs_a_token()
    {
        var response = await _api.PostAsync(ConfirmPath, new { token = "" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("token", "REQUIRED")]);
    }

    [Fact]
    public async Task A_verification_link_cannot_be_used_to_confirm_a_change()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);

        var response = await _api.PostAsync(ConfirmPath, new { token = await _outbox.LatestTokenAsync(email, "verify_email") });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
    }

    private Task<HttpResponseMessage> RequestAsync(string accessToken, string newEmail) =>
        _api.SendWithTokenAsync(HttpMethod.Post, RequestPath, accessToken, new { currentPassword = AuthApi.Password, newEmail });

    private Task<string> LinkTokenAsync(string newEmail) => _outbox.LatestTokenAsync(newEmail, "confirm_email_change");

    private async Task<UserProfile> GetProfileAsync(string accessToken)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", accessToken);
        return (await response.Content.ReadFromJsonAsync<UserProfile>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task AgeChangeEmailTokensAsync(Guid userId, TimeSpan age)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.ExecuteSqlAsync(
            $"""
            UPDATE email_tokens SET created_at = created_at - {age}, expires_at = expires_at - {age}
            WHERE purpose = 'change_email' AND user_id = {userId}
            """,
            TestContext.Current.CancellationToken);
    }
}
