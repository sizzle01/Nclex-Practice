using Nclex.Domain.Entities;

namespace Nclex.Application.Attempts.SaveResponse;

/// <summary>What the server holds for the item after the autosave.</summary>
/// <param name="ClientSeq">The stored sequence number; lower than the one sent when the write was stale.</param>
/// <param name="State">The item's state, e.g. answered.</param>
public sealed record SaveResponseResult(DateTimeOffset SavedAt, int ClientSeq, ItemState State, TimerState Timer);
