namespace Nclex.Application.Auth.Login;

/// <summary>Body of POST /auth/login (api.md section 2).</summary>
public sealed record LoginRequest(string Email, string Password);
