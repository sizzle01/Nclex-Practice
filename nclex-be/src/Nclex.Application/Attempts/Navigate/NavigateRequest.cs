namespace Nclex.Application.Attempts.Navigate;

/// <summary>Body of POST /attempts/{id}/navigate (api.md section 4).</summary>
public sealed record NavigateRequest(int ToPosition);
