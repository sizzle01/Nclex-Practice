using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Catalog.CustomSets;

/// <summary>
/// The builder's filters, shared by the live count and set creation. An empty list means "any".
/// </summary>
public interface IPoolFilters
{
    /// <summary>RN or PN; the user's own track when left out.</summary>
    ExamTrack? Track { get; }

    /// <summary>Client Needs leaf or parent ids; a parent stands for all its leaves.</summary>
    IReadOnlyList<short>? ClientNeedsIds { get; }

    IReadOnlyList<short>? BodySystemIds { get; }

    IReadOnlyList<ItemType>? ItemTypes { get; }

    IReadOnlyList<DifficultyTier>? Difficulties { get; }

    /// <summary>all (default), unused, incorrect or flagged.</summary>
    PoolMode? Pool { get; }
}

/// <summary>Query string of GET /test-sets/custom/available-count; lists repeat, e.g. clientNeedsIds=2&amp;clientNeedsIds=7.</summary>
public sealed record AvailableCountRequest(
    ExamTrack? Track = null,
    IReadOnlyList<short>? ClientNeedsIds = null,
    IReadOnlyList<short>? BodySystemIds = null,
    IReadOnlyList<ItemType>? ItemTypes = null,
    IReadOnlyList<DifficultyTier>? Difficulties = null,
    PoolMode? Pool = null) : IPoolFilters;

/// <summary>Body of GET /test-sets/custom/available-count.</summary>
/// <param name="Available">How many questions match the filters right now.</param>
public sealed record AvailableCountResponse(int Available);
