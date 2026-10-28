using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.GetItem;

/// <summary>
/// GET /attempts/{id}/items/{position}: the item to display, with its reveal once the rules allow it
/// (after Submit, or once the item is locked in tutor mode).
/// </summary>
public sealed class GetItemHandler(AttemptStore store, ItemViewReader items)
{
    public async Task<CandidateItemView> HandleAsync(Guid attemptId, int position, CancellationToken cancellationToken)
    {
        var attempt = await store.FindOwnedAsync(attemptId, cancellationToken);
        switch (attempt.Status)
        {
            case AttemptStatus.Abandoned:
                throw new AppException(AttemptErrors.NotActive);
            case AttemptStatus.Paused:
                // Content is hidden during a pause (PRD 5.4).
                throw new AppException(AttemptErrors.Paused);
        }

        var views = await items.ReadAsync(attempt, [position], cancellationToken);
        return views.SingleOrDefault() ?? throw new AppException(AttemptErrors.ItemNotFound);
    }
}
