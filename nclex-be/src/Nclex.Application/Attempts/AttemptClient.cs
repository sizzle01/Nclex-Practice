using Nclex.Application.Common;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts;

/// <summary>
/// The <c>X-Attempt-Client</c> header: the id of the player page that holds the attempt's device
/// lease (api.md section 1, architecture.md 5.2). Every attempt write sends it.
/// </summary>
public static class AttemptClient
{
    public const string HeaderName = "X-Attempt-Client";

    /// <summary>Long enough for a UUID with a prefix; anything longer is not one of ours.</summary>
    public const int MaxLength = 64;

    /// <summary>The header value, or 400 VALIDATION_FAILED when it is missing or too long.</summary>
    public static string Require(string? clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw AppException.ForField(HeaderName, ValidationErrorCodes.Required);
        }

        if (clientId.Length > MaxLength)
        {
            throw AppException.ForField(HeaderName, ValidationErrorCodes.TooLong);
        }

        return clientId;
    }
}
