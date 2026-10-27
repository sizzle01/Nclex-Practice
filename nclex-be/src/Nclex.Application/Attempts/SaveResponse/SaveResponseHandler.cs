using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Attempts.SaveResponse;

/// <summary>
/// PUT /attempts/{id}/items/{position}/response: the autosave (architecture.md 5.2). Idempotent:
/// a write whose <c>clientSeq</c> is not newer than the stored one changes nothing and still gets 200.
/// </summary>
public sealed class SaveResponseHandler(
    IAppDbContext db,
    AttemptStore store,
    ItemTypeRegistry itemTypes,
    TimeProvider clock)
{
    public async Task<SaveResponseResult> HandleAsync(
        Guid attemptId,
        int position,
        string? clientIdHeader,
        SaveResponseRequest request,
        CancellationToken cancellationToken)
    {
        var clientId = AttemptClient.Require(clientIdHeader);
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        var attempt = await store.LockOwnedAsync(attemptId, cancellationToken);
        AttemptStore.EnsureWritable(attempt, clientId);
        var item = await store.ItemAsync(attempt, position, cancellationToken);

        var response = request.Response is { ValueKind: not JsonValueKind.Null } value ? value : (JsonElement?)null;
        if (response is not null && !item.IsLocked)
        {
            await EnsureValidAsync(item, response.Value, cancellationToken);
        }

        var now = clock.GetUtcNow();
        var outcome = item.SaveResponse(response is null ? null : JsonDocument.Parse(response.Value.GetRawText()), request.ClientSeq, now);
        if (outcome == SaveOutcome.ItemLocked)
        {
            throw new AppException(AttemptErrors.ItemLocked);
        }

        if (outcome == SaveOutcome.Saved)
        {
            attempt.RecordActivity(now);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        return new SaveResponseResult(now, item.ClientSeq, item.State, TimerState.For(attempt, now));
    }

    /// <summary>
    /// Checks the answer against the item (option ids exist, a Select-N answer is not over N...), so
    /// grading in F10 can trust what is stored. Problems are returned as VALIDATION_FAILED.
    /// </summary>
    private async Task EnsureValidAsync(AttemptItem item, JsonElement response, CancellationToken cancellationToken)
    {
        var version = await db.QuestionVersions.AsNoTracking()
            .Where(v => v.Id == item.QuestionVersionId)
            .Select(v => new { v.ItemType, v.Content })
            .SingleAsync(cancellationToken);
        var issues = itemTypes.Get(version.ItemType).ValidateResponse(version.Content.RootElement, response);
        if (issues.Count > 0)
        {
            throw new AppException(
                CommonErrors.ValidationFailed,
                fieldErrors: [.. issues.Select(issue => new AppFieldError(issue.Path, issue.Code))]);
        }
    }
}
