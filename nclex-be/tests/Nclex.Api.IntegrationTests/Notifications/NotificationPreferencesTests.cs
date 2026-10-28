using System.Net;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Notifications;

/// <summary>GET and PUT /me/notification-preferences (api.md section 6).</summary>
public sealed class NotificationPreferencesTests(ApiFactory factory)
{
    private readonly AuthApi _auth = new(factory);
    private readonly NotificationsApi _notifications = new(new AuthApi(factory));

    [Fact]
    public async Task A_new_account_starts_with_the_default_preferences()
    {
        var user = await _auth.SignupNewUserAsync();

        var body = await AttemptsApi.JsonOkAsync(await _notifications.PreferencesAsync(user.AccessToken));

        Rows(body).ShouldBe(
        [
            "results|True|True|False",
            "reminders|True|False|False",
            "engagement|True|False|False",
            "content|True|False|False",
            "security|True|True|True",
        ]);
    }

    [Fact]
    public async Task Preferences_can_be_replaced()
    {
        var user = await _auth.SignupNewUserAsync();

        var put = await AttemptsApi.JsonOkAsync(
            await _notifications.ReplacePreferencesAsync(user.AccessToken, NotificationsApi.Preferences(inAppResults: false, "reminders")));
        var get = await AttemptsApi.JsonOkAsync(await _notifications.PreferencesAsync(user.AccessToken));

        Rows(get).ShouldBe(Rows(put));
        Rows(get).ShouldBe(
        [
            "results|False|False|False",
            "reminders|True|True|False",
            "engagement|True|False|False",
            "content|True|False|False",
            "security|True|True|True",
        ]);
    }

    [Fact]
    public async Task Security_cannot_be_switched_off()
    {
        var user = await _auth.SignupNewUserAsync();
        var body = new
        {
            preferences = new[] { "results", "reminders", "engagement", "content", "security" }
                .Select(category => new { category, inApp = true, email = category != "security" }),
        };

        var problem = await (await _notifications.ReplacePreferencesAsync(user.AccessToken, body))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");

        problem.GetProperty("errors")[0].GetProperty("code").GetString().ShouldBe("INVALID_VALUE");
    }

    [Fact]
    public async Task Every_category_must_be_sent()
    {
        var user = await _auth.SignupNewUserAsync();
        var body = new { preferences = new[] { new { category = "results", inApp = true, email = false } } };

        await (await _notifications.ReplacePreferencesAsync(user.AccessToken, body))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    private static List<string> Rows(JsonElement body) =>
    [
        .. body.GetProperty("preferences").EnumerateArray().Select(p =>
            $"{p.GetProperty("category").GetString()}|{p.GetProperty("inApp").GetBoolean()}|{p.GetProperty("email").GetBoolean()}|{p.GetProperty("locked").GetBoolean()}"),
    ];
}
