using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Email;

namespace Nclex.Api.IntegrationTests.Notifications;

/// <summary>F03's security events now also reach the bell, while their emails keep going out.</summary>
public sealed class SecurityNotificationTests(ApiFactory factory)
{
    private readonly AuthApi _auth = new(factory);
    private readonly NotificationsApi _notifications = new(new AuthApi(factory));
    private readonly OutboxProbe _outbox = new(factory);

    [Fact]
    public async Task Changing_the_password_notifies_in_app_and_by_email()
    {
        var email = AuthApi.NewEmail();
        var user = await _auth.SignupNewUserAsync(email);

        await _auth.SendWithTokenAsync(
            HttpMethod.Post,
            "/api/v1/me/password",
            user.AccessToken,
            new { currentPassword = AuthApi.Password, newPassword = "a-brand-new-password" });

        var item = (await _notifications.ListOkAsync(user.AccessToken)).GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        item.GetProperty("type").GetString().ShouldBe("security");
        item.GetProperty("category").GetString().ShouldBe("security");
        item.GetProperty("title").GetString().ShouldBe("Your password was changed");
        item.GetProperty("data").GetProperty("deepLink").GetString().ShouldBe("/settings/account");
        (await _outbox.ForAsync(email)).ShouldContain(m => m.Template == EmailTemplateNames.PasswordChanged);
    }

    [Fact]
    public async Task Security_notices_ignore_preferences_stored_before_the_lock()
    {
        var email = AuthApi.NewEmail();
        var user = await _auth.SignupNewUserAsync(email);
        await new NotificationProbe(factory).ExecuteAsync(
            $"UPDATE notification_preferences SET in_app = false, email = false WHERE user_id = {user.Body.User.Id} AND category = 'security'");

        await _auth.SendWithTokenAsync(
            HttpMethod.Post,
            "/api/v1/me/password",
            user.AccessToken,
            new { currentPassword = AuthApi.Password, newPassword = "a-brand-new-password" });

        (await _notifications.UnreadCountAsync(user.AccessToken)).ShouldBe(1);
        (await _outbox.ForAsync(email)).ShouldContain(m => m.Template == EmailTemplateNames.PasswordChanged);
    }
}
