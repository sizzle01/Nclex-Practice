namespace Nclex.Api.Errors;

/// <summary>
/// Machine-readable error codes returned in the <c>code</c> field of every error body.
/// The list and meanings come from api.md section 1; features add their codes as they need them.
/// </summary>
public static class ErrorCodes
{
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string Unauthenticated = "UNAUTHENTICATED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string RateLimited = "RATE_LIMITED";

    // Not in api.md: used for unexpected server errors, which the design does not list.
    public const string InternalError = "INTERNAL_ERROR";

    private const string TypeBaseUri = "https://errors.nclex-practice.app/";

    /// <summary>Builds the <c>type</c> URI for a code, e.g. NOT_FOUND becomes .../not-found.</summary>
    public static string TypeUri(string code) => TypeBaseUri + code.ToLowerInvariant().Replace('_', '-');

    /// <summary>The default code for errors produced by the framework itself (404 routes, 401 challenges...).</summary>
    public static string? ForStatus(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => ValidationFailed,
        StatusCodes.Status401Unauthorized => Unauthenticated,
        StatusCodes.Status403Forbidden => Forbidden,
        StatusCodes.Status404NotFound => NotFound,
        StatusCodes.Status429TooManyRequests => RateLimited,
        StatusCodes.Status500InternalServerError => InternalError,
        _ => null,
    };
}
