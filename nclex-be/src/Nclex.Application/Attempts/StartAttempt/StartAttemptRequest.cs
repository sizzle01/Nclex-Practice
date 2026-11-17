namespace Nclex.Application.Attempts.StartAttempt;

/// <summary>Body of POST /attempts (api.md section 4).</summary>
/// <param name="Timed">
/// Adaptive exams only (F19): the candidate's choice of a timed or untimed exam. Left out, the
/// set's own setting applies. Sent for any other set, it is rejected.
/// </param>
public sealed record StartAttemptRequest(Guid TestSetId, bool? Timed = null);
