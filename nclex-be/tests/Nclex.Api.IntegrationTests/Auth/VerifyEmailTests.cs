using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Users;
using Nclex.Domain.Email;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Auth;

public sealed class VerifyEmailTests(ApiFactory factory)
{
    private const string VerifyPath = "/api/v1/auth/verify-email";

    private readonly AuthApi _api = new(factory);
    private readonly OutboxProbe _outbox = new(factory);

    [Fact]
    public async Task Signup_queues_one_verification_email_in_the_same_transaction()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);

        var message = (await _outbox.ForAsync(email)).ShouldHaveSingleItem();
        message.Template.ShouldBe("verify_email");
        message.UserId.ShouldBe(signedIn.Body.User.Id);
        message.Status.ShouldBe(OutboxStatus.Pending);
        signedIn.Body.User.EmailVerified.ShouldBeFalse();
    }

    [Fact]
    public async Task Signup_stores_only_the_hash_of_the_verification_token()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");

        var stored = (await TokensForAsync(signedIn.Body.User.Id)).ShouldHaveSingleItem();

        stored.Purpose.ShouldBe(EmailTokenPurpose.VerifyEmail);
        stored.TokenHash.ShouldBe(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        stored.ExpiresAt.ShouldBe(stored.CreatedAt.AddHours(24), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Verifying_with_the_emailed_token_marks_the_email_verified()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");

        var response = await _api.PostAsync(VerifyPath, new { token });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await GetMeAsync(signedIn.AccessToken)).EmailVerified.ShouldBeTrue();
    }

    [Fact]
    public async Task A_token_works_only_once()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");
        await _api.PostAsync(VerifyPath, new { token });

        var second = await _api.PostAsync(VerifyPath, new { token });

        await second.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
    }

    [Fact]
    public async Task An_expired_token_is_rejected()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");
        await ExpireTokensAsync(signedIn.Body.User.Id);

        var response = await _api.PostAsync(VerifyPath, new { token });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
        (await GetMeAsync(signedIn.AccessToken)).EmailVerified.ShouldBeFalse();
    }

    [Fact]
    public async Task An_unknown_token_is_rejected()
    {
        var response = await _api.PostAsync(VerifyPath, new { token = "not-a-real-token" });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
    }

    [Fact]
    public async Task A_missing_token_fails_validation()
    {
        var response = await _api.PostAsync(VerifyPath, new { });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("token", "REQUIRED")]);
    }

    [Fact]
    public async Task Resending_makes_the_earlier_link_stop_working()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);
        var first = await _outbox.LatestTokenAsync(email, "verify_email");
        await _api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/auth/verify-email/resend", signedIn.AccessToken);
        var second = await _outbox.LatestTokenAsync(email, "verify_email");

        await (await _api.PostAsync(VerifyPath, new { token = first })).ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
        (await _api.PostAsync(VerifyPath, new { token = second })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task A_password_reset_token_cannot_verify_an_email()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        await _api.PostAsync("/api/v1/auth/password/forgot", new { email });
        var resetToken = await _outbox.LatestTokenAsync(email, "reset_password");

        var response = await _api.PostAsync(VerifyPath, new { token = resetToken });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "INVALID_TOKEN");
    }

    [Fact]
    public async Task Two_simultaneous_uses_of_one_token_let_exactly_one_succeed()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");

        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => _api.PostAsync(VerifyPath, new { token })));

        responses.Count(r => r.StatusCode == HttpStatusCode.NoContent).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest).ShouldBe(4);
    }

    private async Task<UserProfile> GetMeAsync(string accessToken)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", accessToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserProfile>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task<List<EmailToken>> TokensForAsync(Guid userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.EmailTokens.Where(t => t.UserId == userId).ToListAsync(TestContext.Current.CancellationToken);
    }

    private async Task ExpireTokensAsync(Guid userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlAsync(
            $"UPDATE email_tokens SET expires_at = now() - interval '1 minute' WHERE user_id = {userId}",
            TestContext.Current.CancellationToken);
    }
}
