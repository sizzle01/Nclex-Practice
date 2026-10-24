using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Auth;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Auth;

public sealed class RefreshTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);

    [Fact]
    public async Task Refresh_rotates_the_cookie_and_returns_a_working_access_token()
    {
        var signedIn = await _api.SignupNewUserAsync();

        var response = await _api.RefreshAsync(signedIn.RefreshToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(AuthApi.Json, TestContext.Current.CancellationToken);
        AuthApi.RefreshTokenFrom(response).ShouldNotBe(signedIn.RefreshToken);
        var me = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me", body!.AccessToken);
        me.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task The_rotated_token_can_itself_be_refreshed()
    {
        var signedIn = await _api.SignupNewUserAsync();
        var second = AuthApi.RefreshTokenFrom(await _api.RefreshAsync(signedIn.RefreshToken));

        var response = await _api.RefreshAsync(second);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_without_the_x_requested_with_header_returns_401()
    {
        var signedIn = await _api.SignupNewUserAsync();

        var response = await _api.RefreshAsync(signedIn.RefreshToken, withRequestedWithHeader: false);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
        // The token was not consumed, so the real client can still use it.
        (await _api.RefreshAsync(signedIn.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_without_a_cookie_returns_401()
    {
        var response = await _api.RefreshAsync(refreshToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Refresh_with_an_unknown_token_returns_401_and_clears_the_cookie()
    {
        var response = await _api.RefreshAsync("not-a-real-token");

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
        AuthApi.RefreshSetCookie(response).ShouldNotBeNull().ShouldStartWith("rt=;");
    }

    [Fact]
    public async Task Reusing_a_rotated_token_revokes_the_whole_family()
    {
        var signedIn = await _api.SignupNewUserAsync();
        var successor = AuthApi.RefreshTokenFrom(await _api.RefreshAsync(signedIn.RefreshToken));

        // An attacker replays the old token...
        var replay = await _api.RefreshAsync(signedIn.RefreshToken);
        await replay.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");

        // ...so the legitimate successor stops working too: everyone in this family must log in again.
        var afterReuse = await _api.RefreshAsync(successor);
        await afterReuse.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Reuse_in_one_family_does_not_sign_out_other_devices()
    {
        var email = AuthApi.NewEmail();
        var laptop = await _api.SignupNewUserAsync(email);
        var phone = await _api.LoginOkAsync(email);
        await _api.RefreshAsync(laptop.RefreshToken);

        await _api.RefreshAsync(laptop.RefreshToken); // reuse on the laptop's family

        (await _api.RefreshAsync(phone.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_with_an_expired_token_returns_401()
    {
        var signedIn = await _api.SignupNewUserAsync();
        await ExpireSessionsAsync(signedIn.Body.User.Id);

        var response = await _api.RefreshAsync(signedIn.RefreshToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    private async Task ExpireSessionsAsync(Guid userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlAsync(
            $"UPDATE refresh_sessions SET expires_at = now() - interval '1 minute' WHERE user_id = {userId}",
            TestContext.Current.CancellationToken);
    }
}
