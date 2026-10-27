using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Auth;

/// <summary>
/// architecture.md section 6, "Brute force": 5 failed logins per (email, IP) in 15 minutes lock
/// the account for 15 minutes. Runs against real Redis with a manual clock (see <see cref="SecurityTestHost"/>).
/// </summary>
public sealed class LockoutTests(ApiFactory factory) : IAsyncDisposable
{
    private const string WrongPassword = "wrong-password-123";

    private readonly SecurityTestHost _host = new(factory);
    private readonly OutboxProbe _outbox = new(factory);
    private readonly AuditProbe _audit = new(factory);

    private AuthApi Api => _host.Api;

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    [Fact]
    public async Task Five_wrong_passwords_get_401_and_the_sixth_attempt_gets_423_with_retry_after()
    {
        var email = AuthApi.NewEmail();
        await Api.SignupNewUserAsync(email);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            await (await Api.LoginAsync(email, WrongPassword)).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        }

        var sixth = await Api.LoginAsync(email, WrongPassword);

        var problem = await sixth.ShouldBeProblemAsync(HttpStatusCode.Locked, "ACCOUNT_LOCKED");
        problem.GetProperty("title").GetString().ShouldBe("Too many failed attempts. Your account is temporarily locked.");
        problem.GetProperty("retryAfterSec").GetInt32().ShouldBe(15 * 60);
        sixth.Headers.RetryAfter!.Delta.ShouldBe(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public async Task A_locked_account_refuses_the_right_password_too()
    {
        var email = AuthApi.NewEmail();
        await Api.SignupNewUserAsync(email);
        await FailLoginsAsync(email, 5);
        _host.Clock.Advance(TimeSpan.FromMinutes(5));

        var response = await Api.LoginAsync(email, AuthApi.Password);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.Locked, "ACCOUNT_LOCKED");
        problem.GetProperty("retryAfterSec").GetInt32().ShouldBe(10 * 60);
        AuthApi.RefreshSetCookie(response).ShouldBeNull();
    }

    [Fact]
    public async Task The_lock_applies_from_every_IP_address()
    {
        var email = AuthApi.NewEmail();
        await Api.SignupNewUserAsync(email);
        await FailLoginsAsync(email, 5);

        var response = await _host.FromAnotherIp().LoginAsync(email, AuthApi.Password);

        await response.ShouldBeProblemAsync(HttpStatusCode.Locked, "ACCOUNT_LOCKED");
    }

    [Fact]
    public async Task The_lock_ends_after_15_minutes()
    {
        var email = AuthApi.NewEmail();
        await Api.SignupNewUserAsync(email);
        await FailLoginsAsync(email, 5);

        _host.Clock.Advance(TimeSpan.FromMinutes(15) - TimeSpan.FromSeconds(1));
        await (await Api.LoginAsync(email, AuthApi.Password)).ShouldBeProblemAsync(HttpStatusCode.Locked, "ACCOUNT_LOCKED");

        _host.Clock.Advance(TimeSpan.FromSeconds(1));
        (await Api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task After_a_lock_ends_the_count_starts_again()
    {
        var email = AuthApi.NewEmail();
        await Api.SignupNewUserAsync(email);
        await FailLoginsAsync(email, 5);
        _host.Clock.Advance(TimeSpan.FromMinutes(15));

        await FailLoginsAsync(email, 4);

        (await Api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Failures_older_than_15_minutes_no_longer_count()
    {
        var email = AuthApi.NewEmail();
        await Api.SignupNewUserAsync(email);
        await FailLoginsAsync(email, 4);
        _host.Clock.Advance(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(1));

        await FailLoginsAsync(email, 4);

        (await Api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Failures_from_different_IP_addresses_are_counted_separately()
    {
        var email = AuthApi.NewEmail();
        await Api.SignupNewUserAsync(email);
        var otherIp = _host.FromAnotherIp();

        await FailLoginsAsync(email, 4);
        for (var attempt = 1; attempt <= 4; attempt++)
        {
            await otherIp.LoginAsync(email, WrongPassword);
        }

        (await Api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_successful_login_resets_the_count()
    {
        var email = AuthApi.NewEmail();
        await Api.SignupNewUserAsync(email);
        await FailLoginsAsync(email, 4);
        (await Api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);

        await FailLoginsAsync(email, 4);

        (await Api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Locking_sets_locked_until_emails_the_owner_and_is_audited()
    {
        var email = AuthApi.NewEmail();
        var user = await Api.SignupNewUserAsync(email);

        await FailLoginsAsync(email, 5);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var stored = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
                .SingleAsync(u => u.Id == user.Body.User.Id, TestContext.Current.CancellationToken);
            stored.LockedUntil.ShouldNotBeNull();
            (stored.LockedUntil.Value - _host.Clock.GetUtcNow()).ShouldBe(TimeSpan.FromMinutes(15), tolerance: TimeSpan.FromMilliseconds(1));
        }

        (await _outbox.ForAsync(email)).Count(m => m.Template == "account_locked").ShouldBe(1);
        var actions = await _audit.ActionsForAsync(user.Body.User.Id);
        actions.Count(a => a == "auth.login_failed").ShouldBe(5);
        actions.ShouldContain("auth.account_locked");
    }

    [Fact]
    public async Task An_unknown_email_gets_the_same_423_after_five_failures()
    {
        var email = AuthApi.NewEmail();

        await FailLoginsAsync(email, 5);
        var sixth = await Api.LoginAsync(email, WrongPassword);

        var problem = await sixth.ShouldBeProblemAsync(HttpStatusCode.Locked, "ACCOUNT_LOCKED");
        problem.GetProperty("retryAfterSec").GetInt32().ShouldBe(15 * 60);
    }

    [Fact]
    public async Task The_51st_login_from_one_IP_address_gets_429_with_retry_after()
    {
        // Different emails each time, so only the per-IP limit applies.
        for (var attempt = 1; attempt <= 50; attempt++)
        {
            (await Api.LoginAsync(AuthApi.NewEmail(), WrongPassword)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        var limited = await Api.LoginAsync(AuthApi.NewEmail(), WrongPassword);

        var problem = await limited.ShouldBeProblemAsync(HttpStatusCode.TooManyRequests, "RATE_LIMITED");
        problem.GetProperty("retryAfterSec").GetInt32().ShouldBe(15 * 60);
        limited.Headers.RetryAfter!.Delta.ShouldBe(TimeSpan.FromMinutes(15));

        // Another IP is unaffected, and this one recovers once its oldest attempts leave the window.
        (await _host.FromAnotherIp().LoginAsync(AuthApi.NewEmail(), WrongPassword)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        _host.Clock.Advance(TimeSpan.FromMinutes(15));
        (await Api.LoginAsync(AuthApi.NewEmail(), WrongPassword)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task FailLoginsAsync(string email, int count)
    {
        for (var attempt = 1; attempt <= count; attempt++)
        {
            (await Api.LoginAsync(email, WrongPassword)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
    }
}
