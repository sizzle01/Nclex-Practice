using Nclex.Application.Common;

namespace Nclex.Application.Catalog;

/// <summary>Errors returned by the catalog endpoints and by anything that checks access to sets.</summary>
public static class CatalogErrors
{
    /// <summary>The set does not exist, is not published, or is someone else's custom set.</summary>
    public static readonly AppError TestSetNotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That test set was not found.");

    /// <summary>A free account tried to use a premium set or bank (api.md section 1).</summary>
    public static readonly AppError PlanUpgradeRequired = new(
        "PLAN_UPGRADE_REQUIRED",
        AppErrorKind.Forbidden,
        "This content needs a premium plan.");
}
