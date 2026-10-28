namespace Nclex.Application.Attempts.StartAttempt;

/// <summary>The attempt to play, and whether it was just created (201) or is an existing draft (200).</summary>
public sealed record StartAttemptResult(AttemptState Attempt, bool Created);
