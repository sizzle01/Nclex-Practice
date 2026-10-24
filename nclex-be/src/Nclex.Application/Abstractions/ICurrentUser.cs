namespace Nclex.Application.Abstractions;

/// <summary>The signed-in user making the current request, as identified by their access token.</summary>
public interface ICurrentUser
{
    /// <summary>The user's id. Only available on endpoints that require authentication.</summary>
    Guid UserId { get; }
}
