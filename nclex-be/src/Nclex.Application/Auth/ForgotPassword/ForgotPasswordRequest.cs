namespace Nclex.Application.Auth.ForgotPassword;

/// <summary>Body of POST /auth/password/forgot.</summary>
public sealed record ForgotPasswordRequest(string Email);
