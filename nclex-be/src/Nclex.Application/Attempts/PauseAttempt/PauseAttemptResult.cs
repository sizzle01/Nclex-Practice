using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.PauseAttempt;

/// <summary>The attempt after pausing: its status and the frozen timers.</summary>
public sealed record PauseAttemptResult(AttemptStatus Status, TimerState Timer);
