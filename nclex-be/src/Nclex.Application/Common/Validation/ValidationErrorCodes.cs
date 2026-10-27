namespace Nclex.Application.Common.Validation;

/// <summary>
/// Codes for the per-field entries in a VALIDATION_FAILED body: <c>errors: [{ field, code }]</c>
/// (api.md section 1). The frontend maps them to messages.
/// </summary>
public static class ValidationErrorCodes
{
    public const string Required = "REQUIRED";
    public const string InvalidFormat = "INVALID_FORMAT";
    public const string InvalidValue = "INVALID_VALUE";
    public const string TooShort = "TOO_SHORT";
    public const string TooLong = "TOO_LONG";

    /// <summary>The current password given to confirm an account change is wrong.</summary>
    public const string Incorrect = "INCORRECT";

    /// <summary>The new value is the same as the current one, e.g. changing email to the same address.</summary>
    public const string Unchanged = "UNCHANGED";
}
