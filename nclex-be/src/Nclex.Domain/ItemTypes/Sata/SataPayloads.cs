namespace Nclex.Domain.ItemTypes.Sata;

/// <summary>What candidates see: 5 to 10 options (interfaces.md 2.2).</summary>
public sealed record SataContent(IReadOnlyList<Option> Options);

/// <summary>The correct options; at least one.</summary>
public sealed record SataKey(IReadOnlyList<string> Correct);

/// <summary>The options the candidate selected; empty when none.</summary>
public sealed record SataResponse(IReadOnlyList<string> Selected);
