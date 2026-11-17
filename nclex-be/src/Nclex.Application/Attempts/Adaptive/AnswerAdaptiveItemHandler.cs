using Nclex.Application.Abstractions;
using Nclex.Application.Attempts.Grading;
using Nclex.Application.Catalog;
using Nclex.Application.Common;
using Nclex.Domain.Cat;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.Adaptive;

/// <summary>
/// POST /attempts/{id}/items/{position}/answer: the adaptive exam's "Next" (architecture.md 5.8).
/// In one transaction it locks and grades the answer, re-estimates ability, and then either stops
/// the exam (a clear result, the maximum, or no questions left) or adds the next question.
/// </summary>
public sealed class AnswerAdaptiveItemHandler(
    IAppDbContext db,
    AttemptStore store,
    AttemptWriteGuard guard,
    AttemptFinalizer finalizer,
    ResponseValidator validator,
    IGradingService grading,
    AdaptivePicker picker,
    PlanTierGuard planTierGuard,
    AttemptStateReader stateReader,
    TimeProvider clock)
{
    public async Task<AnswerAdaptiveItemResult> HandleAsync(
        Guid attemptId,
        int position,
        string? clientIdHeader,
        AnswerAdaptiveItemRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        // Past the deadline this finalizes the attempt as time_expired and fails with
        // ATTEMPT_TIME_EXPIRED, so an answer sent after time is up does not count.
        var attempt = await guard.LockAndApplyTimersAsync(attemptId, transaction, cancellationToken);
        if (!attempt.IsAdaptive)
        {
            throw new AppException(AttemptErrors.AdaptiveOnly);
        }

        // A retry of the answer that ended the exam: report the ending again rather than fail.
        if (attempt.Status == AttemptStatus.Submitted && attempt.SubmitReason == SubmitReason.CatStop)
        {
            return new AnswerAdaptiveItemResult(Ended: true, await stateReader.ReadAsync(attempt, cancellationToken));
        }

        AttemptStore.EnsureWritable(attempt, clientId);

        // A retry of an answer already taken (its response was lost on the way back): the exam has
        // moved on, so return where it is now instead of answering the new question with it.
        if (position < attempt.CurrentPosition)
        {
            return new AnswerAdaptiveItemResult(Ended: false, await stateReader.ReadAsync(attempt, cancellationToken));
        }

        if (position != attempt.CurrentPosition)
        {
            throw new AppException(AttemptErrors.ItemNotFound);
        }

        var now = clock.GetUtcNow();
        var current = await store.CurrentItemAsync(attempt, cancellationToken);
        var response = await validator.ToStoredAsync(current, request.Response, cancellationToken);
        current.SaveResponse(response, request.ClientSeq!.Value, now);
        attempt.LockAdaptiveAnswer(current, now);
        await grading.GradeItemsAsync([current], cancellationToken);

        var state = attempt.ReadCatState().RecordAnswer(Score(current));
        var stop = StoppingRule.AfterAnswer(state.Config, state.Estimate, state.AnsweredCount);
        CatCandidate? next = null;
        if (stop is null)
        {
            var viewer = await planTierGuard.LoadViewerAsync(cancellationToken);
            next = await picker.NextAsync(attempt.Track, viewer.Plan, state, cancellationToken);
            stop = next is null ? CatStopReason.PoolExhausted : null;
        }

        if (stop is { } reason)
        {
            attempt.UpdateCatState(state.Stop(reason));
            await finalizer.FinalizeAsync(attempt, SubmitReason.CatStop, now, cancellationToken);
        }
        else
        {
            state = state.Give(new CatStateItem(next!.QuestionId, next.Difficulty, next.ClientNeedsId));
            db.AttemptItems.Add(attempt.GiveNextAdaptiveItem(current, next.QuestionVersionId, state, now));
        }

        await guard.CommitAsync(attempt, transaction, cancellationToken);
        return new AnswerAdaptiveItemResult(Ended: stop is not null, await stateReader.ReadAsync(attempt, cancellationToken));
    }

    /// <summary>Share of the item's points earned; partial credit on NGN items counts as a fraction.</summary>
    private static double Score(AttemptItem item) =>
        item.MaxPoints is > 0 ? (double)(item.PointsEarned!.Value / item.MaxPoints.Value) : 0.0;
}
