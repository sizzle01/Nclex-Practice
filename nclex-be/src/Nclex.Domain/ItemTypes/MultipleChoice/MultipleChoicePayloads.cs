namespace Nclex.Domain.ItemTypes.MultipleChoice;

/// <summary>What candidates see: the options, typically four (interfaces.md 2.1).</summary>
public sealed record MultipleChoiceContent(IReadOnlyList<Option> Options);

/// <summary>The one correct option.</summary>
public sealed record MultipleChoiceKey(string Correct);

/// <summary>The option the candidate chose; null when they have not chosen one.</summary>
public sealed record MultipleChoiceResponse(string? Selected);
