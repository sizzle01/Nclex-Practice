namespace Nclex.Application.Admin.Questions.Review;

/// <summary>Body of approve (notes optional) and reject (notes required).</summary>
/// <param name="Notes">For the author: what was checked, or what to fix.</param>
public sealed record ReviewRequest(string? Notes)
{
    public const int MaxNotesLength = 4000;
}
