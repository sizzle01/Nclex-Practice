using System.Net;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Users;

/// <summary>
/// Changing the password or email and deleting the account ask for the current password again.
/// Wrong ones count toward a per-user lockout, on real Redis (see <see cref="SecurityTestHost"/>).
/// </summary>
public sealed class ReauthenticationTests(ApiFactory factory) : IAsyncDisposable
{
    private const string WrongPassword = "not-my-password";

    private readonly SecurityTestHost _host = new(factory);
    private readonly OutboxProbe _outbox = new(factory);
    private readonly AuditProbe _audit = new(factory);

    private AuthApi Api => _host.Api;

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    public static TheoryData<string, string> SensitiveEndpoints => new()
    {
        { "POST", "/api/v1/me/password" },
        { "POST", "/api/v1/me/email" },
        { "DELETE", "/api/v1/me" },
    };

    [Theory]
    [MemberData(nameof(SensitiveEndpoints))]
    public async Task A_wrong_current_password_gets_400_on_the_current_password_field(string method, string path)
    {
        var email = AuthApi.NewEmail();
        var user = await Api.SignupNewUserAsync(email);

        var response = await Api.SendWithTokenAsync(new HttpMethod(method), path, user.AccessToken, BodyFor(path, WrongPassword));

        // 400, not 401: the frontend treats 401 as "token expired" and would retry the request.
        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("currentPassword", "INCORRECT")]);
    }

    [Theory]
    [MemberData(nameof(SensitiveEndpoints))]
    public async Task A_wrong_current_password_changes_nothing(string method, string path)
    {
        var email = AuthApi.NewEmail();
        var user = await Api.SignupNewUserAsync(email);

        await Api.SendWithTokenAsync(new HttpMethod(method), path, user.AccessToken, BodyFor(path, WrongPassword));

        (await Api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _outbox.ForAsync(email)).ShouldAllBe(m => m.Template == "verify_email");
        (await Api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", user.AccessToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Five_wrong_current_passwords_lock_the_account()
    {
        var email = AuthApi.NewEmail();
        var user = await Api.SignupNewUserAsync(email);
        var body = new { currentPassword = WrongPassword, newPassword = "a-brand-new-password" };

        for (var attempt = 1; attempt <= 4; attempt++)
        {
            (await Api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/me/password", user.AccessToken, body))
                .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        var fifth = await Api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/me/password", user.AccessToken, body);

        var problem = await fifth.ShouldBeProblemAsync(HttpStatusCode.Locked, "ACCOUNT_LOCKED");
        problem.GetProperty("retryAfterSec").GetInt32().ShouldBe(15 * 60);
        // The lock covers logins too, even with the right password.
        await (await Api.LoginAsync(email, AuthApi.Password)).ShouldBeProblemAsync(HttpStatusCode.Locked, "ACCOUNT_LOCKED");
        (await _outbox.ForAsync(email)).Count(m => m.Template == "account_locked").ShouldBe(1);
        (await _audit.ActionsForAsync(user.Body.User.Id)).ShouldContain("auth.account_locked");
    }

    [Fact]
    public async Task A_locked_account_cannot_make_changes_even_with_the_right_password()
    {
        var email = AuthApi.NewEmail();
        var user = await Api.SignupNewUserAsync(email);
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            await Api.SendWithTokenAsync(HttpMethod.Delete, "/api/v1/me", user.AccessToken, new { currentPassword = WrongPassword });
        }

        var response = await Api.SendWithTokenAsync(HttpMethod.Delete, "/api/v1/me", user.AccessToken, new { currentPassword = AuthApi.Password });

        await response.ShouldBeProblemAsync(HttpStatusCode.Locked, "ACCOUNT_LOCKED");
    }

    [Fact]
    public async Task The_right_password_resets_the_count()
    {
        var email = AuthApi.NewEmail();
        var user = await Api.SignupNewUserAsync(email);
        for (var attempt = 1; attempt <= 4; attempt++)
        {
            await Api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/me/email", user.AccessToken, new { currentPassword = WrongPassword, newEmail = AuthApi.NewEmail() });
        }

        (await Api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/me/email", user.AccessToken, new { currentPassword = AuthApi.Password, newEmail = AuthApi.NewEmail() }))
            .StatusCode.ShouldBe(HttpStatusCode.Accepted);
        for (var attempt = 1; attempt <= 4; attempt++)
        {
            (await Api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/me/email", user.AccessToken, new { currentPassword = WrongPassword, newEmail = AuthApi.NewEmail() }))
                .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    [Theory]
    [MemberData(nameof(SensitiveEndpoints))]
    public async Task Each_endpoint_needs_a_signed_in_user(string method, string path)
    {
        var response = await Api.SendWithTokenAsync(new HttpMethod(method), path, accessToken: null, BodyFor(path, AuthApi.Password));

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    private static object BodyFor(string path, string currentPassword) => path switch
    {
        "/api/v1/me/password" => new { currentPassword, newPassword = "a-brand-new-password" },
        "/api/v1/me/email" => new { currentPassword, newEmail = AuthApi.NewEmail() },
        _ => new { currentPassword },
    };
}
