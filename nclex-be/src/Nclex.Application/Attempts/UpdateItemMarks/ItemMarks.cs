namespace Nclex.Application.Attempts.UpdateItemMarks;

/// <summary>The candidate's marks on an item after the change.</summary>
public sealed record ItemMarks(int Position, bool Flagged, IReadOnlyList<string> StruckOptions);
