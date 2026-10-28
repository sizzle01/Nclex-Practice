using System.Globalization;

namespace Nclex.Domain.Notifications;

/// <summary>The title and body shown in the bell and the feed.</summary>
public sealed record NotificationText(string Title, string Body);

/// <summary>
/// The wording of each notification (interfaces.md 5.6). Pure functions, so the copy can be tested
/// without a database. The security titles beyond "Your password was changed" and "New sign-in"
/// are not in the design and were added for F12 (see the F12 walkthrough).
/// </summary>
public static class NotificationCopy
{
    public static NotificationText ResultPass(decimal scorePct, string setTitle) => new(
        "You passed your mock exam — great work!",
        $"You scored {Percent(scorePct)}% on {setTitle}. Review your rationales to lock it in.");

    /// <param name="weakest">Names of the weakest categories, worst first; the first two are named.</param>
    public static NotificationText ResultFail(decimal scorePct, IReadOnlyList<string> weakest)
    {
        var focus = weakest.Count == 0
            ? "Review your rationales to see what to focus on."
            : $"Here's what to focus on: {string.Join(", ", weakest.Take(2))}.";
        return new("You didn't reach the passing threshold this time", $"You scored {Percent(scorePct)}%. {focus}");
    }

    public static NotificationText ResultReady(decimal scorePct, string setTitle) => new(
        "Your results are ready",
        $"You scored {Percent(scorePct)}% on {setTitle}.");

    /// <param name="answeredShare">Share of items answered, 0 to 1.</param>
    /// <param name="timeLeftMs">Exam time left; null for an untimed set, which drops the "left" clause.</param>
    public static NotificationText DraftReminder(string setTitle, decimal answeredShare, long? timeLeftMs)
    {
        var timeLeft = timeLeftMs is { } ms ? $" with {Duration(ms)} left" : string.Empty;
        return new("Pick up where you left off", $"{setTitle} is {Percent(answeredShare)}% done{timeLeft}.");
    }

    public static NotificationText PasswordChanged() => new(
        "Your password was changed",
        "Your password was just changed. Not you? Reset your password now to take your account back.");

    public static NotificationText NewSignIn(string? browser, string? ipAddress) => new(
        "New sign-in",
        $"Your account was signed in to from a new browser ({browser ?? "unknown browser"}, IP address {ipAddress ?? "unknown"}). "
        + "Not you? Reset your password now.");

    public static NotificationText AccountLocked(DateTimeOffset lockedUntil) => new(
        "Your account was temporarily locked",
        $"After several failed sign-in attempts, signing in is paused until {Time(lockedUntil)}. "
        + "Not you? Reset your password.");

    public static NotificationText EmailChangeRequested(string newEmail) => new(
        "Someone asked to change your email",
        $"A request was made to move your account to {newEmail}. Nothing changes until the link sent there is used. "
        + "Not you? Change your password.");

    public static NotificationText EmailChanged(string newEmail) => new(
        "Your email address was changed",
        $"Your account now signs in with {newEmail}. Not you? Reset your password.");

    /// <summary>A moderator resolved the user's report; the body is the moderator's note as written (F15).</summary>
    public static NotificationText FlagResolved(string resolutionNote) => new(
        "Thanks — we reviewed your report",
        resolutionNote);

    /// <summary>A 0 to 1 share as a whole percentage: 0.585 is "59".</summary>
    public static string Percent(decimal share) =>
        Math.Round(share * 100, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture);

    /// <summary>Time left in words: "2 h 5 min", "12 min", or "less than a minute".</summary>
    public static string Duration(long milliseconds)
    {
        var minutes = (long)TimeSpan.FromMilliseconds(Math.Max(0, milliseconds)).TotalMinutes;
        if (minutes < 1)
        {
            return "less than a minute";
        }

        var (hours, rest) = (minutes / 60, minutes % 60);
        return hours switch
        {
            0 => $"{rest} min",
            _ when rest == 0 => $"{hours} h",
            _ => $"{hours} h {rest} min",
        };
    }

    // UTC, as in the security emails, since the user's time zone is not at hand here.
    private static string Time(DateTimeOffset time) =>
        time.ToUniversalTime().ToString("d MMM yyyy 'at' HH:mm 'UTC'", CultureInfo.InvariantCulture);
}
