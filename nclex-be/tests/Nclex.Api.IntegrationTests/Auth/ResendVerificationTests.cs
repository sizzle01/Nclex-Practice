using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Auth;

public sealed class ResendVerificationTests(ApiFactory factory)
{
    private const string ResendPath = "/api/v1/auth/verify-email/resend";

    private readonly AuthApi _api = new(factory);
    private readonly OutboxProbe _outbox = new(factory);

    [Fact]
    public async Task Resend_queues_another_verification_email()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);

        var response = await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, signedIn.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _outbox.ForAsync(email)).Count(m => m.Template == "verify_email").ShouldBe(2);
    }

    [Fact]
    public async Task The_fourth_email_in_an_hour_is_rate_limited_with_retry_after()
    {
        // Sign-up sends the first email, so two resends reach the limit of three.
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);
        await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, signedIn.AccessToken);
        await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, signedIn.AccessToken);

        var response = await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, signedIn.AccessToken);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.TooManyRequests, "RATE_LIMITED");
        var retryAfter = int.Parse(response.Headers.GetValues("Retry-After").Single(), System.Globalization.CultureInfo.InvariantCulture);
        retryAfter.ShouldBeInRange(3500, 3600);
        problem.GetProperty("retryAfterSec").GetInt32().ShouldBe(retryAfter);
        (await _outbox.ForAsync(email)).Count.ShouldBe(3);
    }

    [Fact]
    public async Task Emails_older_than_an_hour_do_not_count_towards_the_limit()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);
        await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, signedIn.AccessToken);
        await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, signedIn.AccessToken);
        await AgeTokensAsync(signedIn.Body.User.Id, TimeSpan.FromMinutes(61));

        var response = await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, signedIn.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Resend_does_nothing_once_the_email_is_verified()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");
        await _api.PostAsync("/api/v1/auth/verify-email", new { token });

        var response = await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, signedIn.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _outbox.ForAsync(email)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task Resend_needs_an_access_token()
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Post, ResendPath, accessToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    private async Task AgeTokensAsync(Guid userId, TimeSpan age)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlAsync(
            $"UPDATE email_tokens SET created_at = created_at - {age} WHERE user_id = {userId}",
            TestContext.Current.CancellationToken);
    }
}
