using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Auth;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Auth;

public sealed class SignupTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);

    [Fact]
    public async Task Signup_creates_a_free_unverified_candidate_and_signs_them_in()
    {
        var email = AuthApi.NewEmail();

        var response = await _api.SignupAsync(new { email, password = AuthApi.Password, track = "PN", displayName = "  Ada  " });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(AuthApi.Json, TestContext.Current.CancellationToken);
        body.ShouldNotBeNull();
        body.User.Email.ShouldBe(email);
        body.User.EmailVerified.ShouldBeFalse();
        body.User.Role.ShouldBe(UserRole.Candidate);
        body.User.Track.ShouldBe(ExamTrack.PN);
        body.User.PlanTier.ShouldBe(PlanTier.Free);
        body.User.DisplayName.ShouldBe("Ada");
        body.AccessToken.ShouldNotBeNullOrWhiteSpace();
        body.AccessTokenExpiresAt.ShouldBeInRange(DateTimeOffset.UtcNow.AddMinutes(14), DateTimeOffset.UtcNow.AddMinutes(15));
    }

    [Fact]
    public async Task Signup_sets_the_refresh_cookie_with_the_attributes_from_the_spec()
    {
        var response = await _api.SignupAsync(new { email = AuthApi.NewEmail(), password = AuthApi.Password, track = "RN" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var cookie = AuthApi.RefreshSetCookie(response);
        cookie.ShouldNotBeNull();
        var attributes = cookie.Split("; ").Skip(1).Select(a => a.ToLowerInvariant()).ToList();
        attributes.ShouldBe(
            ["max-age=2592000", "path=/api/v1/auth", "secure", "samesite=strict", "httponly"],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Signup_never_returns_the_refresh_token_in_the_body()
    {
        var response = await _api.SignupAsync(new { email = AuthApi.NewEmail(), password = AuthApi.Password, track = "RN" });
        var token = AuthApi.RefreshTokenFrom(response);

        var raw = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        raw.ShouldNotContain(token);
        raw.ShouldNotContain("refresh", Case.Insensitive);
    }

    [Fact]
    public async Task Signup_stores_an_argon2id_hash_and_not_the_password()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == email, TestContext.Current.CancellationToken);

        user.PasswordHash.ShouldStartWith("$argon2id$v=19$m=65536,t=3,p=1$");
        user.PasswordHash.ShouldNotContain(AuthApi.Password);
    }

    [Fact]
    public async Task Signup_with_an_existing_email_in_any_case_returns_409_email_taken()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);

        var response = await _api.SignupAsync(new { email = email.ToUpperInvariant(), password = AuthApi.Password, track = "RN" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "EMAIL_TAKEN");
        AuthApi.RefreshSetCookie(response).ShouldBeNull();
    }

    [Fact]
    public async Task Signup_with_invalid_fields_returns_400_with_a_code_per_field()
    {
        var response = await _api.SignupAsync(new { email = "not-an-email", password = "short" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe(
            [("email", "INVALID_FORMAT"), ("password", "TOO_SHORT"), ("track", "REQUIRED")],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Signup_with_an_unknown_track_returns_400_invalid_value()
    {
        var response = await _api.SignupAsync(new { email = AuthApi.NewEmail(), password = AuthApi.Password, track = "XN" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("track", "INVALID_VALUE")]);
    }
}
