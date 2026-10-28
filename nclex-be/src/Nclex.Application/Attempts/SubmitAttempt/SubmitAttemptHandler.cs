using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Results;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.SubmitAttempt;

/// <summary>
/// POST /attempts/{id}/submit: finish and grade (architecture.md 5.5). Idempotent: once the
/// attempt is submitted, every call returns the stored result, whichever device sends it.
/// </summary>
public sealed class SubmitAttemptHandler(
    IAppDbContext db,
    AttemptStore store,
    AttemptWriteGuard guard,
    AttemptFinalizer finalizer,
    ResponseValidator validator,
    TimeProvider clock)
{
    public async Task<SubmitAttemptResult> HandleAsync(
        Guid attemptId,
        string? clientIdHeader,
        SubmitAttemptRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        // Past the deadline this finalizes the attempt as time_expired and fails with
        // ATTEMPT_TIME_EXPIRED, like any late write: answers sent after time is up do not count.
        var attempt = await guard.LockAndApplyTimersAsync(attemptId, transaction, cancellationToken);
        if (attempt.Status == AttemptStatus.Submitted)
        {
            return await StoredResultAsync(attempt, cancellationToken);
        }

        if (attempt.Status == AttemptStatus.Paused)
        {
            // Only the sweeper submits a paused attempt (architecture.md 5.3); the candidate resumes first.
            throw new AppException(AttemptErrors.NotActive);
        }

        AttemptStore.EnsureWritable(attempt, clientId);
        var now = clock.GetUtcNow();
        await SaveFinalResponsesAsync(attempt, request.FinalResponses ?? [], now, cancellationToken);
        var result = await finalizer.FinalizeAsync(attempt, SubmitReason.User, now, cancellationToken);
        await guard.CommitAsync(attempt, transaction, cancellationToken);
        return new SubmitAttemptResult(attempt.Id, attempt.Status, SubmitReason.User, AttemptResultSummary.From(result));
    }

    /// <summary>
    /// Applies the unsent answers like autosaves. One for a locked item, or older than the stored
    /// answer, is skipped rather than failing the whole submit.
    /// </summary>
    private async Task SaveFinalResponsesAsync(
        Attempt attempt,
        IReadOnlyList<FinalResponse> responses,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        foreach (var final in responses)
        {
            var item = await store.ItemAsync(attempt, final.Position, cancellationToken);
            var response = await validator.ToStoredAsync(item, final.Response, cancellationToken);
            item.SaveResponse(response, final.ClientSeq, now);
        }
    }

    private async Task<SubmitAttemptResult> StoredResultAsync(Attempt attempt, CancellationToken cancellationToken)
    {
        var result = await db.AttemptResults.AsNoTracking().SingleOrDefaultAsync(r => r.AttemptId == attempt.Id, cancellationToken)
            ?? throw new AppException(AttemptErrors.ResultNotFound);
        return new SubmitAttemptResult(attempt.Id, attempt.Status, attempt.SubmitReason!.Value, AttemptResultSummary.From(result));
    }
}
