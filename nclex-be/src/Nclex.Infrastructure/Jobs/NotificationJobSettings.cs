using System.Globalization;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Nclex.Application.Notifications.DraftReminders;

namespace Nclex.Infrastructure.Jobs;

/// <summary>
/// Reads the <c>Notifications</c> section of the worker's settings:
/// <c>DraftReminderAfterDays</c> (default 3) and <c>DraftReminderCron</c> (default hourly).
/// Hourly means a reminder goes out at most an hour after a draft crosses the threshold.
/// </summary>
public static class NotificationJobSettings
{
    public const string SectionName = "Notifications";

    public static DraftReminderSettings DraftReminders(IConfiguration configuration) =>
        configuration[$"{SectionName}:DraftReminderAfterDays"] is { Length: > 0 } days
            ? new DraftReminderSettings(TimeSpan.FromDays(double.Parse(days, CultureInfo.InvariantCulture)))
            : DraftReminderSettings.Default;

    public static string ReminderCron(IConfiguration configuration) =>
        configuration[$"{SectionName}:DraftReminderCron"] is { Length: > 0 } cron ? cron : Cron.Hourly();
}
