using System.Net;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Users;

public sealed class ChangePasswordTests(ApiFactory factory)
{
    private const string Path = "/api/v1/me/password";
    private const string NewPassword = "a-brand-new-password";

    private readonly AuthApi _api = new(factory);
    private readonly OutboxProbe _outbox = new(factory);
    private readonly AuditProbe _audit = new(factory);

    [Fact]
    public async Task The_new_password_works_and_the_old_one_does_not()
    {
        var email = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync(email);

        var response = await ChangeAsync(user.AccessToken, AuthApi.Password, NewPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _api.LoginAsync(email, AuthApi.Password)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await _api.LoginAsync(email, NewPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task This_device_stays_signed_in_and_every_other_device_is_signed_out()
    {
        var email = AuthApi.NewEmail();
        var laptop = await _api.SignupNewUserAsync(email);
        var phone = await _api.LoginOkAsync(email);

        await ChangeAsync(laptop.AccessToken, AuthApi.Password, NewPassword);

        (await _api.RefreshAsync(laptop.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.OK);
        await (await _api.RefreshAsync(phone.RefreshToken)).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task The_owner_is_emailed_and_the_change_is_audited()
    {
        var email = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync(email);

        await ChangeAsync(user.AccessToken, AuthApi.Password, NewPassword);

        (await _outbox.ForAsync(email)).Count(m => m.Template == "password_changed").ShouldBe(1);
        (await _audit.ActionsForAsync(user.Body.User.Id)).ShouldContain("auth.password_changed");
    }

    [Fact]
    public async Task The_new_password_follows_the_sign_up_rules()
    {
        var user = await _api.SignupNewUserAsync();

        var response = await ChangeAsync(user.AccessToken, currentPassword: "", newPassword: "short");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("currentPassword", "REQUIRED"), ("newPassword", "TOO_SHORT")], ignoreOrder: true);
    }

    private Task<HttpResponseMessage> ChangeAsync(string accessToken, string currentPassword, string newPassword) =>
        _api.SendWithTokenAsync(HttpMethod.Post, Path, accessToken, new { currentPassword, newPassword });
}
