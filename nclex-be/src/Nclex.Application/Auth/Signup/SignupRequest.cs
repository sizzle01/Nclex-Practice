using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.Signup;

/// <summary>Body of POST /auth/signup (api.md section 2).</summary>
/// <param name="Email">Login email.</param>
/// <param name="Password">10 to 128 characters.</param>
/// <param name="Track">RN or PN. Nullable only so a missing value is reported as REQUIRED.</param>
/// <param name="DisplayName">Optional name shown in the app.</param>
public sealed record SignupRequest(string Email, string Password, ExamTrack? Track, string? DisplayName = null);
