namespace Nclex.Application.Auth.ConfirmEmailChange;

/// <summary>Body of POST /auth/email/confirm-change: the token from the link sent to the new address.</summary>
public sealed record ConfirmEmailChangeRequest(string Token);
