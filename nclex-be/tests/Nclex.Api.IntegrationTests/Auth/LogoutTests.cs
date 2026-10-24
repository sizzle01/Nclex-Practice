using System.Net;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Auth;

public sealed class LogoutTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);

    [Fact]
    public async Task Logout_revokes_the_session_and_clears_the_cookie()
    {
        var signedIn = await _api.SignupNewUserAsync();

        var response = await _api.LogoutAsync(signedIn.RefreshToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        AuthApi.RefreshSetCookie(response).ShouldNotBeNull().ShouldStartWith("rt=;");
        (await _api.RefreshAsync(signedIn.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_after_a_rotation_revokes_the_current_token()
    {
        var signedIn = await _api.SignupNewUserAsync();
        var current = AuthApi.RefreshTokenFrom(await _api.RefreshAsync(signedIn.RefreshToken));

        await _api.LogoutAsync(current);

        (await _api.RefreshAsync(current)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_with_an_unknown_token_still_succeeds()
    {
        var response = await _api.LogoutAsync("not-a-real-token");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_all_revokes_every_device()
    {
        var email = AuthApi.NewEmail();
        var laptop = await _api.SignupNewUserAsync(email);
        var phone = await _api.LoginOkAsync(email);

        var response = await _api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/auth/logout-all", laptop.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _api.RefreshAsync(laptop.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await _api.RefreshAsync(phone.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_all_does_not_touch_other_users()
    {
        var mine = await _api.SignupNewUserAsync();
        var someoneElse = await _api.SignupNewUserAsync();

        await _api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/auth/logout-all", mine.AccessToken);

        (await _api.RefreshAsync(someoneElse.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_all_without_an_access_token_returns_401()
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/auth/logout-all", accessToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }
}
