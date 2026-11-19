namespace Nclex.Domain.ItemTypes.Bowtie;

/// <summary>
/// What candidates see in a bowtie item (interfaces.md 2.x, bowtie): three lists of options. The
/// candidate drags one condition into the centre of the bowtie, two actions to take into the left
/// wing and two parameters to monitor into the right wing.
/// </summary>
/// <param name="Condition">The conditions the client may be experiencing; the candidate picks 1.</param>
/// <param name="Actions">The actions the nurse could take; the candidate picks 2.</param>
/// <param name="Parameters">The parameters the nurse could monitor; the candidate picks 2.</param>
public sealed record BowtieContent(IReadOnlyList<Option> Condition, IReadOnlyList<Option> Actions, IReadOnlyList<Option> Parameters);

/// <summary>The correct condition, the two correct actions and the two correct parameters.</summary>
public sealed record BowtieKey(string Condition, IReadOnlyList<string> Actions, IReadOnlyList<string> Parameters);

/// <summary>
/// What the candidate placed in the bowtie. Any part may be empty while the candidate is still
/// working, so a half-filled bowtie can be saved and earns the points of the slots that are right.
/// </summary>
public sealed record BowtieResponse(string? Condition, IReadOnlyList<string> Actions, IReadOnlyList<string> Parameters);
