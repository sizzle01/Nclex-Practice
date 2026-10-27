namespace Nclex.Application.Abstractions;

/// <summary>The signed-in user making the current request, as identified by their access token.</summary>
public interface ICurrentUser
{
    /// <summary>The user's id. Only available on endpoints that require authentication.</summary>
    Guid UserId { get; }

    /// <summary>
    /// The refresh-session family this access token was issued for: one sign-in on one device.
    /// Null for tokens issued before F03 added the claim.
    /// </summary>
    Guid? SessionFamilyId { get; }
}
