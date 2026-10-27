namespace Nclex.Application.Auth.VerifyEmail;

/// <summary>Body of POST /auth/verify-email: the token from the emailed link.</summary>
public sealed record VerifyEmailRequest(string Token);
