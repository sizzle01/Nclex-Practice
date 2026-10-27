namespace Nclex.Application.Attempts.UpdateItemMarks;

/// <summary>Body of PATCH /attempts/{id}/items/{position}; a field left out is not changed.</summary>
/// <param name="Flagged">Mark for review.</param>
/// <param name="StruckOptions">The full list of option ids struck through.</param>
public sealed record UpdateItemMarksRequest(bool? Flagged = null, IReadOnlyList<string>? StruckOptions = null)
{
    public const int MaxStruckOptions = 50;
    public const int MaxOptionIdLength = 64;
}
