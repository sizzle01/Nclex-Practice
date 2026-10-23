namespace Nclex.Api.Health;

/// <summary>Health of the API or of one dependency. Serialized as snake_case strings ("ok", "degraded", "unhealthy").</summary>
public enum HealthState
{
    Ok,
    Degraded,
    Unhealthy,
}

/// <summary>Body of GET /healthz.</summary>
public sealed record LivenessResponse(HealthState Status);

/// <summary>Body of GET /readyz: the overall state plus the state of each dependency.</summary>
public sealed record ReadinessResponse(HealthState Status, ReadinessChecks Checks);

public sealed record ReadinessChecks(HealthState Postgres, HealthState Redis);
