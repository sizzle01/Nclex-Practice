namespace Nclex.Application.Attempts.Navigate;

/// <summary>Where the candidate is now.</summary>
public sealed record NavigateResult(int CurrentPosition, TimerState Timer);
