namespace Nclex.Domain.ItemTypes.OrderedResponse;

/// <summary>What candidates see: the items to put in order, shuffled per attempt (interfaces.md 2.9).</summary>
public sealed record OrderedResponseContent(IReadOnlyList<Option> Items);

/// <summary>The correct order of the item ids, and how it is scored.</summary>
public sealed record OrderedResponseKey(IReadOnlyList<string> Order, OrderedScoring Scoring);

/// <summary>The candidate's order of the item ids; every item, once each.</summary>
public sealed record OrderedResponseResponse(IReadOnlyList<string> Order);

/// <summary>How an ordered response is scored.</summary>
public enum OrderedScoring
{
    /// <summary>One point for the whole sequence in the right order (the default in interfaces.md 2).</summary>
    AllOrNothing,

    /// <summary>One point for each item in its correct position.</summary>
    PerPosition,
}
