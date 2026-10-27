using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Users;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;
using Nclex.Infrastructure.Security;

namespace Nclex.Api.IntegrationTests.Users;

public sealed class MeTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);

    [Fact]
    public async Task Get_me_returns_the_signed_in_users_profile()
    {
        var signedIn = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", signedIn.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await ReadProfileAsync(response);
        profile.ShouldBe(signedIn.Body.User);
        profile.Timezone.ShouldBe("UTC");
    }

    [Fact]
    public async Task Get_me_without_a_token_returns_401_unauthenticated()
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", accessToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Get_me_with_an_expired_access_token_returns_401()
    {
        var signedIn = await _api.SignupNewUserAsync();
        var user = await LoadUserAsync(signedIn.Body.User.Id);
        // Control: a token issued now by the same issuer works, so the 401 below is due to expiry alone.
        var fresh = IssueToken(user, DateTimeOffset.UtcNow);
        var expired = IssueToken(user, DateTimeOffset.UtcNow.AddMinutes(-20));

        (await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", fresh)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", expired);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Get_me_with_a_token_signed_by_another_key_returns_401()
    {
        var signedIn = await _api.SignupNewUserAsync();
        var user = await LoadUserAsync(signedIn.Body.User.Id);
        var forged = IssueToken(user, DateTimeOffset.UtcNow, signingKey: "an-attackers-key-0123456789abcdef-0123");

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", forged);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Patch_me_updates_the_given_fields_and_keeps_the_rest()
    {
        var signedIn = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(
            HttpMethod.Patch,
            "/api/v1/me",
            signedIn.AccessToken,
            new { track = "PN", timezone = "America/Chicago", targetExamDate = "2027-03-01" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await ReadProfileAsync(response);
        profile.Track.ShouldBe(ExamTrack.PN);
        profile.Timezone.ShouldBe("America/Chicago");
        profile.TargetExamDate.ShouldBe(new DateOnly(2027, 3, 1));
        profile.DisplayName.ShouldBe("Test");

        var reloaded = await ReadProfileAsync(await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", signedIn.AccessToken));
        reloaded.ShouldBe(profile);
    }

    [Fact]
    public async Task Patch_me_with_an_empty_display_name_removes_it()
    {
        var signedIn = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(HttpMethod.Patch, "/api/v1/me", signedIn.AccessToken, new { displayName = "" });

        (await ReadProfileAsync(response)).DisplayName.ShouldBeNull();
    }

    [Fact]
    public async Task Patch_me_with_invalid_values_returns_400_with_field_codes()
    {
        var signedIn = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(
            HttpMethod.Patch,
            "/api/v1/me",
            signedIn.AccessToken,
            new { displayName = new string('a', 81), timezone = "Mars/Olympus_Mons" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("displayName", "TOO_LONG"), ("timezone", "INVALID_VALUE")], ignoreOrder: true);
    }

    [Fact]
    public async Task Patch_me_without_a_token_returns_401()
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Patch, "/api/v1/me", accessToken: null, new { displayName = "x" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    private static async Task<UserProfile> ReadProfileAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<UserProfile>(AuthApi.Json, TestContext.Current.CancellationToken))!;

    private async Task<User> LoadUserAsync(Guid id)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
            .SingleAsync(u => u.Id == id, TestContext.Current.CancellationToken);
    }

    private static string IssueToken(User user, DateTimeOffset issuedAt, string signingKey = ApiFactory.JwtSigningKey)
    {
        var options = Options.Create(new JwtOptions { Issuer = "nclex-api", Audience = "nclex-web", SigningKey = signingKey });
        return new JwtAccessTokenIssuer(options, new FixedTimeProvider(issuedAt)).Issue(user, sessionFamilyId: Guid.NewGuid()).Value;
    }
}
