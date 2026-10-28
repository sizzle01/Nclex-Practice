using Nclex.Application.Common;

namespace Nclex.Application.Performance;

/// <summary>Errors returned by the performance endpoints (api.md section 1).</summary>
public static class PerformanceErrors
{
    public static readonly AppError UserNotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That user was not found.");
}
