using Nclex.Application.Common;

namespace Nclex.Application.WeakAreas;

/// <summary>Errors returned by the weak-area practice endpoints (api.md section 1).</summary>
public static class WeakAreaErrors
{
    /// <summary>
    /// No Client Needs leaf is under 65% with at least 5 items in the last 10 attempts, so there is
    /// nothing to practise; the app says so rather than inventing a weakness.
    /// </summary>
    public static readonly AppError NoWeakAreas = new(
        "NO_WEAK_AREAS",
        AppErrorKind.Unprocessable,
        "You have no weak areas in your recent tests.");
}
