using System.Net;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Auth;

public sealed class VerifiedEmailPolicyTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);
    private readonly OutboxProbe _outbox = new(factory);

    [Fact]
    public async Task An_unverified_user_gets_403_email_not_verified()
    {
        var signedIn = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, VerifiedOnlyController.Path, signedIn.AccessToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "EMAIL_NOT_VERIFIED");
    }

    [Fact]
    public async Task Verifying_lets_the_same_access_token_through_straight_away()
    {
        var email = AuthApi.NewEmail();
        var signedIn = await _api.SignupNewUserAsync(email);
        var token = await _outbox.LatestTokenAsync(email, "verify_email");
        await _api.PostAsync("/api/v1/auth/verify-email", new { token });

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, VerifiedOnlyController.Path, signedIn.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task No_access_token_still_gets_401()
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, VerifiedOnlyController.Path, accessToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }
}
