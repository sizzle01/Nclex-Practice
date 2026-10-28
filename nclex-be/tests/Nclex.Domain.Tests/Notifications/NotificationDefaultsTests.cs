using Nclex.Domain.Notifications;

namespace Nclex.Domain.Tests.Notifications;

public sealed class NotificationDefaultsTests
{
    private const string InApp = NotificationChannels.InApp;
    private const string Email = NotificationChannels.Email;

    [Theory]
    [InlineData(NotificationCategory.Results, true)]
    [InlineData(NotificationCategory.Security, true)]
    [InlineData(NotificationCategory.Reminders, false)]
    [InlineData(NotificationCategory.Engagement, false)]
    [InlineData(NotificationCategory.Content, false)]
    public void Only_results_and_security_email_by_default(NotificationCategory category, bool email)
    {
        NotificationDefaults.EmailByDefault(category).ShouldBe(email);
    }

    [Fact]
    public void Without_a_preference_row_the_defaults_apply()
    {
        NotificationDefaults.ChannelsFor(NotificationCategory.Results, null, null, hasEmail: true).ShouldBe([InApp, Email]);
        NotificationDefaults.ChannelsFor(NotificationCategory.Reminders, null, null, hasEmail: true).ShouldBe([InApp]);
    }

    [Fact]
    public void The_users_choice_decides_each_channel()
    {
        NotificationDefaults.ChannelsFor(NotificationCategory.Results, inAppPreference: false, emailPreference: true, hasEmail: true)
            .ShouldBe([Email]);
        NotificationDefaults.ChannelsFor(NotificationCategory.Reminders, inAppPreference: true, emailPreference: true, hasEmail: true)
            .ShouldBe([InApp, Email]);
    }

    [Fact]
    public void With_both_channels_off_nothing_is_delivered()
    {
        NotificationDefaults.ChannelsFor(NotificationCategory.Results, false, false, hasEmail: true).ShouldBeEmpty();
    }

    [Fact]
    public void A_notification_without_an_email_is_never_emailed()
    {
        NotificationDefaults.ChannelsFor(NotificationCategory.Results, true, true, hasEmail: false).ShouldBe([InApp]);
    }

    [Fact]
    public void Security_goes_to_both_channels_whatever_the_row_says()
    {
        NotificationDefaults.ChannelsFor(NotificationCategory.Security, false, false, hasEmail: true).ShouldBe([InApp, Email]);
    }
}
