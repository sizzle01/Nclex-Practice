namespace Nclex.Application.Attempts.GetAttempt;

/// <summary>GET /attempts/{id}: the attempt's state, without item content. Needs no lease.</summary>
public sealed class GetAttemptHandler(AttemptStore store, AttemptStateReader stateReader)
{
    public async Task<AttemptState> HandleAsync(Guid attemptId, CancellationToken cancellationToken)
    {
        var attempt = await store.FindOwnedAsync(attemptId, cancellationToken);
        return await stateReader.ReadAsync(attempt, cancellationToken);
    }
}
