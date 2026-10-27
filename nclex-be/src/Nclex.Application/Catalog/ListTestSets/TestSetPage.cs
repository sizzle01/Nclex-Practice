namespace Nclex.Application.Catalog.ListTestSets;

/// <summary>Body of GET /test-sets: one page of sets, in the cursor format of api.md section 1.</summary>
/// <param name="NextCursor">Pass as <c>cursor</c> to get the next page; null on the last page.</param>
public sealed record TestSetPage(IReadOnlyList<TestSetSummary> Items, string? NextCursor);
