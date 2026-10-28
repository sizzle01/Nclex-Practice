using Nclex.Domain.Notifications;

namespace Nclex.Domain.Tests.Notifications;

public sealed class NotificationPreferenceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void A_new_account_gets_one_row_per_category_with_the_default_channels()
    {
        var rows = NotificationPreference.DefaultsFor(UserId);

        rows.Select(r => (r.Category, r.InApp, r.Email)).ShouldBe(
        [
            (NotificationCategory.Results, true, true),
            (NotificationCategory.Reminders, true, false),
            (NotificationCategory.Engagement, true, false),
            (NotificationCategory.Content, true, false),
            (NotificationCategory.Security, true, true),
        ]);
        rows.ShouldAllBe(r => r.UserId == UserId);
    }

    [Fact]
    public void A_category_can_be_switched_off()
    {
        var results = NotificationPreference.DefaultsFor(UserId).Single(r => r.Category == NotificationCategory.Results);

        results.Change(inApp: false, email: false);

        (results.InApp, results.Email).ShouldBe((false, false));
    }

    [Fact]
    public void Security_stays_on_whatever_is_asked()
    {
        var security = NotificationPreference.DefaultsFor(UserId).Single(r => r.Category == NotificationCategory.Security);

        security.Change(inApp: false, email: false);

        (security.InApp, security.Email, security.IsLocked).ShouldBe((true, true, true));
    }
}
