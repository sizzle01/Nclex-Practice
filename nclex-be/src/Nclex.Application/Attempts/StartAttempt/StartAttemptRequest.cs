namespace Nclex.Application.Attempts.StartAttempt;

/// <summary>Body of POST /attempts (api.md section 4).</summary>
public sealed record StartAttemptRequest(Guid TestSetId);
