using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Auth;

public sealed class LoginTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);

    [Fact]
    public async Task Login_with_the_right_password_returns_200_with_a_new_session()
    {
        var email = AuthApi.NewEmail();
        var signup = await _api.SignupNewUserAsync(email);

        var response = await _api.LoginAsync(email, AuthApi.Password);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var login = await AuthApi.SignedIn.FromAsync(response);
        login.Body.User.Id.ShouldBe(signup.Body.User.Id);
        login.RefreshToken.ShouldNotBe(signup.RefreshToken);
    }

    [Fact]
    public async Task Login_ignores_the_case_of_the_email()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);

        var response = await _api.LoginAsync(email.ToUpperInvariant(), AuthApi.Password);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_with_a_wrong_password_returns_401_invalid_credentials()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);

        var response = await _api.LoginAsync(email, "wrong-password-123");

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        AuthApi.RefreshSetCookie(response).ShouldBeNull();
    }

    [Fact]
    public async Task Unknown_email_and_wrong_password_get_the_same_answer()
    {
        var email = AuthApi.NewEmail();
        await _api.SignupNewUserAsync(email);

        var wrongPassword = await (await _api.LoginAsync(email, "wrong-password-123"))
            .ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        var unknownEmail = await (await _api.LoginAsync(AuthApi.NewEmail(), "wrong-password-123"))
            .ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");

        unknownEmail.GetProperty("title").GetString().ShouldBe(wrongPassword.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Login_with_missing_fields_returns_400()
    {
        var response = await _api.Client.PostAsJsonAsync("/api/v1/auth/login", new { email = "" }, TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("email", "REQUIRED"), ("password", "REQUIRED")], ignoreOrder: true);
    }

    [Fact]
    public async Task Malformed_json_returns_400_validation_failed()
    {
        using var content = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json");

        var response = await _api.Client.PostAsync("/api/v1/auth/login", content, TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.GetProperty("errors").ValueKind.ShouldBe(JsonValueKind.Array);
    }
}
